using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Mediators.Conversations;
using Fixit.Core.DataContracts;
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
  public class DeleteConversationById
  {
    private readonly IConversationBaseMediator _conversationBaseMediator;

    public DeleteConversationById(IConversationBaseMediator conversationBaseMediator)
    {
      _conversationBaseMediator = conversationBaseMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(conversationBaseMediator)}... null argument was provided");
    }

    [FunctionName(nameof(DeleteConversationById))]
    [OpenApiOperation(OpenApis.OperationPost, ChatOpenApiTags.Conversations)]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Response body is of type: OperationStatus")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "If the conversation id does not exist.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationDelete, Route = "chat/conversations/{id:Guid}")]
                                         HttpRequestMessage httpRequest,
                                         Guid id,
                                         CancellationToken cancellationToken)
    {
      return await DeleteConversationByIdAsync(id, cancellationToken);
    }

    public async Task<IActionResult> DeleteConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var result = await _conversationBaseMediator.DeleteConversationByIdAsync(conversationId, cancellationToken);
      if (result is default(OperationStatus))
      {
        return new NotFoundObjectResult($"The requested conversation with id {conversationId} was not found...");
      }

      return new OkObjectResult(result);
    }
  }
}
