using Fixit.Chat.Management.Lib.Facades.Contexts;
using Fixit.Chat.Management.Lib.Facades.Options;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Decorators.Exceptions;
using System;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Facades.Builders.Internal
{
  internal class FixitDatabaseGetQueryExecutorBuilder<T> : IDatabaseGetQueryExecutorBuilder<T>
  {
    private bool _disposedValue;
    private OperationStatusWithObject<DatabaseGetRequestContext<T>> _requestContextResults;
    private FunctionBuilderOptions _functionBuilderOptions;

    readonly IExceptionDecorator _exceptionDecorator;
    readonly Func<Task<OperationStatusWithObject<DatabaseGetRequestContext<T>>>> _requestContext;

    public IDatabaseMediator _documentDbManager;

    public FixitDatabaseGetQueryExecutorBuilder(IServiceProvider serviceProvider,
                                                  Func<Task<OperationStatusWithObject<DatabaseGetRequestContext<T>>>> requestContext,
                                                  FunctionBuilderOptions functionBuilderOptions)
    {

      _ = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetQueryExecutorBuilder<T>)} expects a value for {nameof(serviceProvider)}... null argument was provided");
      var exceptionDecorator = (IExceptionDecorator)serviceProvider.GetService(typeof(IExceptionDecorator));

      _functionBuilderOptions = functionBuilderOptions is null ? new FunctionBuilderOptions() : functionBuilderOptions;
      _exceptionDecorator = exceptionDecorator ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetQueryExecutorBuilder<T>)} expects a value for {nameof(exceptionDecorator)}... null argument was provided");
      _requestContext = requestContext ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetQueryExecutorBuilder<T>)} expects a value for {nameof(requestContext)}... null argument was provided");
    }

    public async Task<OperationStatusWithObject<TResult>> ExecuteWithReturnAsync<TResult>(bool parallelism, Func<T, TResult> handler = null) where TResult : class, new()
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetQueryExecutorBuilder<T>)) : _disposedValue;
      await this.ResolveContextAsync();

      var execution = await _exceptionDecorator.ExecuteOperationWithReturnAsync(_functionBuilderOptions.HandleExceptions, async () =>
      {
        var item = _requestContextResults.Result.FetchedResult;
        return handler.Invoke(item);
      });

      return execution;
    }

    public async Task ExecuteAsync(bool parallelism, Action<T> handler)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetQueryExecutorBuilder<T>)) : _disposedValue;
      await this.ResolveContextAsync();

      var item = _requestContextResults.Result.FetchedResult;
      handler.Invoke(item);
    }

    public async Task<OperationStatusWithObject<DatabaseGetRequestContext<T>>> ResolveContextAsync()
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseGetQueryExecutorBuilder<T>)) : _disposedValue;
      _requestContextResults ??= (await _requestContext.Invoke());

      return _requestContextResults;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _documentDbManager = null;
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
