using Fixit.Core.DataContracts.Decorators.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Fixit.Chat.Management.Lib.Facades.Extensions.Services
{
  public static class FacadeServiceExtensions
  {
    public static void AddEmpowerFacadeServices(this IServiceCollection services)
    {
      services.AddTransient<IFixitFacadeFactory, FixitFacadeFactory>();
      services.AddFixitCoreDecoratorServices();
    }
  }
}
