using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Enums;

namespace Fixit.Chat.Management.Lib.Mediators.Conversations
{
  // Add Summaries
  public interface IConversationBaseMediator
  {
    Task<OperationStatusWithObject<ConversationDto>> CreateConversationAsync(ConversationCreateRequestDto conversationCreateRequestDto, CancellationToken cancellationToken);

    Task<OperationStatusWithObject<IEnumerable<ConversationDto>>> GetConversationsByQueryAsync(string searchString, ConversationQueryDto conversationQueryDto, CancellationToken cancellationToken, string orderField = null, OrderDirections? orderDirection = OrderDirections.Asc);

    Task<OperationStatusWithObject<ConversationDto>> UpdateConversationParticipantsReadStatusByBulkIdAsync(Guid conversationId, IEnumerable<ParticipantReadStatusUpdateRequestDto> participantReadStatusUpdateRequestDtos, CancellationToken cancellationToken);

    Task<OperationStatus> DeleteConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken);
  }
}
