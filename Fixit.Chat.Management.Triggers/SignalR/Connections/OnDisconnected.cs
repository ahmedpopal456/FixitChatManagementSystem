using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(OnDisconnected))]
    public async Task OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {
    }
  }
}
