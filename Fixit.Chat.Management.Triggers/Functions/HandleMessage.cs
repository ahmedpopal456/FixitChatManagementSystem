using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Chat.Management.Triggers.Helpers;
using Microsoft.Azure.WebJobs;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class HandleMessage
  {
    private readonly IChatMediator _chatMediator;

    public HandleMessage(IChatMediator chatMediator) : base()
    {
      _chatMediator = chatMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(chatMediator)}... null argument was provided");
    }

    [FunctionName("HandleMessage")]
    public async Task Run([QueueTrigger("handlesendtouserqueue")] string queueItem, CancellationToken cancellationToken)
    {
      await HandleMessageAsync(queueItem, cancellationToken);
    }

    public async Task HandleMessageAsync(string queueItem, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (!ChatValidators.IsValidUserMessageCreateRequestDto(queueItem, out UserMessageCreateRequestDto userMessageCreateRequestDto))
      {
        return;
      }

      await _chatMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);
    }
  }
}
