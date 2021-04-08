using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Chat.Management.Triggers.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class HandleMessage
  {
    private readonly ILogger _logger;
    private readonly IMessagesMediator _messagesMediator;
    private readonly IConversationsMediator _conversationsMediator;

    public HandleMessage(IMessagesMediator messagesMediator,
                         IConversationsMediator conversationsMediator,
                         ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<CreateConversation>();
      _messagesMediator = messagesMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(messagesMediator)}... null argument was provided");
      _conversationsMediator = conversationsMediator ?? throw new ArgumentNullException($"{nameof(CreateConversation)} expects a value for {nameof(conversationsMediator)}... null argument was provided");
    }

    [FunctionName(nameof(HandleMessage))]
    public async Task Run([QueueTrigger("handlesendtouserqueue", Connection = "FIXIT-CMS-STORAGEACCOUNT-CS")] string queueItem, CancellationToken cancellationToken)
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

      var result = await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);
      if (!result.IsOperationSuccessful)
      {
        var errorMessage = $"{nameof(HandleMessage)} failed to update conversation messages id {userMessageCreateRequestDto.ConversationId} with new message id {userMessageCreateRequestDto.Message.Id}";
        _logger.LogError(errorMessage);
        throw new InvalidOperationException(errorMessage);
      }
      var updateResult = await _conversationsMediator.UpdateLastMessageAsync(userMessageCreateRequestDto, cancellationToken);

      if (updateResult.OperationException != null)
      {
        var errorMessage = $"{nameof(HandleMessage)} failed to update conversation id {userMessageCreateRequestDto.ConversationId} with latest message id {userMessageCreateRequestDto.Message.Id}";
        _logger.LogError(errorMessage);
        throw new InvalidOperationException(errorMessage);
      }
    }
  }
}
