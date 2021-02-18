using System.Threading;
using System.Threading.Tasks;
using Fixit.Core.DataContracts.Chat;

namespace Fixit.Chat.Management.Lib.Mediators
{
  public interface IChatMediator
  {
    /// <summary>
    /// Create a conversation in the CosmosDB
    /// </summary>
    /// <param name="conversationCreateRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateConversationAsync(ConversationCreateRequestDto conversationCreateRequestDto, CancellationToken cancellationToken);
  }
}
