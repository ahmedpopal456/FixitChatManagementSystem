using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Core.Database.Mediators;

namespace Fixit.Chat.Management.Lib
{
  public delegate IServiceBusMessagingClientMediator ServiceBusMessagingClientResolver(string key);
  public delegate IDatabaseTableEntityMediator DocumentDbTableEntityResolver(string key);

  public static class ChatAssemblyInfo
  {
    public static readonly string DataVersion = "1.0";
  }
}
