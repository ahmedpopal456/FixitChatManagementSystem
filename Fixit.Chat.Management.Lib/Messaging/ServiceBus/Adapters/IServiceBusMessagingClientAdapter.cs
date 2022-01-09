using Azure.Messaging.ServiceBus;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Messaging.ServiceBus.Adapters
{
  public interface IServiceBusMessagingClientAdapter
  {
    /// <summary>
    /// Send a message to a given queue or topic, asychronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queueOrTopicName"></param>
    /// <param name="serviceBusMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage serviceBusMessage, CancellationToken cancellationToken);
  }
}