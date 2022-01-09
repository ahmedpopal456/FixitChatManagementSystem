using AutoMapper;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Documents;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Users;
using LinqKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Facades;
using Fixit.Chat.Management.Lib.Facades.Options;
using Fixit.Chat.Management.Lib.Extensions;
using Fixit.Chat.Management.Lib.Enums;
using Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos;
using Fixit.Chat.Management.Lib.QueryBuilders;

[assembly: InternalsVisibleTo("Fixit.Chat.Management.Triggers")]
[assembly: InternalsVisibleTo("Fixit.Chat.Management.ServerlessApi")]

namespace Fixit.Chat.Management.Lib.Mediators.Conversations.Internal
{
  internal class ConversationBaseMediator : IConversationBaseMediator
  {
    protected readonly IMapper _mapper;
    protected readonly IDatabaseTableEntityMediator _conversationsTableManager;
    protected readonly IFixitFunctionBuilderFacade _fixitFunctionBuilderFacade;

    public ConversationBaseMediator(IConfiguration configuration,
                                    ILoggerFactory loggerFactory,
                                    IFixitFacadeFactory empowerFacadeFactory,
                                    IMapper mapper,
                                    DocumentDbTableEntityResolver documentDbTableEntityResolver)
    {
      var databaseName = configuration["FIXIT-CHMS-DB-NAME"];
      var conversationsContainerName = configuration["FIXIT-CHMS-TABLE-CONVERSATIONS-NAME"];
      _ = string.IsNullOrWhiteSpace(conversationsContainerName) ? throw new ArgumentNullException($"{nameof(ConversationBaseMediator)} expects the {nameof(configuration)} to have defined the Database Conversations Table as {{FIXIT-CHMS-TABLE-CONVERSATIONS-NAME}} ") : string.Empty;
      _ = documentDbTableEntityResolver ?? throw new ArgumentNullException($"{nameof(ConversationBaseMediator)} expects a value for {nameof(documentDbTableEntityResolver)}... null argument was provided");

      _conversationsTableManager = documentDbTableEntityResolver(conversationsContainerName);
      _mapper = mapper ?? throw new ArgumentNullException($"{nameof(ConversationBaseMediator)} could not retrieve a valid value from {mapper}");
      _fixitFunctionBuilderFacade = empowerFacadeFactory.CreateEmpowerFunctionBuilderFacade(new FunctionBuilderOptions() { HandleExceptions = true, LogErrors = true }) ?? throw new ArgumentNullException($"{nameof(ConversationBaseMediator)} could not retrieve a valid value from {empowerFacadeFactory}");
      _fixitFunctionBuilderFacade.DatabaseRequest
                                   .WithContainer(_conversationsTableManager);
    }

    public async Task<OperationStatusWithObject<IEnumerable<ConversationDto>>> GetConversationsByQueryAsync(string searchString, ConversationQueryDto conversationQueryDto, CancellationToken cancellationToken, string orderField = null, OrderDirections? orderDirection = OrderDirections.Asc)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = new OperationStatusWithObject<IEnumerable<ConversationDto>> { IsOperationSuccessful = true };

      var conversationsGetResponse = await _fixitFunctionBuilderFacade.DatabaseRequest
                                            .Get
                                            .GetManyAsync<ConversationDocument>(GetConversationsSearchQuery(searchString, conversationQueryDto), cancellationToken, null, orderField, orderDirection.GetValueOrDefault(), (document) => document.Results != null && document.Results.Any())
                                            .ForEachExecuteWithReturnAsync((document) => _mapper.Map<ConversationDocument, ConversationDto>(document));

      result.Result = conversationsGetResponse.Where(clientUser => clientUser.IsOperationSuccessful && clientUser.Result != null)
                                              .Select(clientUser => clientUser.Result).ToList();

      return result;
    }

