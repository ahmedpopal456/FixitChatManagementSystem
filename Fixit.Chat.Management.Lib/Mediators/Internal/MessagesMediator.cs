using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Fixit.Chat.Management.Lib.UnitTests")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.Triggers")]
namespace Fixit.Chat.Management.Lib.Mediators.Internal
{
  internal class MessagesMediator : IMessagesMediator
  {
    private readonly IDatabaseTableEntityMediator _databaseConversationMessagesTable;

    public MessagesMediator(IDatabaseMediator databaseMediator,
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

      _databaseConversationMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(databaseMessagesTableName);
    }

    public MessagesMediator(IDatabaseMediator databaseMediator,
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

      _databaseConversationMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(messagesTableName);
    }

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
          // TODO: (#503) Notify receivers
        }
      }
    }
  }
}
