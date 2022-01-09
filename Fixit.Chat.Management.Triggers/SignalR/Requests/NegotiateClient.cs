using Microsoft.AspNetCore.Http;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Triggers.SignalR
{
  public partial class FixitChatHub
  {
    [FunctionName(nameof(Negotiate))]
    public async Task<SignalRConnectionInfo> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat/users/{userId:Guid}/negotiate")]
                                                       HttpRequest httpRequest,
                                                       Guid userId)
    {
      // TODO: Check user before negotiating 

      var options = new NegotiationOptions
      {
        UserId = userId.ToString()
      };

      return await NegotiateAsync(options);
    }
  }
}
