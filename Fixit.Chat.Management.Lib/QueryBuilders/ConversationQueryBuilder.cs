using System;
using System.Linq;
using System.Linq.Expressions;
using Fixit.Core.DataContracts.Chat.Documents;
using Fixit.Core.DataContracts.Chat.Operations.Queries;
using LinqKit;
using Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  public class ConversationQueryBuilder : CosmosBaseQueryBuilder<ConversationDocument>
  {
    public ConversationQueryBuilder(string searchText) : base(searchText)  { }

    public override Expression<Func<ConversationDocument, bool>> GetQuery<TFilteredDto>(TFilteredDto filteredDto)
    {
      if (filteredDto is ConversationQueryDto conversationQueryDto)
      {
        // Query Ids
        var stringIds = conversationQueryDto.Ids?.Select(id => id.ToString());
        Expression<Func<ConversationDocument, bool>> queryIds = conversation =>
          stringIds.Contains(conversation.id);
        _query = !(conversationQueryDto.Ids != null && conversationQueryDto.Ids.Any()) ? _query :
          _query == null ? queryIds : _query.And(queryIds);

        // Query Client Ids
        var stringClientIds = conversationQueryDto.EntityIds?.Select(id => id.ToString());
        Expression<Func<ConversationDocument, bool>> queryClientIds = conversation =>
          stringClientIds.Contains(conversation.EntityId);
        _query = !(conversationQueryDto.EntityIds != null && conversationQueryDto.EntityIds.Any()) ? _query :
          _query == null ? queryClientIds : _query.And(queryClientIds);

        // Query Context Details Types
        Expression<Func<ConversationDocument, bool>> queryContextDetailsTypes = conversation =>
          conversationQueryDto.ContextDetailsQuery.Types.Contains(conversation.Details.Type);
        _query = !(conversationQueryDto.ContextDetailsQuery != null && conversationQueryDto.ContextDetailsQuery.Types != null && conversationQueryDto.ContextDetailsQuery.Types.Any()) ? _query :
          _query == null ? queryContextDetailsTypes : _query.And(queryContextDetailsTypes);

        // Query Context Details Ids
        Expression<Func<ConversationDocument, bool>> queryContextDetailsIds = conversation =>
          conversationQueryDto.ContextDetailsQuery.Ids.Contains(conversation.Details.Id);
        _query = !(conversationQueryDto.ContextDetailsQuery != null && conversationQueryDto.ContextDetailsQuery.Ids != null && conversationQueryDto.ContextDetailsQuery.Ids.Any()) ? _query :
          _query == null ? queryContextDetailsIds : _query.And(queryContextDetailsIds);

        // Query Participants List User Id
        Expression<Func<ConversationDocument, bool>> queryParticipantsUsersId = conversation =>
          conversation.Participants.Select(participant => conversationQueryDto.ParticipantQuery.User.Id == participant.User.Id).Contains(true);
        _query = !(conversationQueryDto.ParticipantQuery != null && conversationQueryDto.ParticipantQuery.User != null) ? _query :
          _query == null ? queryParticipantsUsersId : _query.And(queryParticipantsUsersId);

        // Query Participants List User First Name
        Expression<Func<ConversationDocument, bool>> queryParticipantsUsersFirstName = conversation =>
          conversation.Participants.Any(participant => conversationQueryDto.ParticipantQuery.User.FirstName == participant.User.FirstName);
        _query = !(conversationQueryDto.ParticipantQuery != null && conversationQueryDto.ParticipantQuery.User != null && !string.IsNullOrWhiteSpace(conversationQueryDto.ParticipantQuery.User.FirstName)) ? _query :
          _query == null ? queryParticipantsUsersFirstName : _query.And(queryParticipantsUsersFirstName);

        // Query Participants List User Last Name
        Expression<Func<ConversationDocument, bool>> queryParticipantsUsersLastName = conversation =>
          conversation.Participants.Any(participant => conversationQueryDto.ParticipantQuery.User.LastName == participant.User.LastName);
        _query = !(conversationQueryDto.ParticipantQuery != null && conversationQueryDto.ParticipantQuery.User != null && !string.IsNullOrWhiteSpace(conversationQueryDto.ParticipantQuery.User.LastName)) ? _query :
          _query == null ? queryParticipantsUsersLastName : _query.And(queryParticipantsUsersLastName);

        // Query Participants List HasUnreadMessages
        Expression<Func<ConversationDocument, bool>> queryParticipantsHasUnreadTypes = conversation =>
          conversation.Participants.Any(participant => conversationQueryDto.ParticipantQuery.HasUnreadMessages == participant.HasUnreadMessages);
        _query = !(conversationQueryDto.ParticipantQuery != null && conversationQueryDto.ParticipantQuery.HasUnreadMessages != null) ? _query :
          _query == null ? queryParticipantsHasUnreadTypes : _query.And(queryParticipantsHasUnreadTypes);

        // Query Search
        var _searchTextLowered = _searchText?.ToLower();
        Expression<Func<ConversationDocument, bool>> queryUserSearch = conversation =>
         conversation.Details.Name.ToLower().Contains(_searchTextLowered) ||
         conversation.LastMessage.Message.ToLower().Contains(_searchTextLowered);

        _query = string.IsNullOrWhiteSpace(_searchText) ? _query :
          _query == null ? queryUserSearch : _query.And(queryUserSearch);
      }

      return _query ??= conversation => true;
    }
  }
}
