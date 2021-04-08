using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Triggers.Helpers;
using Fixit.Core.DataContracts.Chat;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class CreateConversation
  {
    private readonly ILogger _logger;
    private readonly IConversationsMediator _conversationsMediator;

    public CreateConversation(IConversationsMediator conversationsMediator,
                              ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<CreateConversation>();
      _conversationsMediator = conversationsMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(conversationsMediator)}... null argument was provided");
    }

    [FunctionName(nameof(CreateConversation))]
    public async Task Run([QueueTrigger("createconversationsqueue", Connection = "FIXIT-CMS-STORAGEACCOUNT-CS")]string queueItem, CancellationToken cancellationToken)
    {
      await CreateConversationAsync(queueItem, cancellationToken);
    }

    public async Task CreateConversationAsync(string queueItem, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!ChatValidators.IsValidConversationCreateRequestDto(queueItem, out ConversationCreateRequestDto conversationCreateRequestDto))
      {
        return;
      }

      var createResult = await _conversationsMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);
      if (!createResult.IsOperationSuccessful)
      {
        var errorMessage = $"{nameof(CreateConversation)} failed to create new conversation for fix instance with id {conversationCreateRequestDto.FixInstanceId}";
        _logger.LogError(errorMessage);
        throw new ArgumentException(errorMessage);
      }
    }
  }
}
