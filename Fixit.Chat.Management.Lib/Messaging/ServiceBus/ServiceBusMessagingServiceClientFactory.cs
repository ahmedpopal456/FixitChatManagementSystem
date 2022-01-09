using Azure.Messaging.ServiceBus;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Adapters.Internal;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators.Internal;
using System;

namespace Fixit.Chat.Management.Lib.Messaging.ServiceBus
{
  public static class ServiceBusMessagingServiceClientFactory
  {
    public static IServiceBusMessagingClientMediator CreateServiceBusMessagingServiceClient(string connectionString, ServiceBusClientOptions serviceBusClientOptions)
    {
      _ = string.IsNullOrWhiteSpace(connectionString) ?  throw new ArgumentNullException($"{nameof(ServiceBusMessagingServiceClientFactory)}: A value for {nameof(connectionString)} was expected... null value was provided.") : string.Empty;

      var serviceBusClient = new ServiceBusClient(connectionString, serviceBusClientOptions);
      return new ServiceBusMessagingClientMediator(new ServiceBusMessagingClientAdapter(serviceBusClient));
    }
  }
}
