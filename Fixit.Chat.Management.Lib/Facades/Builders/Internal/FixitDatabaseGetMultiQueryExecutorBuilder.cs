using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Facades.Contexts;
using Fixit.Chat.Management.Lib.Facades.Enums;
using Fixit.Chat.Management.Lib.Facades.Options;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Decorators.Exceptions;

namespace Fixit.Chat.Management.Lib.Facades.Builders.Internal
{
  internal class FixitDatabaseGetMultiQueryExecutorBuilder<T> : IDatabaseGetMultiQueryExecutorBuilder<T>
  {
    private bool _disposedValue;

    readonly IExceptionDecorator _exceptionDecorator;
    readonly Func<Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>>> _requestContext;

    private OperationStatusWithObject<DatabaseMultiGetRequestContext<T>> _requestContextResults;
    private FunctionBuilderOptions _functionBuilderOptions;

    public FixitDatabaseGetMultiQueryExecutorBuilder(IServiceProvider serviceProvider,
                                                    Func<Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>>> requestContext,
                                                    FunctionBuilderOptions functionBuilderOptions)
    {
      _ = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)} expects a value for {nameof(serviceProvider)}... null argument was provided");
      var exceptionDecorator = (IExceptionDecorator)serviceProvider.GetService(typeof(IExceptionDecorator));

      _functionBuilderOptions = functionBuilderOptions is null ? new FunctionBuilderOptions() : functionBuilderOptions;
      _exceptionDecorator = exceptionDecorator ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)} expects a value for {nameof(exceptionDecorator)}... null argument was provided");
      _requestContext = requestContext ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)} expects a value for {nameof(requestContext)}... null argument was provided");
    }

    public async Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>> ResolveContextAsync()
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)) : _disposedValue;
      _requestContextResults ??= (await _requestContext.Invoke());

      return _requestContextResults;
    }

    public async Task<IEnumerable<OperationStatusWithObject<TResult>>> ForEachExecuteWithReturnAsync<TResult>(Func<T, TResult> handler = null) where TResult : class, new()
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)) : _disposedValue;
      await this.ResolveContextAsync();

      var results = new List<OperationStatusWithObject<TResult>>();
      var items = _requestContextResults?.Result?.FetchTypes == MultiFetchTypes.Many ? _requestContextResults?.Result?.FetchedResultsMany : _requestContextResults?.Result?.FetchedResultsByPage?.Results;

      foreach (var item in items)
      {
        var execution = await _exceptionDecorator.ExecuteOperationWithReturnAsync<TResult>(_functionBuilderOptions.HandleExceptions, async () =>
        {
          return handler.Invoke(item);
        });

        results.Add(execution);
      }
      return results;
    }

    public async Task ForEachExecuteAsync(Action<T> handler = null)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetMultiQueryExecutorBuilder<T>)) : _disposedValue;
      await this.ResolveContextAsync();

      var items = _requestContextResults?.Result?.FetchTypes == MultiFetchTypes.Many ? _requestContextResults?.Result?.FetchedResultsMany : _requestContextResults?.Result?.FetchedResultsByPage?.Results;

      foreach (var item in items)
      {
        handler.Invoke(item);
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _requestContextResults?.Result?.Dispose();
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
