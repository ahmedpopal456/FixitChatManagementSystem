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
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.Networking.Local.NMS;
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
      _messagesTableEntityMediator = new Mock<IDatabaseTableEntityMediator>();
      _nmsHttpClient = new Mock<IFixNmsHttpClient>();

      // Create fake data objects
      _fakeMessageDocuments = _fakeDtoSeedFactory.CreateSeederFactory<ConversationMessagesDocument>(new ConversationMessagesDocument());
      _fakeUserMessageCreateRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<UserMessageCreateRequestDto>(new UserMessageCreateRequestDto());

      _databaseMediator.Setup(databaseMediator => databaseMediator.GetDatabase(_chatDatabaseName))
                       .Returns(_databaseTableMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_messagesDatabaseTableName))
                            .Returns(_messagesTableEntityMediator.Object);

      _messagesMediator = new MessagesMediator(_mapperConfiguration.CreateMapper(),
                                               _databaseMediator.Object,
                                               _nmsHttpClient.Object,
                                               _chatDatabaseName,
                                               _messagesDatabaseTableName);
    }
    #endregion

    #region GetMessagesAsync
    [TestMethod]
    [DataRow("bb84f7d6-7d10-4fd0-85b2-6eba8cc3c896", DisplayName = "Any_ConversationId")]
    public async Task GetMessagesAsync_GetMessageDocumentFailure_ReturnsFailure(string conversationId)
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      Guid conversationIdGuid = new Guid(conversationId);
      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = false
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      //Act
      var actionResult = await _messagesMediator.GetMessagesAsync(conversationIdGuid, 1, 3, cancellationToken);

      //Assert
      Assert.IsFalse(actionResult.IsOperationSuccessful);
    }

    [TestMethod]
    [DataRow(1, 5, DisplayName = "Page1_Size5")]
    [DataRow(3, 2, DisplayName = "Page3_Size2")]
    [DataRow(2, 1, DisplayName = "Page2_Size1")]
    [DataRow(2, 5, DisplayName = "Page2_Size5")]
    [DataRow(2, 4, DisplayName = "Page2_Size4")]
    public async Task GetMessagesAsync_GetMessageDocumentSuccess_ReturnsSuccess(int pageNumber, int pageSize) 
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      Guid conversationIdGuid = new Guid("bb84f7d6-7d10-4fd0-85b2-6eba8cc3c896");
      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        Results = new List<ConversationMessagesDocument>() { _fakeMessageDocuments.Last() },
        IsOperationSuccessful = true
      };
      int messageCount = _fakeMessageDocuments.Last().Messages.Count;

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      //Act
      var actionResult = await _messagesMediator.GetMessagesAsync(conversationIdGuid, pageNumber, pageSize, cancellationToken);

      //Assert
      Assert.IsTrue(actionResult.IsOperationSuccessful);
      Assert.AreEqual(actionResult.Messages.Count, pageNumber * pageSize > messageCount ? messageCount % pageSize : pageSize);
    }

    [TestMethod]
    [DataRow(3, 3, DisplayName = "Page3_Size3")]
    [DataRow(4, 2, DisplayName = "Page4_Size2")]
    public async Task GetMessagesAsync_GetMessagesPageEmpty_ReturnsEmpty(int pageNumber, int pageSize)
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      Guid conversationIdGuid = new Guid("bb84f7d6-7d10-4fd0-85b2-6eba8cc3c896");
      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        Results = new List<ConversationMessagesDocument>() { _fakeMessageDocuments.Last() },
        IsOperationSuccessful = true
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      //Act
      var actionResult = await _messagesMediator.GetMessagesAsync(conversationIdGuid, pageNumber, pageSize, cancellationToken);

      //Assert
      Assert.IsTrue(actionResult.IsOperationSuccessful);
      Assert.AreEqual(actionResult.Messages.Count, 0);
    }
    #endregion

    #region HandleMessageAsync
    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentFailure_ReturnsFailure()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.First();
      var newMessageDocument = new ConversationMessagesDocument();
      var operationStatus = new OperationStatus() { IsOperationSuccessful = true };

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = false
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));
      _nmsHttpClient.Setup(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(operationStatus);

      //Act
      var actionResult = await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      Assert.IsFalse(actionResult.IsOperationSuccessful);
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
      _nmsHttpClient.Verify(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentNull_NewMessageDocument()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.First();
      var newMessageDocument = new ConversationMessagesDocument();
      var operationStatus = new OperationStatus() { IsOperationSuccessful = true };

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = true,
        Results = null
      };

      var operationStatusSuccess = new OperationStatus() { IsOperationSuccessful = true };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));
      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationMessagesDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => newMessageDocument = document)
                                  .ReturnsAsync(operationStatusSuccess);
      _nmsHttpClient.Setup(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(operationStatus);

      //Act
      var actionResult = await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      Assert.IsTrue(actionResult.IsOperationSuccessful);
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
      _nmsHttpClient.Verify(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()), Times.Once());
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
      var operationStatus = new OperationStatus() { IsOperationSuccessful = true };

      var messageDocumentCollection = new DocumentCollectionDto<ConversationMessagesDocument>()
      {
        IsOperationSuccessful = true,
        Results = new List<ConversationMessagesDocument>() { _fakeMessageDocuments.First() }
      };

      var operationStatusSuccess = new OperationStatus() { IsOperationSuccessful = true };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationMessagesDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));
      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationMessagesDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => updatedMessageDocument = document)
                                  .ReturnsAsync(operationStatusSuccess);
      _nmsHttpClient.Setup(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(operationStatus);

      //Act
      var actionResult = await _messagesMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      Assert.IsTrue(actionResult.IsOperationSuccessful);
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpsertItemAsync(It.IsAny<ConversationMessagesDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once()); ;
      _nmsHttpClient.Verify(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()), Times.Once());
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
      _nmsHttpClient.Reset();

      // Clean-up data objects
      _fakeMessageDocuments = null;
      _fakeUserMessageCreateRequestDtos = null;
    }
    #endregion
  }
}
