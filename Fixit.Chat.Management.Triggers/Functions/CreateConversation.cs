using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Triggers.Helpers;
using Fixit.Core.DataContracts.Chat;
using Microsoft.Azure.WebJobs;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class CreateConversation
  {
    private readonly IConversationsMediator _conversationsMediator;

    public CreateConversation(IConversationsMediator conversationsMediator)
    {
      _conversationsMediator = conversationsMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(conversationsMediator)}... null argument was provided");
    }

    [FunctionName("CreateConversation")]
    public async Task Run([QueueTrigger("createconversationsqueue")]string queueItem, CancellationToken cancellationToken)
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

      await _conversationsMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);
    }
  }
}
