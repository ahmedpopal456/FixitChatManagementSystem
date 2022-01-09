using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Fixit.Chat.Management.Lib.Messaging.ServiceBus.Extensions
{
  public static class ServiceBusExtension
  {
    public static void AddServiceBusMessagingService(this IServiceCollection services)
    {
      services.AddTransient<IServiceBusMessagingClientMediator, ServiceBusMessagingClientMediator>();
    }
  }
}