using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Helpers;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.Networking.Local.NMS;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Fixit.Chat.Management.Lib.UnitTests")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.ServerlessApi")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.Triggers")]
namespace Fixit.Chat.Management.Lib.Mediators.Internal
{
  internal class ConversationsMediator : IConversationsMediator
  {
    private readonly IDatabaseTableEntityMediator _databaseConversationsTable;
    private readonly IChatNotificationFactory _chatNotificationFactory;
    private readonly IFixNmsHttpClient _nmsHttpClient;

    public ConversationsMediator(IDatabaseMediator databaseMediator,
                                 IChatNotificationFactory chatNotificationFactory,
                                 IFixNmsHttpClient nmsHttpClient,
                                 IConfiguration configurationProvider)
    {
      var databaseName = configurationProvider["FIXIT-CM-DB-NAME"];
      var databaseConversationsTableName = configurationProvider["FIXIT-CM-DB-CONVERSATIONSTABLE"];

      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects the {nameof(configurationProvider)} to have defined the Chat Management Database as {{FIXIT-CM-DB-NAME}} ");
      }

      if (string.IsNullOrWhiteSpace(databaseConversationsTableName))
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects the {nameof(configurationProvider)} to have defined the Chat Management Conversations Table as {{FIXIT-CM-DB-CONVERSATIONSTABLE}} ");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _databaseConversationsTable = databaseMediator.GetDatabase(databaseName).GetContainer(databaseConversationsTableName);
      _chatNotificationFactory = chatNotificationFactory ?? throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(chatNotificationFactory)}... null argument was provided");
      _nmsHttpClient = nmsHttpClient ?? throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(nmsHttpClient)}... null argument was provided");
    }

    public ConversationsMediator(IDatabaseMediator databaseMediator,
                                 IChatNotificationFactory chatNotificationFactory,
                                 IFixNmsHttpClient nmsHttpClient,
                                 string databaseName,
                                 string conversationsTableName)
    {
      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(databaseName)}... null argument was provided");
      }

      if (string.IsNullOrWhiteSpace(conversationsTableName))
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(conversationsTableName)}... null argument was provided");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _databaseConversationsTable = databaseMediator.GetDatabase(databaseName).GetContainer(conversationsTableName);
      _chatNotificationFactory = chatNotificationFactory ?? throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(chatNotificationFactory)}... null argument was provided");
      _nmsHttpClient = nmsHttpClient ?? throw new ArgumentNullException($"{nameof(ConversationsMediator)} expects a value for {nameof(nmsHttpClient)}... null argument was provided");
    }

    #region ServerlessApi
    public async Task<DocumentCollectionDto<ConversationDocument>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var (conversationDocumentCollection, token) = await _databaseConversationsTable.GetItemQueryableAsync<ConversationDocument>(null, cancellationToken, conversationDocument => conversationDocument.Participants.Count(participant => participant.User.Id == userId) == 1);
      return conversationDocumentCollection;
    }
    #endregion

    #region Triggers
    public async Task CreateConversationAsync(ConversationCreateRequestDto conversationCreateRequestDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      ConversationDocument conversationDocument = new ConversationDocument() { FixInstanceId = conversationCreateRequestDto.FixInstanceId };
      var currentTime = DateTimeOffset.UtcNow;
      conversationDocument.CreatedTimestampsUtc = currentTime.ToUnixTimeSeconds();
      conversationDocument.Participants = conversationCreateRequestDto.Participants.Select(user => new ParticipantDto() { User = user, CreatedTimestampsUtc = currentTime.ToUnixTimeSeconds() })
                                                                                   .ToList();

      var result = await _databaseConversationsTable.CreateItemAsync(conversationDocument, currentTime.ToString("yyyy-MM"), cancellationToken);

      if (result.IsOperationSuccessful)
      {
        var notificationDtoList = new List<EnqueueNotificationRequestDto>();
        notificationDtoList.Add(_chatNotificationFactory.CreateClientConversationNotificationDto(result.Document));
        notificationDtoList.Add(_chatNotificationFactory.CreateCraftsmanConversationNotificationDto(result.Document));

        foreach (EnqueueNotificationRequestDto notificationDto in notificationDtoList)
        {
          await _nmsHttpClient.PostNotification(notificationDto, cancellationToken);
        }
      }
    }
    #endregion
  }
}
