using Fixit.Chat.Management.Lib.Facades.Builders;
using Fixit.Chat.Management.Lib.Facades.Builders.Internal;
using System;

namespace Fixit.Chat.Management.Lib.Facades.Internal
{
  internal class FixitFunctionBuilderFacade : IFixitFunctionBuilderFacade
  {
    private bool _disposedValue;

    private IFunctionInitBuilder _init;
    private IFixitDatabaseRequestBuilderFacade _databaseRequest;

    readonly IServiceProvider _serviceProvider;

    public FixitFunctionBuilderFacade(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitFunctionBuilderFacade)} expects a value for {nameof(serviceProvider)}... null argument was provided");
    }
     
    public IFunctionInitBuilder Init
    {
      get => _init ??= new FixitFunctionInitBuilder(_serviceProvider);
      set => _init = value;
    }

    public IFixitDatabaseRequestBuilderFacade DatabaseRequest
    {
      get => _databaseRequest ??= (IFixitDatabaseRequestBuilderFacade) new FixitDatabaseRequestBuilderFacade(_serviceProvider, Init.GetOptions());
      set => _databaseRequest = value;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          Init.Dispose();
          DatabaseRequest.Dispose(); 
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
