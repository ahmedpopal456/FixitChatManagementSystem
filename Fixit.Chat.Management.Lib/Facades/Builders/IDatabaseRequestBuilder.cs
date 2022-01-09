using Fixit.Chat.Management.Lib.Facades.Enums;
using System;

namespace Fixit.Chat.Management.Lib.Facades.Builders
{
  // TODO: Add Summaries
  public interface IDatabaseRequestBuilder: IDisposable
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="empowerFunctionBuilder"></param>
    /// <param name="databaseType"></param>
    /// <returns></returns>
    IDatabaseGetQueryBuilder SetGetOperationDecorator(IFunctionInitBuilder empowerFunctionBuilder, DatabaseTypes databaseType);
  }
}
