using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using System;
using Fixit.Chat.Management.Lib.Schemas;
using Fixit.Core.DataContracts.Chat.Operations.Messages;
using Fixit.Chat.Management.Lib.Schemas.Enums;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using Fixit.Chat.Management.Triggers;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Core.Storage.Storage;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Core.Networking.Local.NMS;
using System.Collections.Generic;

namespace Empower.Chat.Management.Triggers.ServiceBus.Conversations
{
  public class OnConversationActionDispatcher : TriggersBase
  {
    public OnConversationActionDispatcher(IConfiguration configurationProvider,
                                          IConversationMessageMediator conversationMessageMediator,
                                          IConversationTriggersMediator conversationTriggersMediator,
                                          IStorageFactory storageFactory,
                                          IServiceBusMessagingClientMediator serviceBusMessagingClientMediator,
                                          IFixNmsHttpClient fixNmsHttpClient) : base(configurationProvider, conversationMessageMediator, conversationTriggersMediator, storageFactory, serviceBusMessagingClientMediator, fixNmsHttpClient) { }

    [FunctionName(nameof(OnConversationActionDispatcher))]
    public async Task RunAsync([ServiceBusTrigger("onconversationactiondispatcher", Connection = "EMP-CHMS-SB-CS", IsSessionsEnabled = true)] ServiceBusReceivedMessage[] serviceBusReceivedMessages,
                               CancellationToken cancellationToken)
    {
      if (serviceBusReceivedMessages is { } && serviceBusReceivedMessages.Any())
      {
        foreach(var serviceBusReceivedMessage in serviceBusReceivedMessages)
        {
          var queriesConversationsResponse = await  _conversationTriggersMediator.GetConversationsByQueryAsync(null, new ConversationQueryDto() { Ids = new List<Guid>() { Guid.Parse(serviceBusReceivedMessage.SessionId) } }, cancellationToken);
          if (queriesConversationsResponse.IsOperationSuccessful && queriesConversationsResponse.Result is { } && queriesConversationsResponse.Result.Any())
          {
            var currentConversation = queriesConversationsResponse.Result.First(); 
            var serviceBusMessage = new ServiceBusMessage()
            {
              SessionId = serviceBusReceivedMessage.SessionId,
            };

            var onConversationDispatchedAction = JsonConvert.DeserializeObject<OnConversationDispatchedAction>(Encoding.UTF8.GetString(serviceBusReceivedMessage.Body));
            switch (onConversationDispatchedAction.Action)
            {
              case OnConversationDispatchedActions.AddParticipantsToConversation: 
              {
                var addParticipantToChatMessage = JsonConvert.DeserializeObject<AddParticipantToChatMessage>(JsonConvert.SerializeObject(onConversationDispatchedAction.ActionPayload));
                addParticipantToChatMessage.Conversation = currentConversation;
                serviceBusMessage.Body = new BinaryData(addParticipantToChatMessage);

                await this._serviceBusMessagingClientMediator.SendMessageAsync(_joinGroupQueue, serviceBusMessage, cancellationToken); break; 
              };

              case OnConversationDispatchedActions.AddMessagesToConversation : 
              {
                var chatMessageGroupSendMessage = JsonConvert.DeserializeObject<ChatMessageGroupSendMessage>(JsonConvert.SerializeObject(onConversationDispatchedAction.ActionPayload));
                chatMessageGroupSendMessage.Conversation = currentConversation;
                serviceBusMessage.Body = new BinaryData(chatMessageGroupSendMessage);

                await this._serviceBusMessagingClientMediator.SendMessageAsync(_sendMessageToGroup, serviceBusMessage, cancellationToken);
                await _conversationTriggersMediator.EnqueueNotificationForConversationLastMessageAsync(chatMessageGroupSendMessage, cancellationToken);
                break; 
              };

              default: throw new InvalidOperationException();
            };
          }
        }
      }
    }
  }
}
