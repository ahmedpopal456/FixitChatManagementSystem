using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations.Messages
{
  public class DeleteConversationMessagesByIds
  {
    private readonly IConversationMessageMediator _conversationMessageMediator;

    public DeleteConversationMessagesByIds(IConversationMessageMediator conversationMessageMediator)
    {
      _conversationMessageMediator = conversationMessageMediator ?? throw new ArgumentNullException($"{nameof(UpdateConversationMessageById)} expects a value for {nameof(conversationMessageMediator)}... null argument was provided");
    }

    [FunctionName(nameof(DeleteConversationMessagesByIds))]
    [OpenApiOperation(OpenApis.OperationDelete, ChatOpenApiTags.Messages)]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Response body is of type: IList of OperationStatusWithObject with long")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Header 'messageIds' is required and expected to be of type long.")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationDelete, Route = "chat/conversations/{conversationId:Guid}/messages")]
                                         HttpRequestMessage httpRequest,
                                         Guid conversationId,
                                         CancellationToken cancellationToken)
    {
      return await DeleteConversationMessageAsync(httpRequest, conversationId, cancellationToken);
    }

    public async Task<IActionResult> DeleteConversationMessageAsync(HttpRequestMessage httpRequestMessage, Guid conversationId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var messageIds = new List<long>();
      if (!httpRequestMessage.Headers.TryGetValues("messageIds", out var messageIdsString))
      {
        return new BadRequestObjectResult($"{nameof(DeleteConversationMessagesByIds)} expects value for header 'messageIds'...");
      }
      else
      {
        foreach (var messageIdString in messageIdsString)
        {
          if (!long.TryParse(messageIdString, out var messageId))
            return new BadRequestObjectResult($"{nameof(DeleteConversationMessagesByIds)} expects value of header 'messageIds' to be of type long...");

          messageIds.Add(messageId);
        };
      }

      var result = await _conversationMessageMediator.DeleteConversationMessageByBulkIdsAsync(conversationId, messageIds, cancellationToken);
      return new OkObjectResult(result);
    }
  }
}
