using Fixit.Chat.Management.Lib.Facades.Builders;
using Fixit.Chat.Management.Lib.Facades.Builders.Internal;
using Fixit.Chat.Management.Lib.Facades.Options;
using Fixit.Core.Database.Mediators;
using System;

namespace Fixit.Chat.Management.Lib.Facades.Internal
{
  internal class FixitDatabaseRequestBuilderFacade : FixitFunctionBuilderFacade, IFixitDatabaseRequestBuilderFacade
  {
    private bool _disposedValue;

    private string _databaseName;
    private string _tableName;
    private IDatabaseTableEntityMediator _container;

    readonly FunctionBuilderOptions _functionBuilderOptions;
    readonly IServiceProvider _serviceProvider;

    private IDatabaseGetQueryBuilder _get;

    public FixitDatabaseRequestBuilderFacade(IServiceProvider serviceProvider,
                                               FunctionBuilderOptions functionBuilderOptions) : base(serviceProvider)
    {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitDatabaseRequestBuilderFacade)} expects a value for {nameof(serviceProvider)}... null argument was provided");
      _functionBuilderOptions = functionBuilderOptions is null ? new FunctionBuilderOptions() : functionBuilderOptions;
    }

    public IDatabaseGetQueryBuilder Get
    {
      set => _get = value;
      get
      {
        var documentDbTable = _container;
        if (documentDbTable is null)
        {          
          _ = string.IsNullOrWhiteSpace(_tableName) ? throw new InvalidOperationException($"{nameof(_tableName)} must be defined...") : string.Empty;
          _ = string.IsNullOrWhiteSpace(_databaseName) ? throw new InvalidOperationException($"{nameof(_databaseName)} must be defined...") : string.Empty;

          var documentDbManager = (IDatabaseMediator)_serviceProvider.GetService(typeof(IDatabaseMediator));
          documentDbTable = documentDbManager?.GetDatabase(_databaseName)?.GetContainer(_tableName);
          _ = documentDbTable is null ? throw new ArgumentNullException($"{nameof(documentDbManager)} could not resolve with provided parameters...") : documentDbTable;
        }

        return _get ??= new FixitDatabaseGetQueryBuilder(_serviceProvider, documentDbTable, _functionBuilderOptions);
      }
    }

    public IFixitDatabaseRequestBuilderFacade WithDatabaseName(string databaseName)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseRequestBuilderFacade)) : _disposedValue;

      _databaseName = databaseName;
      return this;
    }

    public IFixitDatabaseRequestBuilderFacade WithTableName(string tableName)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseRequestBuilderFacade)) : _disposedValue;

      _tableName = tableName;
      return this;
    }

    public IFixitDatabaseRequestBuilderFacade WithContainer(IDatabaseTableEntityMediator container)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitDatabaseRequestBuilderFacade)) : _disposedValue;

      _container = container;
      return this;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          Get.Dispose();
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
