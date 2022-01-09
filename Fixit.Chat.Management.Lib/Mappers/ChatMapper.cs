using AutoMapper;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Documents;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Messages.TableEntities;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Fixit.Chat.Management.Lib.Mappers.Converters.Conversations;
using Fixit.Chat.Management.Lib.Mappers.Converters.Messages;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Mappers
{
  public class ChatMapper : Profile
  {
    public ChatMapper()
    {
      CreateMap<UserSummaryDto, UserBaseDto>()
        .ReverseMap();

      CreateMap<ConversationMessageUpsertRequestDto, TableConversationMessageEntity>()
        .ConvertUsing<ConversationMessageInsertRequestToTableConversationEntity>();

      CreateMap<TableConversationMessageEntity, ConversationMessageDto>()
        .ConvertUsing<TableConversationMessageEntityToDto>();

      CreateMap<ConversationDocument, ConversationDto>()
        .ConvertUsing<ConversationDocumentToDtoConverter>();

      CreateMap<ConversationMessageUpsertRequestDto, ConversationMessageDto>()
        .ConvertUsing<ConversationMessageInsertRequestToDto>();
    }
  }
}
