using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Internal;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
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
    private IEnumerable<MessageDocument> _fakeMessageDocuments;
    private IEnumerable<UserMessageCreateRequestDto> _fakeUserMessageCreateRequestDtos;

    // DB and table name

    private readonly string _chatDatabaseName = "ChatDatabaseName";
    private readonly string _conversationsDatabaseTableName = "ConversationsTableName";
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
      _fakeConversationDocuments = _fakeDtoSeedFactory.CreateSeederFactory<ConversationDocument>(new ConversationDocument());
      _fakeConversationCreateRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<ConversationCreateRequestDto>(new ConversationCreateRequestDto());
      _fakeMessageDocuments = _fakeDtoSeedFactory.CreateSeederFactory<MessageDocument>(new MessageDocument());
      _fakeUserMessageCreateRequestDtos = _fakeDtoSeedFactory.CreateSeederFactory<UserMessageCreateRequestDto>(new UserMessageCreateRequestDto());

      _databaseMediator.Setup(databaseMediator => databaseMediator.GetDatabase(_chatDatabaseName))
                       .Returns(_databaseTableMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_conversationsDatabaseTableName))
                       .Returns(_conversationsTableEntityMediator.Object);
      _databaseTableMediator.Setup(databaseTableMediator => databaseTableMediator.GetContainer(_messagesDatabaseTableName))
                       .Returns(_messagesTableEntityMediator.Object);

      _chatMediator = new ChatMediator(_databaseMediator.Object,
                                       _chatDatabaseName,
                                       _conversationsDatabaseTableName,
                                       _messagesDatabaseTableName);
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

      _conversationsTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<ConversationDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<ConversationDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => conversationDocument = document)
                                  .ReturnsAsync(new CreateDocumentDto<ConversationDocument>() { Document = _fakeConversationDocuments.First() });

      //Act
      await _chatMediator.CreateConversationAsync(conversationCreateRequestDto, cancellationToken);

      //Assert
      Assert.AreEqual(conversationDocument.FixInstanceId, conversationCreateRequestDto.FixInstanceId);
      Assert.AreEqual(conversationDocument.Participants.Count, conversationCreateRequestDto.Participants.Count);
    }
    #endregion

    #region HandleMessageAsync
    [TestMethod]
    public async Task HandleMessageAsync_GetMessageDocumentNull_CreateMessageDocument()
    {
      //Arrange
      var cancellationToken = CancellationToken.None;
      var userMessageCreateRequestDto = _fakeUserMessageCreateRequestDtos.First();

      var newMessageDocument = new MessageDocument();

      var messageDocumentCollection = new DocumentCollectionDto<MessageDocument>()
      {
        IsOperationSuccessful = true,
        Results = null
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<MessageDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<MessageDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<MessageDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => newMessageDocument = document)
                                  .ReturnsAsync(new CreateDocumentDto<MessageDocument>() { Document = _fakeMessageDocuments.First() });

      //Act
      await _chatMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.CreateItemAsync(It.IsAny<MessageDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
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

      var updatedMessageDocument = new MessageDocument();

      var messageDocumentCollection = new DocumentCollectionDto<MessageDocument>()
      {
        IsOperationSuccessful = true,
        Results = new List<MessageDocument>() { _fakeMessageDocuments.First() }
      };

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.GetItemQueryableAsync(null, It.IsAny<CancellationToken>(), It.IsAny<Expression<Func<MessageDocument, bool>>>(), null))
                                  .ReturnsAsync((messageDocumentCollection, null));

      _messagesTableEntityMediator.Setup(databaseTableEntityMediator => databaseTableEntityMediator.UpdateItemAsync(It.IsAny<MessageDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .Callback<MessageDocument, string, CancellationToken>((document, partitionKey, cancellationToken) => updatedMessageDocument = document)
                                  .ReturnsAsync(new CreateDocumentDto<MessageDocument>() { Document = _fakeMessageDocuments.First() });

      //Act
      await _chatMediator.HandleMessageAsync(userMessageCreateRequestDto, cancellationToken);

      //Assert
      _messagesTableEntityMediator.Verify(databaseTableEntityMediator => databaseTableEntityMediator.UpdateItemAsync(It.IsAny<MessageDocument>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
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
      _conversationsTableEntityMediator.Reset();

      // Clean-up data objects
      _fakeConversationDocuments = null;
      _fakeConversationCreateRequestDtos = null;
    }
    #endregion
  }
}
