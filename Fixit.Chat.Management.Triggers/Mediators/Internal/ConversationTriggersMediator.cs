using AutoMapper;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Documents;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Users;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Mediators.Conversations.Internal;
using Fixit.Chat.Management.Lib.Facades;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib;
using Fixit.Core.Database.DataContracts.Documents;

namespace Fixit.Chat.Management.Triggers.Mediators.Internal
{
  internal class ConversationTriggersMediator : ConversationBaseMediator, IConversationTriggersMediator
  {
    private readonly IConfiguration _configuration;

    public ConversationTriggersMediator(IConfiguration configuration,
                                        ILoggerFactory loggerFactory,
                                        IFixitFacadeFactory empowerFacadeFactory,
                                        IMapper mapper,
    DocumentDbTableEntityResolver documentDbTableEntityResolver) : base(configuration, loggerFactory, empowerFacadeFactory, mapper, documentDbTableEntityResolver)
    {
      _configuration = configuration ?? throw new ArgumentNullException($"{nameof(ConversationTriggersMediator)} expects an argument for {nameof(configuration)}. Null argument was provided.");
    }

    public async Task<OperationStatusWithObject<ConversationDto>> AddConversationParticipantsByBulkIdAsync(AddConversationParticipantsRequestDto addConversationParticipantsRequestDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = default(OperationStatusWithObject<ConversationDto>);

      var expression = PredicateBuilder.New((Expression<Func<ConversationDocument, bool>>)(document => document.id == addConversationParticipantsRequestDto.ConversationId.ToString()));
      var conversationGetResponse = await _fixitFunctionBuilderFacade.DatabaseRequest
                                                 .Get
                                                 .GetAsync<ConversationDocument>(expression, cancellationToken, null, (document) => document != null)
                                                 .ResolveContextAsync();

      if (conversationGetResponse.Result != null && conversationGetResponse.Result.FetchedResult != null)
      {
        var documentToUpdate = conversationGetResponse.Result.FetchedResult;

        List<Task> searchParticipants = new List<Task>();

        foreach (var participant in addConversationParticipantsRequestDto.Participants)
        {
          if (documentToUpdate.Participants == null || !documentToUpdate.Participants.Any(p => p.User?.Id == participant.Id))
          {
            searchParticipants.Add(Task.Run(async () =>
            {
              documentToUpdate.Participants.Add(new ParticipantDto()
              {
                User = _mapper.Map<UserSummaryDto, UserBaseDto>(participant)
              });
            }));
          }
        }
        await Task.WhenAll(searchParticipants);

        var updateResponse = await _conversationsTableManager.UpsertItemAsync(documentToUpdate, documentToUpdate.EntityId, cancellationToken);
        result = _mapper.Map<OperationStatus, OperationStatusWithObject<ConversationDto>>(updateResponse);
        if (result.IsOperationSuccessful)
        {
          result.Result = _mapper.Map<ConversationDocument, ConversationDto>(documentToUpdate);
        }
      }
      return result;
    }

    public async Task<OperationStatusWithObject<ConversationDto>> UpdateConversationLastMessageAsync(Guid conversationId, ConversationMessageDto lastConversationMessageDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = default(OperationStatusWithObject<ConversationDto>);

      var expression = PredicateBuilder.New((Expression<Func<ConversationDocument, bool>>)(document => document.id == conversationId.ToString()));
      var conversationGetResponse = await _fixitFunctionBuilderFacade.DatabaseRequest
                                                 .Get
                                                 .GetAsync<ConversationDocument>(expression, cancellationToken, null, (document) => document != null)
                                                 .ResolveContextAsync();

      if (conversationGetResponse.Result is { FetchedResult: { } })
      {
        var documentToUpdate = conversationGetResponse.Result.FetchedResult;

        documentToUpdate.LastMessage = lastConversationMessageDto;
        documentToUpdate.StampUpdatedTimestampUtc();

        var receivingConversationParticipants = documentToUpdate.Participants?.Where(participant => participant.User?.Id != lastConversationMessageDto.CreatedByUser.Id);
        var receivingConversationParticipantsEnumerator = receivingConversationParticipants.GetEnumerator();
        while (receivingConversationParticipantsEnumerator.MoveNext())
        {
          var participant = receivingConversationParticipantsEnumerator.Current;
          participant.HasUnreadMessages = true;
        }

        var updateResponse = await _conversationsTableManager.UpsertItemAsync(documentToUpdate, documentToUpdate.EntityId, cancellationToken);

        result = _mapper.Map<OperationStatus, OperationStatusWithObject<ConversationDto>>(updateResponse);
        if (result.IsOperationSuccessful)
        {
          result.Result = _mapper.Map<ConversationDocument, ConversationDto>(documentToUpdate);
        }
      }
      return result;
    }
  }
}
