using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Core.DataContracts.Chat.Notifications;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts.Notifications.Enums;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.DataContracts.Notifications.Payloads;
using Fixit.Core.Networking.Local.NMS;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fixit.Chat.Management.Triggers.Functions
{
  public class CreateFixConversation
  {
    private readonly ILogger _logger;
    private readonly IConversationTriggersMediator _conversationsMediator;
    private readonly IFixNmsHttpClient _fixNmsHttpClient;

    public CreateFixConversation(IConversationTriggersMediator conversationsMediator,
                                 IFixNmsHttpClient fixNmsHttpClient,
                                 ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<CreateFixConversation>();
      _fixNmsHttpClient = fixNmsHttpClient ?? throw new ArgumentNullException($"{nameof(CreateFixConversation)} expects a value for {nameof(fixNmsHttpClient)}... Null argument was provided");
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
          else
          {
            var client = createResult.Result.Participants.First();
            var craftsman = createResult.Result.Participants.Last();

            var enqueueNotificationRequestDto = new EnqueueNotificationRequestDto
            {
              Title = $"New Chat Started Between {client.User.FirstName} and {craftsman.User.LastName}!",
              Message = "Be the first to send a message!",
              Payload = new NotificationPayloadDto()
              {
                Action = NotificationTypes.NewConversation,
                SystemPayload = new ConversationMessageNotificationDto()
                {
                  ConversationId = createResult.Result.Id
                }
              },
              IsTransient = true,
              RecipientUsers = createResult.Result.Participants.Select(item => item.User)
            };
            await _fixNmsHttpClient.PostNotification(enqueueNotificationRequestDto, cancellationToken);
          }
        }
      }
    }
  }
}
