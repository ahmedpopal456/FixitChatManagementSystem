using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.DataContracts.Chat.Enums;
using Fixit.Core.DataContracts.Users;
using Fixit.Core.Storage.Queue.Mediators;

namespace Fixit.Chat.Management.Triggers.Functions.SignalR
{
  public class FixitChatHub : ServerlessHub
  {
    private const string _newMessageChannel = "newMessage";

    private readonly IQueueClientMediator _queueClientMediator;

    public FixitChatHub(IQueueServiceClientMediator queueServiceClientMediator, IConfiguration configurationProvider)
    {
      var newMessagesQueueName = configurationProvider["FIXIT-CMS-MESSAGESQUEUE-NAME"];

      if (queueServiceClientMediator == null)
      {
        throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a value for {nameof(queueServiceClientMediator)}... null argument was provided");
      }

      if (string.IsNullOrWhiteSpace(newMessagesQueueName))
      {
        throw new ArgumentNullException($"{nameof(FixitChatHub)} expects the {nameof(configurationProvider)} to have defined the Chat Management Database as {{FIXIT-CMS-MESSAGESQUEUE-NAME}} ");
      }

      _queueClientMediator = queueServiceClientMediator.GetQueueClient(newMessagesQueueName) ?? throw new ArgumentNullException($"{nameof(FixitChatHub)} expects a valid value for {nameof(newMessagesQueueName)}, {newMessagesQueueName} does not exist");
    }

    [FunctionName(nameof(Negotiate))]
    public async Task<SignalRConnectionInfo> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
    {
      var options = new NegotiationOptions() { UserId = req.Headers["x-ms-signalr-user-id"] };
      return await NegotiateAsync(options);
    }

    [FunctionName(nameof(SendToUser))]
    public async Task SendToUser([SignalRTrigger] InvocationContext invocationContext, string conversationId, UserSummaryDto sender, UserSummaryDto recipient, string message)
    {
      var newMessage = new UserMessageCreateRequestDto() {
        ConversationId = new Guid(conversationId),
        Recipient = recipient,
        Message = new MessageDto()
        {
          Id = Guid.NewGuid(),
          Message = message,
          CreatedTimestampsUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
          UpdatedTimestampsUtc = 0,
          Type = MessageType.Text,
          CreatedByUser = sender
        }
      };
      await Clients.Users(recipient.Id.ToString(), invocationContext.UserId).SendAsync(_newMessageChannel, newMessage);

      string newMessageJson = JsonConvert.SerializeObject(newMessage);
      string base64EncodedNewMessage = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(newMessageJson));
      await _queueClientMediator.SendMessageAsync(base64EncodedNewMessage, TimeSpan.FromSeconds(0), TimeSpan.FromDays(7));
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger] InvocationContext invocationContext)
    {
      // To be handled later
    }

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {
      // To be handled later
    }
  }
}
