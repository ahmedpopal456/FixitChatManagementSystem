using Fixit.Chat.Management.Lib.Seeders;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fixit.Chat.Management.Lib.UnitTests
{
  [TestClass]
  public class TestBase
  {
    public IFakeSeederFactory _fakeDtoSeedFactory;

    // Main Object Mocks
    protected Mock<IConfiguration> _configuration;

    // Database System Mocks
    protected Mock<IDatabaseMediator> _databaseMediator;
    protected Mock<IDatabaseTableMediator> _databaseTableMediator;
    protected Mock<IDatabaseTableEntityMediator> _conversationsTableEntityMediator;
    protected Mock<IDatabaseTableEntityMediator> _messagesTableEntityMediator;

    public TestBase()
    {
      _fakeDtoSeedFactory = new FakeDtoSeederFactory();
    }

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext testContext)
    {
    }

    [AssemblyCleanup]
    public static void AfterSuiteTests()
    {
    }
  }
}
