using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.DataContracts.Chat;

namespace Fixit.Chat.Management.Lib.Mediators
{
  public interface IMessagesMediator
  {
    /// <summary>
    /// Handles new messages created
    /// </summary>
    /// <param name="userMessageCreateRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleMessageAsync(UserMessageCreateRequestDto userMessageCreateRequestDto, CancellationToken cancellationToken);
  }
}
