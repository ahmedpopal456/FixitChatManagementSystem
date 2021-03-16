using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fixit.Core.Database;
using Fixit.Core.Database.Mediators;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.ServerlessApi;
using Fixit.Chat.Management.Lib.Mediators.Internal;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Fixit.Chat.Management.ServerlessApi
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      IConfiguration configuration = (IConfiguration)builder.Services.BuildServiceProvider()
                                                       .GetService(typeof(IConfiguration));

      DatabaseFactory databaseFactory = new DatabaseFactory(configuration["FIXIT-CM-DB-EP"], configuration["FIXIT-CM-DB-KEY"]);

      builder.Services.AddSingleton<IDatabaseMediator>(databaseFactory.CreateCosmosClient());
      builder.Services.AddSingleton<IConversationsMediator, ConversationsMediator>(provider =>
      {
        var databaseMediator = provider.GetService<IDatabaseMediator>();
        var configuration = provider.GetService<IConfiguration>();
        return new ConversationsMediator(databaseMediator, configuration);
      });
      builder.Services.AddSingleton<IMessagesMediator, MessagesMediator>(provider =>
      {
        var databaseMediator = provider.GetService<IDatabaseMediator>();
        var configuration = provider.GetService<IConfiguration>();
        return new MessagesMediator(databaseMediator, configuration);
      });
    }
  }
}
