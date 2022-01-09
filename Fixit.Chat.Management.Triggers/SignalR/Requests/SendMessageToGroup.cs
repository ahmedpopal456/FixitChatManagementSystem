using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts.Chat.Operations.Messages;
using Azure.Messaging.ServiceBus;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.DataContracts.Notifications.Payloads;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(SendMessageToGroup))]
    public async Task SendMessageToGroup([SignalRTrigger] InvocationContext invocationContext,
                                         Guid conversationId,
                                         ConversationMessageUpsertRequestDto conversationMessageCreateRequestDto)
    {
      var cancellationToken = new CancellationTokenSource().Token;
      if (!conversationMessageCreateRequestDto.Validate())
      {
        throw new InvalidOperationException($"{nameof(conversationMessageCreateRequestDto)} is invalid...");
      }

      var sentTimestampUtc = EpochHelper.GetTimestampMilliSecondsUtcNow();
      var chatMessageGroupSendMessage = new ChatMessageGroupSendMessage()
      {
        ConversationId = conversationId,
        MessageCreateRequest = conversationMessageCreateRequestDto,
        SentByUserId = Guid.Parse(invocationContext.UserId),
        SentTimestampUtc = sentTimestampUtc
      };
      var serviceBusMessage = new ServiceBusMessage()
      {
        SessionId = conversationId.ToString(),
        Body = new BinaryData(chatMessageGroupSendMessage),
      };

      var insertMessageInQueueResponse = await _serviceBusMessagingClientMediator.SendMessageAsync(_sendMessageToGroup, serviceBusMessage, cancellationToken);
      if (insertMessageInQueueResponse.IsOperationSuccessful)
      {
        var bouncedMessage = _mapper.Map<ConversationMessageUpsertRequestDto, ConversationMessageDto>(conversationMessageCreateRequestDto);
        bouncedMessage.UpdatedTimestampUtc = bouncedMessage.CreatedTimestampUtc = sentTimestampUtc;
        
        await Clients.Group(conversationId.ToString()).SendAsync(conversationId.ToString(), bouncedMessage);

        await _fixNmsHttpClient.PostNotification(new EnqueueNotificationRequestDto()
        {
          Action = Core.DataContracts.Notifications.Enums.NotificationTypes.NewMessage,
          Payload = new ConversationMessagePayloadDto()
          {
            Id = bouncedMessage.Id.ToString(),
            SentByUser = bouncedMessage.CreatedByUser,
            Message = bouncedMessage.Message,
          }
        }, cancellationToken);
      }
    }
  }
}
