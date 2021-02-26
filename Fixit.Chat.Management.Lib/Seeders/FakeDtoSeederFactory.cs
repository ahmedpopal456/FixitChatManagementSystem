using Fixit.Core.DataContracts.Seeders;
using System.Collections.Generic;

namespace Fixit.Chat.Management.Lib.Seeders
{
  public class FakeDtoSeederFactory : IFakeSeederFactory
  {
    public IList<T> CreateSeederFactory<T>(IFakeSeederAdapter<T> fakeSeederAdapter) where T : class
    {
      return fakeSeederAdapter.SeedFakeDtos();
    }
  }
}
