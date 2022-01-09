using System;

namespace Fixit.Chat.Management.Lib.Facades.Contexts
{
  public class DatabaseGetRequestContext<T> : DatabaseRequestContext, IDisposable
  {
    private bool _disposedValue = false; 

    public DatabaseGetRequestContext() : base() { }

    public T FetchedResult { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          FetchedResult = default(T);
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