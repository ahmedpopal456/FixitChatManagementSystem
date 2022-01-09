using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(LeaveGroup))]
    public async Task LeaveGroup([SignalRTrigger] InvocationContext invocationContext, string conversationId)
    {
      var cancellationToken = new CancellationTokenSource().Token;
      await UserGroups.RemoveFromGroupAsync(invocationContext.UserId, conversationId, cancellationToken);
    }
  }
}
