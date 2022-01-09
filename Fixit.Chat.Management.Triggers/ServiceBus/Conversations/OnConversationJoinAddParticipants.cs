using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Fixit.Core.DataContracts.Chat.Operations.Messages;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using Fixit.Core.Storage.Storage;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Core.Networking.Local.NMS;

namespace Fixit.Chat.Management.Triggers.ServiceBus.Conversations
{
  public class OnConversationJoinAddParticipants : TriggersBase
  {
    public OnConversationJoinAddParticipants(IConfiguration configurationProvider,
                                             IConversationMessageMediator conversationMessageMediator,
                                             IConversationTriggersMediator conversationTriggersMediator,
                                             IStorageFactory storageFactory,
                                             IFixNmsHttpClient fixNmsHttpClient,
                                             IServiceBusMessagingClientMediator serviceBusMessagingClientMediator) : base(configurationProvider, conversationMessageMediator, conversationTriggersMediator, storageFactory, serviceBusMessagingClientMediator, fixNmsHttpClient) { }

    [FunctionName(nameof(OnConversationJoinAddParticipants))]
    public async Task RunAsync([ServiceBusTrigger("joingroupqueue", Connection = "FIXIT-CHMS-SB-CS", IsSessionsEnabled = true)] ServiceBusReceivedMessage[] messages,
                               CancellationToken cancellationToken)
    {
      var participantsToAddToConversation = messages.Select(message => JsonConvert.DeserializeObject<AddParticipantToChatMessage>(Encoding.UTF8.GetString(message.Body)));

      if(participantsToAddToConversation is { } && participantsToAddToConversation.Any())
      {
        var conversationId = participantsToAddToConversation.First().ConversationId;
        var participants = participantsToAddToConversation.Select(participant => participant.User);

        var addParticipantsToConversationRequest = new AddConversationParticipantsRequestDto()
        {
          ConversationId = conversationId,
          Participants = participants.ToList()
        };  

        await _conversationTriggersMediator.AddConversationParticipantsByBulkIdAsync(addParticipantsToConversationRequest, cancellationToken);
      }
    }
  }
}
