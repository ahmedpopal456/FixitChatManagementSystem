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
using Fixit.Chat.Management.Lib.Schemas;
using Fixit.Chat.Management.Lib.Schemas.Enums;

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
      conversationMessageCreateRequestDto.Message = string.IsNullOrWhiteSpace(conversationMessageCreateRequestDto.Message) ? null : conversationMessageCreateRequestDto.Message;
      var onConversationDispatchedAction = new OnConversationDispatchedAction()
      {
        Action = OnConversationDispatchedActions.AddMessagesToConversation,
        ActionPayload = new ChatMessageGroupSendMessage()
        {
          ConversationId = conversationId,
          MessageCreateRequest = conversationMessageCreateRequestDto,
          SentByUserId = Guid.Parse(invocationContext.UserId),
          SentTimestampUtc = sentTimestampUtc
        }
      };

      var serviceBusMessage = new ServiceBusMessage()
      {
        SessionId = conversationId.ToString(),
        Body = new BinaryData(onConversationDispatchedAction),
      };

      var insertMessageInQueueResponse = await _serviceBusMessagingClientMediator.SendMessageAsync(_onconversationactiondispatcher, serviceBusMessage, cancellationToken);
      if (insertMessageInQueueResponse.IsOperationSuccessful)
      {
        var bouncedMessage = _mapper.Map<ConversationMessageUpsertRequestDto, ConversationMessageDto>(conversationMessageCreateRequestDto);
        bouncedMessage.Id = (DateTime.MaxValue.Ticks - sentTimestampUtc).ToString();
        bouncedMessage.UpdatedTimestampUtc = bouncedMessage.CreatedTimestampUtc = sentTimestampUtc;

        await Clients.Group(conversationId.ToString()).SendAsync(conversationId.ToString(), bouncedMessage);
      }
    }
  }
}
