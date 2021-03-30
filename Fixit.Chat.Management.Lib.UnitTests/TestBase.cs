using AutoMapper;
using Fixit.Chat.Management.Lib.Helpers;
using Fixit.Chat.Management.Lib.Mappers;
using Fixit.Chat.Management.Lib.Seeders;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.Networking.Local.NMS;
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

    // Networking System Mocks
    protected Mock<IFixNmsHttpClient> _nmsHttpClient;

    // Helper Classes Mocks
    protected Mock<IChatNotificationFactory> _chatNotificationFactory;

    // Mapper
    protected MapperConfiguration _mapperConfiguration = new MapperConfiguration(config =>
    {
      config.AddProfile(new ChatManagementMapper());
    });

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
