using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Chat.Management.Lib.Constants;

namespace Fixit.Chat.Management.Lib.Mediators.Conversations.Messages
{
  // Add Summaries
  public interface IConversationMessageMediator
  {
    Task<OperationStatusWithObject<ConversationMessageDto>> AddConversationMessageAsync(Guid conversationId, ConversationMessageUpsertRequestDto messageCreateRequest, long sentTimestampUtc, CancellationToken cancellationToken = default);

    Task<SegmentedDocumentCollectionDto<ConversationMessageDto>> GetSegmentedConversationMessagesAsync(Guid conversationId, int segmentSize = PagedDocumentsAttributes.DefaultPageSize, TableContinuationToken? tableContinuationToken = null, CancellationToken cancellationToken = default);

    Task<OperationStatus> UpdateConversationMessageAsync(Guid conversationId, long messageId, Guid requestedByUserId, ConversationMessageUpsertRequestDto messageUpdateRequest, CancellationToken cancellationToken = default);

    Task<IList<OperationStatusWithObject<long>>> DeleteConversationMessageByBulkIdsAsync(Guid conversationId, IEnumerable<long> messageIds, CancellationToken cancellationToken = default);
  }
}