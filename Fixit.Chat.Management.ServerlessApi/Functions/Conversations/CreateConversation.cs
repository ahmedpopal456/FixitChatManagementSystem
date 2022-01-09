using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib.Mediators.Conversations;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations
{
  public class CreateConversation
  {
    private readonly IConversationBaseMediator _conversationBaseMediator;

    public CreateConversation(IConversationBaseMediator conversationBaseMediator)
    {
      _conversationBaseMediator = conversationBaseMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(conversationBaseMediator)}... null argument was provided");
    }

    [FunctionName(nameof(CreateConversation))]
    [OpenApiOperation(OpenApis.OperationPost, ChatOpenApiTags.Conversations)]
    [OpenApiRequestBody(OpenApis.ContentTypeApplicationJson, typeof(ConversationCreateRequestDto), Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Response body is of type: OperationStatusWithObject with ConversationDto")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "ConversationCreateRequestDto is required and expected to have valid fields.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationPost, Route = "chat/conversations")]
                                         HttpRequestMessage httpRequest,
                                         CancellationToken cancellationToken)
    {
      return await CreateConversationAsync(httpRequest, cancellationToken);
    }

    public async Task<IActionResult> CreateConversationAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      httpRequestMessage.Content.DeserializeToObject<ConversationCreateRequestDto>(out ConversationCreateRequestDto conversationQueryDto);
      if (conversationQueryDto is null)
      {
        return new BadRequestObjectResult($"Either {nameof(ConversationCreateRequestDto)} is null or has one or more invalid fields...");
      }

      var result = await _conversationBaseMediator.CreateConversationAsync( conversationQueryDto, cancellationToken);
      return new OkObjectResult(result);
    }
  }
}
