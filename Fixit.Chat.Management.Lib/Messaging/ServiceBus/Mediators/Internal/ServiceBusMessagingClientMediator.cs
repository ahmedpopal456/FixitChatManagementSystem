using Azure.Messaging.ServiceBus;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Adapters;
using Fixit.Core.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators.Internal
{
  public class ServiceBusMessagingClientMediator : IServiceBusMessagingClientMediator
  {
    private readonly IServiceBusMessagingClientAdapter _serviceBusMessagingClientAdapter;

    public ServiceBusMessagingClientMediator(IServiceBusMessagingClientAdapter serviceBusMessagingClientAdapter)
    {
      _serviceBusMessagingClientAdapter = serviceBusMessagingClientAdapter ?? throw new ArgumentNullException($"{nameof(ServiceBusMessagingClientMediator)} expects a value for {nameof(serviceBusMessagingClientAdapter)}... null argument was provided");
    }
    
    public async Task<OperationStatus> SendMessageAsync(string queueOrTopicName, ServiceBusMessage serviceBusMessage, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var result = new OperationStatus() { IsOperationSuccessful = true };
      try
      {
        await _serviceBusMessagingClientAdapter.SendMessageAsync(queueOrTopicName, serviceBusMessage, cancellationToken);
      }
      catch (ServiceBusException serviceBusException)
      {
        result.IsOperationSuccessful = false;
        result.OperationException = serviceBusException;
      }

      return result;
    }
  }
}
