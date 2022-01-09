using AutoMapper;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Core.Networking.Local.NMS;
using Fixit.Core.Storage.Storage;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub : ServerlessHub
  {
    private readonly IConversationMessageMediator _conversationMessageMediator;
    private readonly IConversationTriggersMediator _conversationTriggersMediator;
    private readonly IFixNmsHttpClient _fixNmsHttpClient;
    private readonly IMapper _mapper;
    private readonly ILogger<FixitChatHub> _logger;

    private readonly string _joinGroupQueue;
    private readonly string _sendMessageToGroup;
    private readonly string _broadcastUserTypingActivity = "broadcastusertypingactivity";

    private readonly IServiceBusMessagingClientMediator _serviceBusMessagingClientMediator;

    public FixitChatHub(IConfiguration configurationProvider,
                        IConversationTriggersMediator conversationTriggersMediator,
                        IConversationMessageMediator conversationMessageMediator,
                        IStorageFactory storageFactory,
                        IServiceBusMessagingClientMediator serviceBusMessagingClientMediator,
                        IMapper mapper,
                        IFixNmsHttpClient fixNmsHttpClient,
                        ILogger<FixitChatHub> logger)

    {
      _ = configurationProvider ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(configurationProvider)}... null argument was provided");
      _ = storageFactory ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(storageFactory)}... null argument was provided");

      _mapper = mapper ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(mapper)}... null argument was provided");
      _logger = logger ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(logger)}... null argument was provided");
      _joinGroupQueue = string.IsNullOrWhiteSpace(configurationProvider["FIXIT-CHMS-JOINGROUP-QUEUE-NAME"]) ? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects the {nameof(configurationProvider)} to have defined the Chat Join Group Queue Name as {{FIXIT-CHMS-JOINGROUP-QUEUE-NAME}}") : configurationProvider["FIXIT-CHMS-JOINGROUP-QUEUE-NAME"];
      _sendMessageToGroup = string.IsNullOrWhiteSpace(configurationProvider["FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME"]) ? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects the {nameof(configurationProvider)} to have defined the Chat Join Group Queue Name as {{FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME}}") : configurationProvider["FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME"];
      _conversationMessageMediator = conversationMessageMediator ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(conversationMessageMediator)}... Null argument was provided");
      _conversationTriggersMediator = conversationTriggersMediator ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(conversationTriggersMediator)}... Null argument was provided");
      _serviceBusMessagingClientMediator = serviceBusMessagingClientMediator ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(serviceBusMessagingClientMediator)}... Null argument was provided");
      _fixNmsHttpClient = fixNmsHttpClient ?? throw new ArgumentNullException($"{nameof(TriggersBase)} expects a value for {nameof(fixNmsHttpClient)}... Null argument was provided");
    }
  }
}
