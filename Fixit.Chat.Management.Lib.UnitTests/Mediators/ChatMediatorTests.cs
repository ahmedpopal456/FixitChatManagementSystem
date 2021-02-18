using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fixit.Chat.Management.Lib.UnitTests.Mediators
{
  [TestClass]
  public class ChatMediatorTests : TestBase
  {
    private ChatMediator _chatMediator;

    // Fake data
    private IEnumerable<ConversationDocument> _fakeConversationDocuments;
    private IEnumerable<ConversationCreateRequestDto> _fakeConversationCreateRequestDtos;

    // DB and table name

    private readonly string _chatDatabaseName = "ChatDatabaseName";
    private readonly string _conversationsDatabaseTableName = "ConversationsTableName";

    #region TestInitialize
    [TestInitialize]
    public void TestInitialize()
    {
      // Setup all needed Interfaces to project test controllers
      _configuration = new Mock<IConfiguration>();
      _databaseMediator = new Mock<IDatabaseMediator>();
      _databaseTableMediator = new Mock<IDatabaseTableMediator>();
      _databaseTableEntityMediator = new Mock<IDatabaseTableEntityMediator>();

      // Create Seeders
      var fakeConversationDocumentSeeder = _fakeDtoSeedFactory.CreateFakeSeeder<ConversationDocument>();
      var fakeConversationCreateRequestDtoSeeder = _fakeDtoSeedFactory.CreateFakeSeeder<ConversationCreateRequestDto>();

      // Create fake data objects
      _fakeConversationDocuments = fakeConversationDocumentSeeder.SeedFakeDtos();
      _fakeConversationCreateRequestDtos = fakeConversationCreateRequestDtoSeeder.SeedFakeDtos();

      _databaseMediator.Setup(databaseMediator => databaseMediator.GetDatabase(_chatDatabaseName))
                       .Returns(_databaseTableMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_conversationsDatabaseTableName))
                       .Returns(_databaseTableEntityMediator.Object);


      _chatMediator = new ChatMediator(_mapperConfiguration.CreateMapper(),
                                       _databaseMediator.Object,
                                       _chatDatabaseName,
                                       _conversationsDatabaseTableName);
    }
    #endregion

    #region CreateConversationAsync
    [TestMethod]
    public async Task CreateConversationAsync_CreateRequestSuccess_ReturnsSuccess()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var conversationCreateRequestDto = _fakeConversationCreateRequestDtos.First();

      var conversationDocument = new ConversationDocument();

      _databaseTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<ConversationDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => conversationDocument = document)
                                  .ReturnsAsync(new CreateDocumentDto<ConversationDocument>() { Document = _fakeConversationDocuments.First() });

      //Act
      await _chatMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);

      //Assert
      Assert.AreEqual(conversationCreateRequestDto.FixInstanceId, conversationDocument.FixInstanceId);
      Assert.AreEqual(conversationCreateRequestDto.Participants.Count, conversationDocument.Participants.Count);
    }
    #endregion

    #region TestCleanup
    [TestCleanup]
    public void TestCleanup()
    {
      // Clean-up mock objects
      _configuration.Reset();
      _databaseMediator.Reset();
      _databaseTableMediator.Reset();
      _databaseTableEntityMediator.Reset();

      // Clean-up data objects
      _fakeConversationDocuments = null;
      _fakeConversationCreateRequestDtos = null;
    }
    #endregion
  }
}
