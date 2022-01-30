using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class CreateFixConversation
  {
    private readonly ILogger _logger;
    private readonly IConversationTriggersMediator _conversationsMediator;

    public CreateFixConversation(IConversationTriggersMediator conversationsMediator,
                                 ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<CreateFixConversation>();
      _conversationsMediator = conversationsMediator ?? throw new ArgumentNullException($"{nameof(CreateFixConversation)} expects a value for {nameof(conversationsMediator)}... null argument was provided");
    }

    [FunctionName(nameof(CreateFixConversation))]
    public async Task Run([QueueTrigger("createconversationsqueue", Connection = "FIXIT-CHMS-STORAGEACCOUNT-CS")]string queueItem, CancellationToken cancellationToken)
    {
      await CreateConversationAsync(queueItem, cancellationToken);
    }

    public async Task CreateConversationAsync(string queueItem, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      
      var createConversationDeserialized = JsonConvert.DeserializeObject<ConversationCreateRequestDto>(queueItem);
      if (createConversationDeserialized is { Details: { Id: var fixInstanceId } })
      {
        var isValid = !fixInstanceId.Equals(Guid.Empty) && createConversationDeserialized.Participants != null && createConversationDeserialized.Participants.Count > default(int);
        if (isValid)
        {
          var createResult = await _conversationsMediator.CreateConversationAsync(createConversationDeserialized, cancellationToken);
          if (!createResult.IsOperationSuccessful)
          {
            var errorMessage = $"{nameof(CreateFixConversation)} failed to create new conversation for fix instance with id {fixInstanceId}";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
          }
        }
      }
    }
  }
}
