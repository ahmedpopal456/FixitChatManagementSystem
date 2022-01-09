using AutoMapper;
using Azure.Messaging.ServiceBus;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib.Facades;
using Fixit.Chat.Management.Lib.Mappers;
using Fixit.Chat.Management.Lib.Mediators.Conversations;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Internal;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Messages.Internal;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus;
using Fixit.Chat.Management.Lib.Messaging.ServiceBus.Mediators;
using Fixit.Chat.Management.Triggers;
using Fixit.Chat.Management.Triggers.Mediators;
using Fixit.Chat.Management.Triggers.Mediators.Internal;
using Fixit.Core.Database;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts.Decorators.Extensions;
using Fixit.Core.Networking.Extensions;
using Fixit.Core.Storage.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Fixit.Chat.Management.Triggers
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      IConfiguration configuration = (IConfiguration)builder.Services.BuildServiceProvider()
                                                            .GetService(typeof(IConfiguration));

      builder.Services.AddSingleton<IMapper>(provider =>
      {
        var mapperConfig = new MapperConfiguration(mc =>
        {
          mc.AddProfile(new ChatMapper());
          mc.AddProfile(new OperationStatusMapper());
        });

        return mapperConfig.CreateMapper();
      });

      builder.Services.AddNmsServices("https://fixit-dev-nms-api.azurewebsites.net/");
      builder.Services.AddSignalRCore();
      builder.Services.AddDocumentDbTableEntityResolver(configuration);
      builder.Services.AddTransient<IFixitFacadeFactory, FixitFacadeFactory>();
      builder.Services.AddLogging();
      builder.Services.AddFixitCoreDecoratorServices();
      builder.Services.AddTransient<IConversationBaseMediator, ConversationBaseMediator>();
      builder.Services.AddTransient<IConversationMessageMediator, ConversationMessageMediator>();
      builder.Services.AddTransient<IConversationTriggersMediator, ConversationTriggersMediator>();
      builder.Services.AddSingleton<IStorageFactory, AzureStorageFactory>(provider =>
      {
        var configuration = provider.GetService<IConfiguration>();
        var name = configuration["FIXIT-CHMS-SA-AN"];
        var key = configuration["FIXIT-CHMS-SA-AK"];
        var uri = configuration["FIXIT-CHMS-SA-EP"];

        return new AzureStorageFactory(provider.GetService<IMapper>(), name, key, uri);
      });

      builder.Services.AddSingleton<IDatabaseMediator>(provider =>
      {
        var configuration = provider.GetService<IConfiguration>();

        var key = configuration["FIXIT-CHMS-DB-KEY"];
        var uri = configuration["FIXIT-CHMS-DB-URI"];
        return new DatabaseFactory(uri, key).CreateCosmosClient();
      });

      builder.Services.AddSingleton<IServiceBusMessagingClientMediator>(provider =>
      {
        var configuration = provider.GetService<IConfiguration>();

        var serviceBusConnectionString = configuration["FIXIT-CHMS-SB-CS"];
        var serviceBusOptions = new ServiceBusClientOptions()
        {
          TransportType = ServiceBusTransportType.AmqpTcp,
        };

        var serviceBusClient = ServiceBusMessagingServiceClientFactory.CreateServiceBusMessagingServiceClient(serviceBusConnectionString, serviceBusOptions);
        return serviceBusClient;
      });
    }
  }
}
