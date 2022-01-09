using Fixit.Chat.Management.Lib.Facades.Enums;
using Fixit.Core.Database.DataContracts.Documents;
using System;
using System.Collections.Generic;

namespace Fixit.Chat.Management.Lib.Facades.Contexts
{
  public class DatabaseMultiGetRequestContext<T> : DatabaseRequestContext, IDisposable
  {
    private bool _disposedValue = false;

    public DatabaseMultiGetRequestContext() : base() { }

    public MultiFetchTypes FetchTypes { get; set; }

    public List<T> FetchedResultsMany { get; set; }

    public PagedDocumentCollectionDto<T> FetchedResultsByPage { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          FetchedResultsMany.Clear();
          FetchedResultsMany = null;
          FetchedResultsByPage.Results?.Clear();
          FetchedResultsByPage = null; 
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