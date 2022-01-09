using Fixit.Chat.Management.Lib.Facades.Options;

namespace Fixit.Chat.Management.Lib.Facades
{
  // TODO: Add Summaries
  public interface IFixitFacadeFactory
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="functionBuilderOptions"></param>
    /// <returns></returns>
    public IFixitFunctionBuilderFacade CreateEmpowerFunctionBuilderFacade(FunctionBuilderOptions functionBuilderOptions);
  }
}
