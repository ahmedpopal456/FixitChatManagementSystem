using System;
using AutoMapper;
using Fixit.Chat.Management.Lib.Helpers;
using Fixit.Chat.Management.Lib.Mappers;
using Fixit.Chat.Management.Lib.Mediators;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Core.Database.Mediators;
using Fixit.Core.Networking.Extensions;
using Fixit.Core.Networking.Local.NMS;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fixit.Chat.Management.Lib.Extensions
{
  public static class FixitChatSystemExtension
  {
    public static void AddFixitChatServices(this IFunctionsHostBuilder builder)
    {
      var mapperConfig = new MapperConfiguration(mc =>
      {
        mc.AddProfile(new ChatManagementMapper());
      });
      builder.Services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

      builder.Services.AddSingleton<IChatNotificationFactory>(new ChatNotificationFactory());
      builder.Services.AddNmsServices("https://fixit-dev-nms-api.azurewebsites.net/");

      builder.Services.AddSingleton<IConversationsMediator, ConversationsMediator>(provider =>
      {
        var chatNotificationFactory = provider.GetService<IChatNotificationFactory>();
        var databaseMediator = provider.GetService<IDatabaseMediator>();
        var configuration = provider.GetService<IConfiguration>();
        var nmsHttpClient = provider.GetService<IFixNmsHttpClient>();
        return new ConversationsMediator(databaseMediator, chatNotificationFactory, nmsHttpClient, configuration);
      });
      builder.Services.AddSingleton<IMessagesMediator, MessagesMediator>(provider =>
      {
        var mapper = provider.GetService<IMapper>();
        var databaseMediator = provider.GetService<IDatabaseMediator>();
        var configuration = provider.GetService<IConfiguration>();
        var nmsHttpClient = provider.GetService<IFixNmsHttpClient>();
        return new MessagesMediator(mapper, databaseMediator, nmsHttpClient, configuration);
      });
    }
  }
}
