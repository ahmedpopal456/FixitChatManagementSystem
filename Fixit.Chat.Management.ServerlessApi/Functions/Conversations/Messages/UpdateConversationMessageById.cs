using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations.Messages
{
  public class UpdateConversationMessageById
  {
    private readonly IConversationMessageMediator _conversationMessageMediator;

    public UpdateConversationMessageById(IConversationMessageMediator conversationMessageMediator)
    {
      _conversationMessageMediator = conversationMessageMediator ?? throw new ArgumentNullException($"{nameof(UpdateConversationMessageById)} expects a value for {nameof(conversationMessageMediator)}... null argument was provided");
    }

    [FunctionName(nameof(UpdateConversationMessageById))]
    [OpenApiOperation(OpenApis.OperationPut, ChatOpenApiTags.Messages)]
    [OpenApiRequestBody(OpenApis.ContentTypeApplicationJson, typeof(ConversationMessageUpsertRequestDto), Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Response body is of type: OperationStatus")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "ConversationMessageUpsertRequestDto is required and expected to have valid fields.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationPut, Route = "chat/conversations/{conversationId:Guid}/messages/{messageId:long}")]
                                         HttpRequestMessage httpRequest,
                                         Guid conversationId,
                                         long messageId,
                                         CancellationToken cancellationToken)
    {
      return await UpdateConversationMessageByIdAsync(httpRequest, conversationId, messageId, cancellationToken);
    }

    public async Task<IActionResult> UpdateConversationMessageByIdAsync(HttpRequestMessage httpRequestMessage, Guid conversationId, long messageId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      httpRequestMessage.Content.DeserializeToObject<ConversationMessageUpsertRequestDto>(out ConversationMessageUpsertRequestDto conversationMessageCreateRequestDto);
      if (conversationMessageCreateRequestDto is null)
      {
        return new BadRequestObjectResult($"Either {nameof(ConversationMessageUpsertRequestDto)} is null or has one or more invalid fields...");
      }

      var result = await _conversationMessageMediator.UpdateConversationMessageAsync(conversationId, messageId, Guid.Empty, conversationMessageCreateRequestDto, cancellationToken);
      return new OkObjectResult(result);
    }
  }
}
