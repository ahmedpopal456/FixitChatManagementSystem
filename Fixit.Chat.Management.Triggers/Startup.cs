using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fixit.Core.Database;
using Fixit.Core.Database.Mediators;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Chat.Management.Lib.Mappers;
using Fixit.Chat.Management.Triggers;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Fixit.Chat.Management.Triggers
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      IConfiguration configuration = (IConfiguration)builder.Services.BuildServiceProvider()
                                                       .GetService(typeof(IConfiguration));

      var mapperConfig = new MapperConfiguration(mc =>
      {
        mc.AddProfile(new ChatManagementMapper());
      });

      DatabaseFactory databaseFactory = new DatabaseFactory(configuration["FIXIT-CM-DB-EP"], configuration["FIXIT-CM-DB-KEY"]);

      builder.Services.AddSingleton<IMapper>(mapperConfig.CreateMapper());
      builder.Services.AddSingleton<IDatabaseMediator>(databaseFactory.CreateCosmosClient());
      builder.Services.AddSingleton<IChatMediator, ChatMediator>(provider =>
      {
        var mapper = provider.GetService<IMapper>();
        var databaseMediator = provider.GetService<IDatabaseMediator>();
        var configuration = provider.GetService<IConfiguration>();
        return new ChatMediator(mapper, databaseMediator, configuration);
      });
    }
  }
}
