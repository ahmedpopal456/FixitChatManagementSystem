using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Conversations;

namespace Fixit.Chat.Management.Triggers.Mediators
{
  public interface IConversationTriggersMediator : IConversationBaseMediator
  {
    /// <summary>
    /// Add a list of participants to a conversation
    /// </summary>
    /// <param name="addConversationParticipantsRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OperationStatusWithObject<ConversationDto>> AddConversationParticipantsByBulkIdAsync(AddConversationParticipantsRequestDto addConversationParticipantsRequestDto, CancellationToken cancellationToken);

    /// <summary>
    /// Update a conversation's last message
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="lastMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OperationStatusWithObject<ConversationDto>> UpdateConversationLastMessageAsync(Guid conversationId, ConversationMessageDto lastMessage, CancellationToken cancellationToken);
  }
}