using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.OpenApi.Models;

namespace Fixit.Chat.Management.ServerlessApi.Functions
{
  public class GetMessages
  {
    private readonly IMessagesMediator _messagesMediator;

    public GetMessages(IMessagesMediator messagesMediator) : base()
    {
      _messagesMediator = messagesMediator ?? throw new ArgumentNullException($"{nameof(GetMessages)} expects a value for {nameof(messagesMediator)}... null argument was provided");
    }

    [FunctionName("GetMessages")]
    [OpenApiOperation("get", "ChatMessages")]
    [OpenApiParameter("conversationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
    [OpenApiParameter("pageNumber", In = ParameterLocation.Query, Required = false, Type = typeof(int))]
    [OpenApiParameter("pageSize", In = ParameterLocation.Query, Required = false, Type = typeof(int))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMessagesResponseDto))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/users/me/conversations/{conversationId:Guid}/messages")]
                                         HttpRequestMessage httpRequest,
                                         CancellationToken cancellationToken,
                                         Guid conversationId)
    {
      int.TryParse(HttpUtility.ParseQueryString(httpRequest.RequestUri.Query).Get("pageNumber"), out int pageNumber);
      int.TryParse(HttpUtility.ParseQueryString(httpRequest.RequestUri.Query).Get("pageSize"), out int pageSize);
      return await GetMessagesAsync(conversationId, pageNumber, pageSize, cancellationToken);
    }

    public async Task<IActionResult> GetMessagesAsync(Guid conversationId,
                                                      int pageNumber,
                                                      int pageSize,
                                                      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (conversationId.Equals(Guid.Empty))
      {
        return new BadRequestObjectResult($"{nameof(GetMessagesAsync)} expects a value for {nameof(conversationId)}... null argument was provided");
      }
      if (pageNumber < default(int))
      {
        return new BadRequestObjectResult($"{nameof(GetMessagesAsync)} expects a valid value for {nameof(pageNumber)}");
      }
      if (pageSize < default(int))
      {
        return new BadRequestObjectResult($"{nameof(GetMessagesAsync)} expects a valid value for {nameof(pageSize)}");
      }
      var result = await _messagesMediator.GetMessagesAsync(conversationId, pageNumber, pageSize, cancellationToken);
      if (!result.IsOperationSuccessful)
      {
        if (result.OperationException != null)
        {
          return new BadRequestObjectResult(result);
        }
        return new NotFoundObjectResult($"Conversation with id {conversationId} could not be found..");
      }

      return new OkObjectResult(result);
    }
  }
}
