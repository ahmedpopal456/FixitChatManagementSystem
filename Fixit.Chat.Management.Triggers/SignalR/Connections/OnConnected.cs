using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(OnConnected))]
    public async Task OnConnected([SignalRTrigger] InvocationContext invocationContext)
    {
    }
  }
}