    public async Task<OperationStatusWithObject<ConversationDto>> CreateConversationAsync(ConversationCreateRequestDto conversationCreateRequestDto, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var conversationDocumentToCreate = new ConversationDocument();

      conversationDocumentToCreate.Details = conversationCreateRequestDto.Details;
      conversationDocumentToCreate.StampAllTimestampUtc();

      List<Task> searchParticipants = new List<Task>();

      foreach (var participant in conversationCreateRequestDto.Participants)
      {
        searchParticipants.Add(Task.Run(async () =>
        {
          conversationDocumentToCreate.Participants ??= new List<ParticipantDto>();
          conversationDocumentToCreate.Participants.Add(new ParticipantDto()
          {
            User = _mapper.Map<UserSummaryDto, UserBaseDto>(participant)
          });
        }));
      }

      await Task.WhenAll(searchParticipants);

      var conversationCreateDocumentDto = await _conversationsTableManager.CreateItemAsync<ConversationDocument>(conversationDocumentToCreate, conversationDocumentToCreate.EntityId, cancellationToken);
      var result = _mapper.Map<CreateDocumentDto<ConversationDocument>, OperationStatusWithObject<ConversationDto>>(conversationCreateDocumentDto);
      if (result.IsOperationSuccessful)
      {
        result.Result = _mapper.Map<ConversationDocument, ConversationDto>(conversationDocumentToCreate);
      }

      return result;
    }

    public async Task<OperationStatus> DeleteConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = default(OperationStatus);

      var expression = PredicateBuilder.New((Expression<Func<ConversationDocument, bool>>)(document => document.id == conversationId.ToString()));
      var conversationGetResponse = await _fixitFunctionBuilderFacade.DatabaseRequest
                                                 .Get
                                                 .GetAsync<ConversationDocument>(expression, cancellationToken, null, (document) => document != null)
                                                 .ResolveContextAsync();

      if (conversationGetResponse.Result != null && conversationGetResponse.Result.FetchedResult != null)
      {
        var conversationToDelete = conversationGetResponse.Result.FetchedResult;
        conversationToDelete.UpdateSoftDeletableProperties(isDeleted: true);

        result = await _conversationsTableManager.UpsertItemAsync(conversationToDelete, conversationToDelete.EntityId, cancellationToken);
      }

      return result;
    }

    public async Task<OperationStatusWithObject<ConversationDto>> UpdateConversationParticipantsReadStatusByBulkIdAsync(Guid conversationId, IEnumerable<ParticipantReadStatusUpdateRequestDto> participantReadStatusUpdateRequestDtos, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var result = default(OperationStatusWithObject<ConversationDto>);

      var expression = PredicateBuilder.New((Expression<Func<ConversationDocument, bool>>)(document => document.id == conversationId.ToString()));
      var conversationGetResponse = await _fixitFunctionBuilderFacade.DatabaseRequest
                                                 .Get
                                                 .GetAsync<ConversationDocument>(expression, cancellationToken, null, (document) => document != null)
                                                 .ResolveContextAsync();

      if (conversationGetResponse.Result != null && conversationGetResponse.Result.FetchedResult != null && conversationGetResponse.Result.FetchedResult.Participants is { })
      {
        var conversationParticipants = new ConcurrentBag<ParticipantDto>(conversationGetResponse.Result.FetchedResult.Participants);
        Parallel.ForEach<ParticipantReadStatusUpdateRequestDto>(participantReadStatusUpdateRequestDtos, (updateRequest) =>
        {
          var participantToUpdate = conversationParticipants.FirstOrDefault(x => x.User.Id == updateRequest.ParticipantId);
          if (participantToUpdate is { })
          {
            participantToUpdate.HasUnreadMessages = updateRequest.HasUnreadMessages;
          }
        });

        var conversationToUpdate = conversationGetResponse.Result.FetchedResult;
        conversationToUpdate.Participants = conversationParticipants.ToList();

        var updateResponse = await _conversationsTableManager.UpsertItemAsync(conversationToUpdate, conversationToUpdate.EntityId, cancellationToken);
        result = _mapper.Map<OperationStatus, OperationStatusWithObject<ConversationDto>>(updateResponse);

        if (result.IsOperationSuccessful)
        {
          result.Result = _mapper.Map<ConversationDocument, ConversationDto>(conversationToUpdate);
        }
      }
      return result;
    }

    private Expression<Func<ConversationDocument, bool>> GetConversationsSearchQuery(string searchString, ConversationQueryDto conversationsQueryDto)
    {
      var conversationsQueryDtoVerified = conversationsQueryDto ?? new ConversationQueryDto();
      var queryBuilderFactory = new CosmosQueryBuilderFactory<ConversationDocument>();
      var query = queryBuilderFactory.AddBaseQueryBuilder<ConversationQueryBuilder>(searchString)
                                     .AddSoftDeletableQueryBuilder<ConversationDocument>()
                                     .GetQuery(conversationsQueryDtoVerified);

      Expression<Func<ConversationDocument, bool>> expression = (conversationDocument) => true;
      return query ?? expression;
    }
  }
}
