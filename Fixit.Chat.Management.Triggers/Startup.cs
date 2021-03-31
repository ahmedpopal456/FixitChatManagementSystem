using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fixit.Core.Database;
using Fixit.Core.Database.Mediators;
using Fixit.Chat.Management.Triggers;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Core.Storage;
using Fixit.Core.Storage.Queue.Mediators;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Fixit.Chat.Management.Triggers
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      IConfiguration configuration = (IConfiguration)builder.Services.BuildServiceProvider()
                                                       .GetService(typeof(IConfiguration));

      DatabaseFactory databaseFactory = new DatabaseFactory(configuration["FIXIT-CMS-DB-EP"], configuration["FIXIT-CMS-DB-KEY"]);
      builder.Services.AddSingleton<IDatabaseMediator>(databaseFactory.CreateCosmosClient());

      StorageFactory storageFactory = new StorageFactory(configuration["FIXIT-CMS-STORAGEACCOUNT-CS"]);
      builder.Services.AddSingleton<IQueueServiceClientMediator>(storageFactory.CreateQueueServiceClientMediator());

      builder.Services.AddLogging();
      builder.AddFixitChatServices();
    }
  }
}
