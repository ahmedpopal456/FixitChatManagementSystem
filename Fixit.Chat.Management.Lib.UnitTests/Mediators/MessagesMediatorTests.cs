using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fixit.Chat.Management.Lib.UnitTests.Mediators
{
  [TestClass]
  public class MessagesMediatorTests : TestBase
  {
    private MessagesMediator _messagesMediator;

    // Fake data
    private IEnumerable<ConversationMessagesDocument> _fakeMessageDocuments;
    private IEnumerable<UserMessageCreateRequestDto> _fakeUserMessageCreateRequestDtos;

    // DB and table name

    private readonly string _chatDatabaseName = "ChatDatabaseName";
    private readonly string _messagesDatabaseTableName = "MessagesTableName";

    #region TestInitialize
    [TestInitialize]
    public void TestInitialize()
    {
      // Setup all needed Interfaces to project test controllers
      _configuration = new Mock<IConfiguration>();
      _databaseMediator = new Mock<IDatabaseMediator>();
      _databaseTableMediator = new Mock<IDatabaseTableMediator>();
      _conversationsTableEntityMediator = new Mock<IDatabaseTableEntityMediator>();
      _messagesTableEntityMediator = new Mock<IDatabaseTableEntityMediator>();

      // Create fake data objects
      _fakeMessageDocuments = _fakeDtoSeedFactory.CreateSeederFactory<ConversationMessagesDocument>(new ConversationMessagesDocument());
      _fakeUserMessageCreateRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<UserMessageCreateRequestDto>(new UserMessageCreateRequestDto());

      _databaseMediator.Setup(databaseMediator => databaseMediator.GetDatabase(_chatDatabaseName))
                       .Returns(_databaseTableMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_messagesDatabaseTableName))
                       .Returns(_messagesTableEntityMediator.Object);

      _messagesMediator = new MessagesMediator(_databaseMediator.Object,
                                               _chatDatabaseName,
                                               _messagesDatabaseTableName);
    }
    #endregion

    #region HandleMessageAsync
    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentFailure_DoNothing()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.First();

      var newMessageDocument = new ConversationMessagesDocument();

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = false
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      //Act
      await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentNull_NewMessageDocument()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.First();

      var newMessageDocument = new ConversationMessagesDocument();

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = true,
        Results = null
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationMessagesDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => newMessageDocument = document)
                                  .ReturnsAsync(new OperationStatus() { IsOperationSuccessful = true });

      //Act
      await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once()); ;
      Assert.AreEqual(newMessageDocument.ConversationId, userMessageCreateRequestDto.ConversationId);
      Assert.AreEqual(newMessageDocument.Messages.Count, 1);
      Assert.IsTrue(newMessageDocument.Messages.Contains(userMessageCreateRequestDto.Message));
    }

    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentSuccess_UpdateMessageDocument()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.Last();

      var updatedMessageDocument = new ConversationMessagesDocument();

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = true,
        Results = new List<ConversationMessagesDocument>() { _fakeMessageDocuments.First() }
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationMessagesDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => updatedMessageDocument = document)
                                  .ReturnsAsync(new OperationStatus() { IsOperationSuccessful = true });

      //Act
      await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());;
      Assert.AreEqual(updatedMessageDocument.Messages.Count, 2);
      Assert.IsTrue(updatedMessageDocument.Messages.Contains(userMessageCreateRequestDto.Message));
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
      _messagesTableEntityMediator.Reset();

      // Clean-up data objects
      _fakeMessageDocuments = null;
      _fakeUserMessageCreateRequestDtos = null;
    }
    #endregion
  }
}
