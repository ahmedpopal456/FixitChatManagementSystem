using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Fixit.Core.DataContracts.Chat.Operations.Messages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fixit.Core.DataContracts.Chat.Messages;
using Newtonsoft.Json;
using System.Text;
using Fixit.Core.Storage.Storage;
using Fixit.Core.DataContracts;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Core.Networking.Local.NMS;

namespace Fixit.Chat.Management.Triggers.ServiceBus.Conversations.Messages
{
  public class OnChatMessageGroupSentPersist : TriggersBase
  {
    public OnChatMessageGroupSentPersist(IConfiguration configurationProvider,
                                         IConversationMessageMediator conversationMessageMediator,
                                         IConversationTriggersMediator conversationTriggersMediator,
                                         IStorageFactory storageFactory,
                                         IFixNmsHttpClient fixNmsHttpClient,
                                         IServiceBusMessagingClientMediator serviceBusMessagingClientMediator) : base(configurationProvider, conversationMessageMediator, conversationTriggersMediator, storageFactory, serviceBusMessagingClientMediator, fixNmsHttpClient) { }

    [FunctionName(nameof(OnChatMessageGroupSentPersist))]
    public async Task RunAsync([ServiceBusTrigger("sendmessagetogroupqueue", Connection = "FIXIT-CHMS-SB-CS", IsSessionsEnabled = true)] ServiceBusReceivedMessage[] messages,
                               CancellationToken cancellationToken)
    {
      var results = new List<OperationStatusWithObject<ConversationMessageDto>>();
      var chatGroupMessages = messages.Select(message => JsonConvert.DeserializeObject<ChatMessageGroupSendMessage>(Encoding.UTF8.GetString(message.Body)));

      if (chatGroupMessages is { } && chatGroupMessages.Any())
      {
        var conversationId = chatGroupMessages.First()?.ConversationId;

        List<Task> addMessagesToConversation = new List<Task>();
        foreach (var chatGroupMessage in chatGroupMessages)
        {
          addMessagesToConversation.Add(Task.Run(async () =>
          {
            var response = await _conversationMessageMediator.AddConversationMessageAsync(chatGroupMessage.ConversationId, chatGroupMessage.MessageCreateRequest, chatGroupMessage.SentTimestampUtc, cancellationToken);
            results.Add(response);
          }));
        }
        await Task.WhenAll(addMessagesToConversation);

        var storedMessages = results.Where(item => item.IsOperationSuccessful).Select(item => item.Result)?.ToList();
        if (storedMessages is { })
        {
          storedMessages.Sort((x, y) => x.CreatedTimestampUtc.CompareTo(y.CreatedTimestampUtc));
          
          var lastSuccessfullyStoredMessage = storedMessages.Last();
          await _conversationTriggersMediator.UpdateConversationLastMessageAsync(conversationId.GetValueOrDefault(), lastSuccessfullyStoredMessage, cancellationToken);
        }
      }
    }
  }
}
