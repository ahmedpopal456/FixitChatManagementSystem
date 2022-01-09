using Fixit.Chat.Management.Lib.Facades.Builders;
using Fixit.Core.Database.Mediators;
using System;

namespace Fixit.Chat.Management.Lib.Facades
{
  // TODO: Add Summaries
  public interface IFixitDatabaseRequestBuilderFacade : IDisposable, IFixitFunctionBuilderFacade
  {
    /// <summary>
    /// 
    /// </summary>
    public IDatabaseGetQueryBuilder Get { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public IFixitDatabaseRequestBuilderFacade WithDatabaseName(string databaseName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public IFixitDatabaseRequestBuilderFacade WithTableName(string tableName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    public IFixitDatabaseRequestBuilderFacade WithContainer(IDatabaseTableEntityMediator container);
  }
}
