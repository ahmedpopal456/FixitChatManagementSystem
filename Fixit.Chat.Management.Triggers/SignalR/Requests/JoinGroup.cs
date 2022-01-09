using Azure.Messaging.ServiceBus;
using Fixit.Core.DataContracts.Chat.Operations.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using Fixit.Core.DataContracts.Users;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(JoinGroup))]
    public async Task JoinGroup([SignalRTrigger] InvocationContext invocationContext, string conversationId, UserSummaryDto user)
    {
      var cancellationToken = new CancellationTokenSource().Token;
      var parsedConversationId = Guid.TryParse(conversationId, out Guid result) ? result : throw new ArgumentException($"{conversationId} is not a valid {nameof(Guid)}");

      var conversations = await _conversationTriggersMediator.GetConversationsByQueryAsync(null, new ConversationQueryDto() { Ids = new List<Guid>() { Guid.Parse(conversationId) } }, cancellationToken);
      var conversation = conversations.Result.FirstOrDefault();

      if (conversation is { })
      {
        var existingParticipant = conversation.Participants?.FirstOrDefault(participant => participant.User.Id.ToString() == invocationContext.UserId);
        if (existingParticipant is null)
        {
          var insertMessageInQueueResponse = await _serviceBusMessagingClientMediator.SendMessageAsync(_joinGroupQueue, new ServiceBusMessage()
          {
            SessionId = conversationId,
            Body = new BinaryData(new AddParticipantToChatMessage()
            {
              ConversationId = Guid.Parse(conversationId),
              User = user,
            }),
          }, cancellationToken);

          if (!insertMessageInQueueResponse.IsOperationSuccessful)
          {
            throw new ApplicationException($"Queueing Message to add a participant {invocationContext.UserId} to group {conversationId} failed with error: {insertMessageInQueueResponse.OperationException}");
          }
        }

        await UserGroups.AddToGroupAsync(invocationContext.UserId, conversationId);
      }
    }
  }
}
