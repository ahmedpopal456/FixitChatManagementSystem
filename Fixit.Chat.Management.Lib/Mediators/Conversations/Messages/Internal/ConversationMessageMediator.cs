#nullable enable
using AutoMapper;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Messages.TableEntities;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using Fixit.Core.Storage.Storage;
using Fixit.Core.Storage.Storage.Table.Managers;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Chat.Management.Lib.Constants;

namespace Fixit.Chat.Management.Lib.Mediators.Conversations.Messages.Internal
{
  internal class ConversationMessageMediator : IConversationMessageMediator
  {
    private readonly IMapper _mapper;
    private readonly ITableServiceClient _tableServiceClient;

    public ConversationMessageMediator(IConfiguration configurationProvider,
                                       IStorageFactory storageFactory,
                                       IMapper mapper)
    {
      _tableServiceClient = storageFactory == null ? throw new ArgumentNullException($"{nameof(ConversationMessageMediator)} expects a value for {nameof(storageFactory)}... null argument was provided") : storageFactory.CreateTableStorageClient();
      _mapper = mapper ?? throw new ArgumentNullException($"{nameof(ConversationMessageMediator)} expects a value for {nameof(mapper)}... null argument was provided");
    }

    public async Task<OperationStatusWithObject<ConversationMessageDto>> AddConversationMessageAsync(Guid conversationId, ConversationMessageUpsertRequestDto messageCreateRequest, long sentTimestampUtc, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var serializedTableName = SerializeTableName(conversationId);
      var conversationTable = await _tableServiceClient.CreateOrGetTableAsync(serializedTableName, cancellationToken);

      var conversationMessageRequestEntity = _mapper.Map<ConversationMessageUpsertRequestDto, TableConversationMessageEntity>(messageCreateRequest);
      conversationMessageRequestEntity.CreatedTimestampUtc = conversationMessageRequestEntity.UpdatedTimestampUtc = sentTimestampUtc;
      conversationMessageRequestEntity.RowKey = (DateTime.MaxValue.Ticks - sentTimestampUtc).ToString();
      conversationMessageRequestEntity.PartitionKey = (DateTime.MaxValue.Ticks - sentTimestampUtc).ToString();

      conversationMessageRequestEntity.CreatedByUser = conversationMessageRequestEntity.UpdatedByUser = JsonConvert.SerializeObject(messageCreateRequest.SentByUser);

      var response = await conversationTable.InsertOrReplaceEntityAsync(conversationMessageRequestEntity, cancellationToken);
      var result = _mapper.Map<OperationStatus, OperationStatusWithObject<ConversationMessageDto>>(response);
      result.Result = _mapper.Map<TableConversationMessageEntity, ConversationMessageDto>(conversationMessageRequestEntity);
      return result;
    }

    public async Task<IList<OperationStatusWithObject<long>>> DeleteConversationMessageByBulkIdsAsync(Guid conversationId, IEnumerable<long> messageIds, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = new List<OperationStatusWithObject<long>>();

      var serializedTableName = SerializeTableName(conversationId);
      var conversationTable = await _tableServiceClient.GetTableAsync(serializedTableName, cancellationToken);
      if (conversationTable != null)
      {
        Parallel.ForEach(messageIds, async messageId =>
        {
          var idKey = messageId.ToString();
          var response = await conversationTable.DeleteEntityIfExistsAsync<TableConversationMessageEntity>(idKey, idKey, cancellationToken);

          var operationStatus = _mapper.Map<OperationStatus, OperationStatusWithObject<long>>(response);
          operationStatus.Result = messageId;
          result.Add(operationStatus);
        });
      }
      else
      {
        result.Add(new OperationStatusWithObject<long>
        {
          Error = new FixitResponseInfoDto
          {
            Code = HttpStatusCode.NotFound.ToString(),
            CodeDefinition = nameof(HttpStatusCode.NotFound),
            Description = $"Table {conversationId} does not exist..."
          }
        });
      }
      return result;
    }

    public async Task<SegmentedDocumentCollectionDto<ConversationMessageDto>> GetSegmentedConversationMessagesAsync(Guid conversationId, int segmentSize = PagedDocumentsAttributes.DefaultPageSize, TableContinuationToken? tableContinuationToken = null, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = new SegmentedDocumentCollectionDto<ConversationMessageDto> { IsOperationSuccessful = true };

      var serializedTableName = SerializeTableName(conversationId);
      var conversationTable = await _tableServiceClient.GetTableAsync(serializedTableName, cancellationToken);
      if (conversationTable != null)
      {
        Expression<Func<TableConversationMessageEntity, bool>> expression = item => true;
        var (responseMessages, responseContinuationToken) = await conversationTable.GetEntitiesByFilterAsync(expression, segmentSize, tableContinuationToken);
        if (responseMessages != null)
        {
          result.Results = responseMessages.Select(message => _mapper.Map<TableConversationMessageEntity, ConversationMessageDto>(message)).ToList();
          result.TableContinuationToken = responseContinuationToken;
        }
      }
      else
      {
        result.IsOperationSuccessful = false;
        result.Error = new FixitResponseInfoDto
        {
          Code = HttpStatusCode.NotFound.ToString(),
          CodeDefinition = nameof(HttpStatusCode.NotFound),
          Description = $"Table {conversationId} does not exist..."
        };
      }
      return result;
    }

    public async Task<OperationStatus> UpdateConversationMessageAsync(Guid conversationId, long messageId, Guid requestedByUserId, ConversationMessageUpsertRequestDto messageUpdateRequest, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = new OperationStatus();

      var serializedTableName = SerializeTableName(conversationId);
      var conversationTable = await _tableServiceClient.GetTableAsync(serializedTableName, cancellationToken);
      if (conversationTable != null)
      {
        var messageIdString = messageId.ToString();
        var conversationMessageEntity = conversationTable.GetEntity<TableConversationMessageEntity>(messageIdString, messageIdString);
        if (conversationMessageEntity != null)
        {
          var conversationMessageRequestEntity = _mapper.Map<ConversationMessageUpsertRequestDto, TableConversationMessageEntity>(messageUpdateRequest, conversationMessageEntity);
          conversationMessageEntity.UpdatedTimestampUtc = EpochHelper.GetTimestampMilliSecondsUtcNow();

          conversationMessageRequestEntity.CreatedByUser = conversationMessageRequestEntity.UpdatedByUser = JsonConvert.SerializeObject(messageUpdateRequest.SentByUser);
          result = await conversationTable.InsertOrReplaceEntityAsync(conversationMessageRequestEntity, cancellationToken);
        }
        else
        {
          result.Error = new FixitResponseInfoDto
          {
            Code = HttpStatusCode.BadRequest.ToString(),
            CodeDefinition = nameof(HttpStatusCode.BadRequest),
            Description = $"Conversation Message {messageIdString} from Table {conversationId} does not exist..."
          };
        }
      }
      else
      {
        result.Error = new FixitResponseInfoDto
        {
          Code = HttpStatusCode.BadRequest.ToString(),
          CodeDefinition = nameof(HttpStatusCode.BadRequest),
          Description = $"Table {conversationId} does not exist..."
        };
      }

      return result;
    }

    #region Storage Table Name Serializers

    private string SerializeTableName(Guid tableName)
    {
      var serializedTableName = tableName.ToString().ToLower().Replace("-", "");
      return new StringBuilder(StorageTableNameConstants.Prefix.ToLower()).Append(serializedTableName).ToString();
    }

    #endregion
  }
}
