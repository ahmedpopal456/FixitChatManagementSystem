using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Helpers;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.DataContracts.Users.Enums;
using Fixit.Core.Networking.Local.NMS;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Fixit.Chat.Management.Lib.UnitTests.Mediators
{
  [TestClass]
  public class ConversationsMediatorTests : TestBase
  {
    private ConversationsMediator _conversationsMediator;

    // Fake data
    private IEnumerable<ConversationDocument> _fakeConversationDocuments;
    private IEnumerable<ConversationCreateRequestDto> _fakeConversationCreateRequestDtos;
    private IEnumerable<EnqueueNotificationRequestDto> _fakeEnqueueNotificationRequestDtos;

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
      _conversationsTableEntityMediator = new Mock<IDatabaseTableEntityMediator>();
      _nmsHttpClient = new Mock<IFixNmsHttpClient>();
      _chatNotificationFactory = new Mock<IChatNotificationFactory>();

      // Create fake data objects
      _fakeConversationDocuments = _fakeDtoSeedFactory.CreateSeederFactory<ConversationDocument>(new ConversationDocument());
      _fakeConversationCreateRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<ConversationCreateRequestDto>(new ConversationCreateRequestDto());
      _fakeEnqueueNotificationRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<EnqueueNotificationRequestDto>(new EnqueueNotificationRequestDto());

      _databaseMediator.Setup(databaseMediator => databaseMediator.GetDatabase(_chatDatabaseName))
                       .Returns(_databaseTableMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_conversationsDatabaseTableName))
                            .Returns(_conversationsTableEntityMediator.Object);

      _conversationsMediator = new ConversationsMediator(_databaseMediator.Object,
                                                         _chatNotificationFactory.Object,
                                                         _nmsHttpClient.Object,
                                                         _chatDatabaseName,
                                                         _conversationsDatabaseTableName);
    }
    #endregion

    #region GetConversationsAsync
    [TestMethod]
    [DataRow("8b418766-4a99-42a8-b6d7-9fe52b88ea93", DisplayName = "Any_UserId")]
    public async Task GetConversationsAsync_GetConversationFailure_ReturnsFailure(string userId)
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      Guid userIdGuid = new Guid(userId);
      var conversationDocumentCollection = new DocumentCollectionDto<ConversationDocument>()
      {
        IsOperationSuccessful = false
      };

      _conversationsTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationDocument, bool>>>(), null))
                                       .ReturnsAsync((conversationDocumentCollection, null));

      //Act
      var actionResult = await _conversationsMediator.GetConversationsAsync(userIdGuid, cancellationToken);

      //Assert
      Assert.IsFalse(actionResult.IsOperationSuccessful);
    }

    [TestMethod]
    [DataRow("8b418766-4a99-42a8-b6d7-9fe52b88ea93", DisplayName = "Any_UserId")]
    public async Task GetConversationsAsync_GetConversationSuccess_ReturnsSuccess(string userId)
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      Guid userIdGuid = new Guid(userId);
      var conversationDocumentCollection = new DocumentCollectionDto<ConversationDocument>()
      {
        Results = new List<ConversationDocument>() { _fakeConversationDocuments.First() },
        IsOperationSuccessful = true
      };

      _conversationsTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<ConversationDocument, bool>>>(), null))
                                       .ReturnsAsync((conversationDocumentCollection, null));

      //Act
      var actionResult = await _conversationsMediator.GetConversationsAsync(userIdGuid, cancellationToken);

      //Assert
      Assert.IsTrue(actionResult.IsOperationSuccessful);
      Assert.IsTrue(actionResult.Results.Count > 0);
    }
    #endregion

    #region CreateConversationAsync
    [TestMethod]
    public async Task CreateConversationAsync_CreateRequestSuccess_SendNotification()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var conversationCreateRequestDto = _fakeConversationCreateRequestDtos.First();
      var conversationDocument = new ConversationDocument();
      var operationStatus = new OperationStatus() { IsOperationSuccessful = true };

      _conversationsTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<ConversationDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                       .Callback<ConversationDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => conversationDocument = document)
                                       .ReturnsAsync(new CreateDocumentDto<ConversationDocument>() { IsOperationSuccessful = true, Document = _fakeConversationDocuments.First() });
      _chatNotificationFactory.Setup(factory => factory.CreateClientConversationNotificationDto(It.IsAny<ConversationDocument>()))
                              .Returns(_fakeEnqueueNotificationRequestDtos.First());
      _chatNotificationFactory.Setup(factory => factory.CreateCraftsmanConversationNotificationDto(It.IsAny<ConversationDocument>()))
                              .Returns(_fakeEnqueueNotificationRequestDtos.First());
      _nmsHttpClient.Setup(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(operationStatus);

      //Act
      await _conversationsMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);

      //Assert
      Assert.AreEqual(conversationDocument.FixInstanceId, conversationCreateRequestDto.FixInstanceId);
      Assert.AreEqual(conversationDocument.Participants.Count, conversationCreateRequestDto.Participants.Count);
      _nmsHttpClient.Verify(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task CreateConversationAsync_CreateRequestFailure_NoNotification()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var conversationCreateRequestDto = _fakeConversationCreateRequestDtos.First();
      var operationStatus = new OperationStatus() { IsOperationSuccessful = true };

      _conversationsTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<ConversationDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(new CreateDocumentDto<ConversationDocument>() { IsOperationSuccessful = false });
      _chatNotificationFactory.Setup(factory => factory.CreateClientConversationNotificationDto(It.IsAny<ConversationDocument>()))
                              .Returns(_fakeEnqueueNotificationRequestDtos.First());
      _chatNotificationFactory.Setup(factory => factory.CreateCraftsmanConversationNotificationDto(It.IsAny<ConversationDocument>()))
                              .Returns(_fakeEnqueueNotificationRequestDtos.First());
      _nmsHttpClient.Setup(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(operationStatus);

      //Act
      await _conversationsMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);

      //Assert
      _nmsHttpClient.Verify(httpClient => httpClient.PostNotification(It.IsAny<EnqueueNotificationRequestDto>(), It.IsAny<CancellationToken>()), Times.Never());
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
      _conversationsTableEntityMediator.Reset();
      _nmsHttpClient.Reset();
      _chatNotificationFactory.Reset();

      // Clean-up data objects
      _fakeConversationDocuments = null;
      _fakeConversationCreateRequestDtos = null;
      _fakeEnqueueNotificationRequestDtos = null;
    }
    #endregion
  }
}
