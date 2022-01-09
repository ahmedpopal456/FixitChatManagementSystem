using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.DataContracts.Chat.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations.Messages
{
  public class GetConversationMessagesBySegment
  {
    private readonly IConversationMessageMediator _conversationMessageMediator;

    public GetConversationMessagesBySegment(IConversationMessageMediator conversationMessageMediator)
    {
      _conversationMessageMediator = conversationMessageMediator ?? throw new ArgumentNullException($"{nameof(UpdateConversationMessageById)} expects a value for {nameof(conversationMessageMediator)}... null argument was provided");
    }

    [FunctionName(nameof(GetConversationMessagesBySegment))]
    [OpenApiOperation(OpenApis.OperationGet, ChatOpenApiTags.Messages)]
    [OpenApiParameter("segmentSize", In = ParameterLocation.Query, Required = false, Type = typeof(int))]
    [OpenApiParameter("nextRowKey", In = ParameterLocation.Header, Required = false, Type = typeof(string))]
    [OpenApiParameter("nextPartitionKey", In = ParameterLocation.Header, Required = false, Type = typeof(string))]
    [OpenApiParameter("nextTableName", In = ParameterLocation.Header, Required = false, Type = typeof(string))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, OpenApis.ContentTypeApplicationJson, typeof(SegmentedDocumentCollectionDto<ConversationMessageDto>))]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "When provided, segmentSize is expected to be greater than or equal to 0")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationGet, Route = "chat/conversations/{conversationId:Guid}/messages")]
                                         HttpRequestMessage httpRequest,
                                         Guid conversationId,
                                         CancellationToken cancellationToken)
    {
      return await GetSegmentedConversationMessagesAsync(httpRequest, conversationId, cancellationToken);
    }

    public async Task<IActionResult> GetSegmentedConversationMessagesAsync(HttpRequestMessage httpRequestMessage, Guid conversationId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      int segmentSize = default;
      var segmentSizeString = HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query).Get("segmentSize");
      if (!string.IsNullOrWhiteSpace(segmentSizeString) && !int.TryParse(segmentSizeString, out segmentSize))
      {
        return new BadRequestObjectResult($"The {nameof(segmentSize)} {segmentSize} is not valid...");
      }
      if (segmentSize <= default(int))
      {
        segmentSize = PagedDocumentsAttributes.DefaultPageSize;
      }

      var tableContinuationToken = new TableContinuationToken();
      if (httpRequestMessage.Headers.TryGetValues("nextRowKey", out var nextRowKey))
        tableContinuationToken.NextRowKey = nextRowKey.First();

      if (httpRequestMessage.Headers.TryGetValues("nextPartitionKey", out var nextPartitionKey))
        tableContinuationToken.NextPartitionKey = nextPartitionKey.First();

      if (httpRequestMessage.Headers.TryGetValues("nextTableName", out var nextTableName))
        tableContinuationToken.NextTableName = nextTableName.First();

      var result = await _conversationMessageMediator.GetSegmentedConversationMessagesAsync(conversationId, segmentSize, tableContinuationToken, cancellationToken);
      return new OkObjectResult(result);
    }
  }
}
