using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.Database.DataContracts.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.OpenApi.Models;

namespace Fixit.Chat.Management.ServerlessApi.Functions
{
  public class GetConversations
  {
    private readonly IConversationsMediator _conversationsMediator;

    public GetConversations(IConversationsMediator conversationsMediator) : base()
    {
      _conversationsMediator = conversationsMediator ?? throw new ArgumentNullException($"{nameof(GetConversations)} expects a value for {nameof(conversationsMediator)}... null argument was provided");
    }

    [FunctionName("GetConversations")]
    [OpenApiOperation("get", "ChatConversations")]
    [OpenApiParameter("userId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DocumentCollectionDto<ConversationDocument>))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/users/{userId:Guid}/conversations")]
                                         HttpRequestMessage httpRequest,
                                         CancellationToken cancellationToken,
                                         Guid userId)
    {
      return await GetConversationsAsync(userId, cancellationToken);
    }

    public async Task<IActionResult> GetConversationsAsync(Guid userId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (userId.Equals(Guid.Empty))
      {
        return new BadRequestObjectResult($"{nameof(GetConversationsAsync)} expects a value for {nameof(userId)}... null argument was provided");
      }

      var result = await _conversationsMediator.GetConversationsAsync(userId, cancellationToken);
      if (!result.IsOperationSuccessful)
      {
        if (result.OperationException != null)
        {
          return new BadRequestObjectResult(result);
        }
        return new NotFoundObjectResult($"Profile of user with id {userId} could not be found..");
      }

      return new OkObjectResult(result);
    }
  }
}
