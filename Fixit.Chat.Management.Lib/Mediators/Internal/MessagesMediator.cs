using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.Networking.Local.NMS;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Fixit.Chat.Management.Lib.UnitTests")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.ServerlessApi")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.Triggers")]
namespace Fixit.Chat.Management.Lib.Mediators.Internal
{
  internal class MessagesMediator : IMessagesMediator
  {
    private readonly IMapper _mapper;
    private readonly IDatabaseTableEntityMediator _databaseConversationMessagesTable;
    private readonly IFixNmsHttpClient _nmsHttpClient;

    public MessagesMediator(IMapper mapper,
                            IDatabaseMediator databaseMediator,
                            IFixNmsHttpClient nmsHttpClient,
                            IConfiguration configurationProvider)
    {
      var databaseName = configurationProvider["FIXIT-CM-DB-NAME"];
      var databaseMessagesTableName = configurationProvider["FIXIT-CM-DB-MESSAGESTABLE"];

      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects the {nameof(configurationProvider)} to have defined the Chat Management Database as {{FIXIT-CM-DB-NAME}} ");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _mapper = mapper ?? throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(mapper)}... null argument was provided");
      _databaseConversationMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(databaseMessagesTableName);
      _nmsHttpClient = nmsHttpClient ?? throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(nmsHttpClient)}... null argument was provided");
    }

    public MessagesMediator(IMapper mapper,
                            IDatabaseMediator databaseMediator,
                            IFixNmsHttpClient nmsHttpClient,
                            string databaseName,
                            string messagesTableName)
    {
      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseName)}... null argument was provided");
      }

      if (string.IsNullOrWhiteSpace(messagesTableName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(messagesTableName)}... null argument was provided");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _mapper = mapper ?? throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(mapper)}... null argument was provided");
      _databaseConversationMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(messagesTableName);
      _nmsHttpClient = nmsHttpClient ?? throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(nmsHttpClient)}... null argument was provided");
    }

    #region ServerlessApi
    public async Task<GetMessagesResponseDto> GetMessagesAsync(Guid conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      pageNumber = pageNumber == default(int) ? 1 : pageNumber;
      pageSize = pageSize == default(int) ? 100 : pageSize;

      var messagesResult = default(GetMessagesResponseDto);

      var (messageDocumentCollection, token) = await _databaseConversationMessagesTable.GetItemQueryableAsync<ConversationMessagesDocument>(null, cancellationToken, MessageDocument => MessageDocument.ConversationId == conversationId);

      messagesResult = new GetMessagesResponseDto()
      {
        IsOperationSuccessful = messageDocumentCollection.IsOperationSuccessful,
        OperationException = messageDocumentCollection.OperationException,
        OperationMessage = messageDocumentCollection.OperationMessage
      };

      if (messageDocumentCollection.IsOperationSuccessful)
      {
        ConversationMessagesDocument messageDocument = messageDocumentCollection.Results.SingleOrDefault();
        messagesResult.Messages = messageDocument.Messages.TakeLast(pageSize * pageNumber)
                                                          .SkipLast(pageSize * (pageNumber - 1))
                                                          .ToList();
      }
      return messagesResult;
    }
    #endregion

    #region Triggers
    public async Task HandleMessageAsync(UserMessageCreateRequestDto userMessageCreateRequestDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      OperationStatus result;

      var (messageDocumentCollection, token) = await _databaseConversationMessagesTable.GetItemQueryableAsync<ConversationMessagesDocument>(null, cancellationToken, messageDocument => messageDocument.ConversationId == userMessageCreateRequestDto.ConversationId);
      if (messageDocumentCollection.IsOperationSuccessful)
      {
        ConversationMessagesDocument messageDocument = messageDocumentCollection.Results.SingleOrDefault();
        var currentTime = DateTimeOffset.UtcNow;

        messageDocument ??= new ConversationMessagesDocument()
        {
          ConversationId = userMessageCreateRequestDto.ConversationId,
        };

        messageDocument.Messages.Add(userMessageCreateRequestDto.Message);
        result = await _databaseConversationMessagesTable.UpsertItemAsync(messageDocument, messageDocument.EntityId ?? currentTime.ToString("yyyy-MM"), cancellationToken);

        if (result.IsOperationSuccessful)
        {
          EnqueueNotificationRequestDto notificationDto = _mapper.Map<UserMessageCreateRequestDto, EnqueueNotificationRequestDto>(userMessageCreateRequestDto);
          await _nmsHttpClient.PostNotification(notificationDto, cancellationToken);
        }
      }
    }
    #endregion
  }
}
