﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Fixit.Chat.Management.Lib.UnitTests")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.Triggers")]
namespace Fixit.Chat.Management.Lib.Mediators.Internal
{
  internal class MessagesMediator : IMessagesMediator
  {
    private readonly IDatabaseTableEntityMediator _databaseMessagesTable;

    public MessagesMediator(IDatabaseMediator databaseMediator,
                            IConfiguration configurationProvider)
    {
      var databaseName = configurationProvider["FIXIT-CM-DB-NAME"];
      var databaseMessagesTableName = configurationProvider["FIXIT-CM-DB-MESSAGESTABLE"];

      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects the {nameof(configurationProvider)} to have defined the Chat Management Database as {{FIXIT-CM-DB-NAME}} ");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _databaseMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(databaseMessagesTableName);
    }

    public MessagesMediator(IDatabaseMediator databaseMediator,
                            string databaseName,
                            string messagesTableName)
    {
      if (string.IsNullOrWhiteSpace(databaseName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseName)}... null argument was provided");
      }

      if (string.IsNullOrWhiteSpace(messagesTableName))
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(messagesTableName)}... null argument was provided");
      }

      if (databaseMediator == null)
      {
        throw new ArgumentNullException($"{nameof(MessagesMediator)} expects a value for {nameof(databaseMediator)}... null argument was provided");
      }

      _databaseMessagesTable = databaseMediator.GetDatabase(databaseName).GetContainer(messagesTableName);
    }

    public async Task HandleMessageAsync(UserMessageCreateRequestDto userMessageCreateRequestDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      OperationStatus result;

      var (messageDocumentCollection, token) = await _databaseMessagesTable.GetItemQueryableAsync<MessageDocument>(null, cancellationToken, messageDocument => messageDocument.ConversationId == userMessageCreateRequestDto.ConversationId);
      if (messageDocumentCollection.IsOperationSuccessful)
      {
        MessageDocument messageDocument = messageDocumentCollection.Results.SingleOrDefault();
        if (messageDocument == default(MessageDocument))
        {
          var currentTime = DateTimeOffset.Now;

          MessageDocument newMessageDocument = new MessageDocument()
          {
            ConversationId = userMessageCreateRequestDto.ConversationId,
            Messages = new List<MessageDto>()
            {
              userMessageCreateRequestDto.Message
            }
          };
          result = await _databaseMessagesTable.CreateItemAsync(newMessageDocument, currentTime.ToString("yyyy-MM"), cancellationToken);
        }
        else
        {
          messageDocument.Messages.Add(userMessageCreateRequestDto.Message);
          result = await _databaseMessagesTable.UpdateItemAsync(messageDocument, messageDocument.EntityId, cancellationToken);
        }

        if (result.IsOperationSuccessful)
        {
          // TODO: (#503) Notify receivers
        }
      }
    }
  }
}
