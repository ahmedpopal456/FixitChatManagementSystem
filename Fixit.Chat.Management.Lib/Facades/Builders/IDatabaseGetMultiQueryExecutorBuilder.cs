using Fixit.Chat.Management.Lib.Facades.Contexts;
using Fixit.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Facades.Builders
{
  // TODO: Add Summaries
  public interface IDatabaseGetMultiQueryExecutorBuilder<T> : IDisposable
  {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="parallelism"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public Task<IEnumerable<OperationStatusWithObject<TResult>>> ForEachExecuteWithReturnAsync<TResult>(Func<T, TResult> handler = null) where TResult : class, new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parallelism"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public Task ForEachExecuteAsync(Action<T> handler = null);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>> ResolveContextAsync();
  }
}