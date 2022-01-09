using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Core.Networking.Local.NMS;
using Fixit.Core.Storage.Storage;
using Microsoft.Extensions.Configuration;
using System;

namespace Fixit.Chat.Management.Triggers
{
  public class TriggersBase 
  {
    protected readonly IConversationMessageMediator _conversationMessageMediator;
    protected readonly IConversationTriggersMediator _conversationTriggersMediator;
    protected readonly IFixNmsHttpClient _fixNmsHttpClient;

    protected readonly string _joinGroupQueue;
    protected readonly string _sendMessageToGroup;

    protected readonly IServiceBusMessagingClientMediator _serviceBusMessagingClientMediator;

    public TriggersBase(IConfiguration configurationProvider, 
                        IConversationMessageMediator conversationMessageMediator,
                        IConversationTriggersMediator conversationTriggersMediator,
                        IStorageFactory storageFactory,
                        IServiceBusMessagingClientMediator serviceBusMessagingClientMediator,
                        IFixNmsHttpClient fixNmsHttpClient)
    {
      _ = configurationProvider ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(configurationProvider)}... null argument was provided");
      _ = storageFactory ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(storageFactory)}... null argument was provided");

      _joinGroupQueue = string.IsNullOrWhiteSpace(configurationProvider["FIXIT-CHMS-JOINGROUP-QUEUE-NAME"]) ? throw new ArgumentNullException($"{nameof(TriggersBase)} expects the {nameof(configurationProvider)} to have defined the Chat Join Group Queue Name as {{FIXIT-CHMS-JOINGROUP-QUEUE-NAME}}") : configurationProvider["FIXIT-CHMS-JOINGROUP-QUEUE-NAME"];
      _sendMessageToGroup = string.IsNullOrWhiteSpace(configurationProvider["FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME"]) ? throw new ArgumentNullException($"{nameof(TriggersBase)} expects the {nameof(configurationProvider)} to have defined the Chat Join Group Queue Name as {{FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME}}") : configurationProvider["FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME"];
      _conversationMessageMediator = conversationMessageMediator ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(conversationMessageMediator)}... Null argument was provided");
      _conversationTriggersMediator = conversationTriggersMediator ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(conversationTriggersMediator)}... Null argument was provided");
      _serviceBusMessagingClientMediator = serviceBusMessagingClientMediator ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(serviceBusMessagingClientMediator)}... Null argument was provided");
      _fixNmsHttpClient = fixNmsHttpClient ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(fixNmsHttpClient)}... Null argument was provided");
    }
  }
}
