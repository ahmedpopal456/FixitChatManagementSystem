using Fixit.Chat.Management.Lib.Facades.Builders;
using System;

namespace Fixit.Chat.Management.Lib.Facades
{
  // TODO: Add Summaries
  public interface IFixitFunctionBuilderFacade : IDisposable
  {
    /// <summary>
    /// 
    /// </summary>
    public IFunctionInitBuilder Init { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public IFixitDatabaseRequestBuilderFacade DatabaseRequest { get; set; }
  }
}
