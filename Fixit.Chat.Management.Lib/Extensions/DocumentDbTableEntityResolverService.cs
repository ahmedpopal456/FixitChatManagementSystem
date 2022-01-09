using Fixit.Chat.Management.Lib.Constants;
using Fixit.Core.Database.Mediators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Fixit.Chat.Management.Lib.Extensions
{
  public static class DocumentDbTableEntityResolverService
  {
    public static void AddDocumentDbTableEntityResolver(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddSingleton<DocumentDbTableEntityResolver>(serviceProvider => name =>
      {
        var dbName = configuration["FIXIT-CHMS-DB-NAME"];

        var dbManager = serviceProvider.GetService<IDatabaseMediator>();
        var db = dbManager.GetDatabase(dbName);

        return name switch
        {
          StorageTableNameConstants.Conversations => db.GetContainer(StorageTableNameConstants.Conversations),
          _ => throw new KeyNotFoundException()
        };
      });
    }
  }
}
