using Fixit.Chat.Management.Lib.Facades.Internal;
using Fixit.Chat.Management.Lib.Facades.Options;
using System;

namespace Fixit.Chat.Management.Lib.Facades.Builders.Internal
{
  internal class FixitFunctionInitBuilder : FixitFunctionBuilderFacade, IFunctionInitBuilder
  {
    private bool _disposedValue;

    private FunctionBuilderOptions _functionBuilderOptions;

    public FixitFunctionInitBuilder(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public IFunctionInitBuilder WithOptions(FunctionBuilderOptions functionBuilderOptions)
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitFunctionInitBuilder)) : _disposedValue;
      
      _functionBuilderOptions = functionBuilderOptions;
      return this;
    }

    public FunctionBuilderOptions GetOptions()
    {
      _ = _disposedValue ? throw new ObjectDisposedException(nameof(FixitFunctionInitBuilder)) : _disposedValue;
      return _functionBuilderOptions;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _functionBuilderOptions = null;
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
