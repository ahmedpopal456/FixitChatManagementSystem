using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Fixit.Chat.Management.Lib.Mediators.Conversations;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib.Enums;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations
{
  public class GetConversationsByQuery
  {
    private readonly IConversationBaseMediator _conversationBaseMediator;

    public GetConversationsByQuery(IConversationBaseMediator conversationBaseMediator) : base()
    {
      _conversationBaseMediator = conversationBaseMediator ?? throw new ArgumentNullException($"{nameof(GetConversationsByQuery)} expects a value for {nameof(conversationBaseMediator)}... null argument was provided");
    }

    [FunctionName(nameof(GetConversationsByQuery))]
    [OpenApiOperation(OpenApis.OperationPost, ChatOpenApiTags.Conversations)]
    [OpenApiParameter("search", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
    [OpenApiParameter("orderDirection", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
    [OpenApiParameter("orderField", In = ParameterLocation.Query, Required = false, Type = typeof(string))]
    [OpenApiRequestBody(OpenApis.ContentTypeApplicationJson, typeof(ConversationQueryDto), Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Response Body is of type: OperationStatusWithObject with IEnumerable of ConversationDto")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "When provided, orderDirectionParsed is expected to be valid. When provided, ConversationQueryDto is expected to have valid fields.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationPost, Route = "chat/conversations/query")]
                                         HttpRequestMessage httpRequest,
                                         CancellationToken cancellationToken)
    {
      return await GetConversationsByQueryAsync(httpRequest, cancellationToken);
    }

    public async Task<IActionResult> GetConversationsByQueryAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var searchString = HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query).Get("search");
      var orderDirection = HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query).Get("orderDirection");
      var orderField = HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query).Get("orderField");

      var orderDirectionParsed = default(object?);
      if (!string.IsNullOrWhiteSpace(orderDirection) && !Enum.TryParse(typeof(OrderDirections), orderDirection, out orderDirectionParsed))
      {
        return new BadRequestObjectResult($"The type {orderDirectionParsed} is not valid");
      }

      httpRequestMessage.Content.DeserializeToObject<ConversationQueryDto>(out ConversationQueryDto conversationQueryDto);
      if (conversationQueryDto is null)
      {
        return new BadRequestObjectResult($"Either {nameof(ConversationQueryDto)} is null or has one or more invalid fields...");
      }

      var result = await _conversationBaseMediator.GetConversationsByQueryAsync(searchString, conversationQueryDto, cancellationToken, orderField, (OrderDirections?)orderDirectionParsed);
      return new OkObjectResult(result);
    }
  }
}
