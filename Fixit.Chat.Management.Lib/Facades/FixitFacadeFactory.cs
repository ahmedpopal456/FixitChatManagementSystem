using Fixit.Chat.Management.Lib.Facades.Internal;
using Fixit.Chat.Management.Lib.Facades.Options;
using System;

namespace Fixit.Chat.Management.Lib.Facades
{
  // TODO: Add an extension to ensure that interfaces for the facade are available when needed
  public class FixitFacadeFactory : IFixitFacadeFactory
  {
    private IFixitFunctionBuilderFacade _empowerFunctionBuilderFacade { get; set; }

    readonly IServiceProvider _serviceProvider;

    public FixitFacadeFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitFacadeFactory)} expects a value for {nameof(serviceProvider)}... null argument was provided");
    }

    public IFixitFunctionBuilderFacade CreateEmpowerFunctionBuilderFacade(FunctionBuilderOptions functionBuilderOptions)
    {
      if (_empowerFunctionBuilderFacade == null)
      {
        _empowerFunctionBuilderFacade = new FixitFunctionBuilderFacade(_serviceProvider);
        _empowerFunctionBuilderFacade
          .Init
            .WithOptions(functionBuilderOptions);
      }

      return _empowerFunctionBuilderFacade;
    }
  }
}
