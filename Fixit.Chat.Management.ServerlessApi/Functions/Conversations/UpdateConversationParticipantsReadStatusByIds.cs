using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Conversations;
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Chat.Management.Lib.Extensions;

namespace Fixit.Chat.Management.ServerlessApi.Functions.Conversations
{
  public class UpdateConversationParticipantsReadStatusByIds
  {
    private readonly IConversationBaseMediator _conversationBaseMediator;

    public UpdateConversationParticipantsReadStatusByIds(IConversationBaseMediator conversationBaseMediator)
    {
      _conversationBaseMediator = conversationBaseMediator ?? throw new ArgumentNullException($"{nameof(UpdateConversationParticipantsReadStatusByIds)} expects a value for {nameof(conversationBaseMediator)}... null argument was provided");
    }

    [FunctionName(nameof(UpdateConversationParticipantsReadStatusByIds))]
    [OpenApiOperation(OpenApis.OperationPost, ChatOpenApiTags.Conversations)]
    [OpenApiRequestBody(OpenApis.ContentTypeApplicationJson, typeof(IEnumerable<ParticipantReadStatusUpdateRequestDto>), Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, OpenApis.ContentTypeApplicationJson, typeof(OperationStatusWithObject<ConversationDto>))]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "participantReadStatusUpdateRequestDtos is required and expected to have valid fields")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "If the conversationId does not exist")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, OpenApis.OperationPut, Route = "chat/conversations/{id:Guid}/participants/messages/read")]
                                         HttpRequestMessage httpRequest,
                                         Guid id,
                                         CancellationToken cancellationToken)
    {
      return await UpdateConversationParticipantsReadStatusByIdsAsync(id, httpRequest, cancellationToken);
    }

    public async Task<IActionResult> UpdateConversationParticipantsReadStatusByIdsAsync(Guid conversationId, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      httpRequestMessage.Content.DeserializeToObjectList<ParticipantReadStatusUpdateRequestDto>(out IEnumerable<ParticipantReadStatusUpdateRequestDto> participantReadStatusUpdateRequestDtos);
      if (participantReadStatusUpdateRequestDtos is null)
      {
        return new BadRequestObjectResult($"Either {nameof(participantReadStatusUpdateRequestDtos)} is null or has one or more invalid fields...");
      }

      var result = await _conversationBaseMediator.UpdateConversationParticipantsReadStatusByBulkIdAsync(conversationId, participantReadStatusUpdateRequestDtos, cancellationToken);
      if (result is default(OperationStatusWithObject<ConversationDto>))
      {
        return new NotFoundObjectResult($"The requested conversation with id {conversationId} was not found...");
      }

      return new OkObjectResult(result);
    }
  }
}
