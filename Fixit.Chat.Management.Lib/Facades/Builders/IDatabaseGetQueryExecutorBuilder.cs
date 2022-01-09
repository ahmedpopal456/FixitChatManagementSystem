using Fixit.Chat.Management.Lib.Facades.Contexts;
using Fixit.Core.DataContracts;
using System;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Facades.Builders
{
  // TODO: Add Summaries
  public interface IDatabaseGetQueryExecutorBuilder<T> : IDisposable
  {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="parallelism"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public Task<OperationStatusWithObject<TResult>> ExecuteWithReturnAsync<TResult>(bool parallelism, Func<T, TResult> handler = null) where TResult : class, new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parallelism"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    public Task ExecuteAsync(bool parallelism, Action<T> handler);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<OperationStatusWithObject<DatabaseGetRequestContext<T>>> ResolveContextAsync();
  }
}