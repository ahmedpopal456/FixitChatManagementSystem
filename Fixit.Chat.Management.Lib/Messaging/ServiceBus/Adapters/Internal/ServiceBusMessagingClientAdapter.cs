using Azure.Messaging.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Messaging.ServiceBus.Adapters.Internal
{
  public class ServiceBusMessagingClientAdapter : IServiceBusMessagingClientAdapter
  {
    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusMessagingClientAdapter(ServiceBusClient serviceBusClient)
    {
      _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException($"{nameof(ServiceBusMessagingClientAdapter)} expects a value for {nameof(serviceBusClient)}... null argument was provided");
    }
    
    public Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage serviceBusMessage, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return _serviceBusClient.CreateSender(queueOrTopicName)?.SendMessageAsync(serviceBusMessage);
    }
  }
}
