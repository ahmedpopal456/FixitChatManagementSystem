using Fixit.Chat.Management.Lib.Facades.Options;
using System;

namespace Fixit.Chat.Management.Lib.Facades.Builders
{
  // TODO: Add Summaries
  public interface IFunctionInitBuilder : IDisposable, IFixitFunctionBuilderFacade
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="functionBuilderOptions"></param>
    /// <returns></returns>
    IFunctionInitBuilder WithOptions(FunctionBuilderOptions functionBuilderOptions);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public FunctionBuilderOptions GetOptions();
  }
}
