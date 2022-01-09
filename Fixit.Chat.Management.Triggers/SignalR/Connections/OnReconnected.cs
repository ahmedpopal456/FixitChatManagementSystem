using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(OnReconnected))]
    public async Task OnReconnected([SignalRTrigger] InvocationContext invocationContext)
    {
    }
  }
}
