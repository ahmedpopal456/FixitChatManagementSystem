using System.Linq;
using AutoMapper;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Chat.Documents;

namespace Fixit.Chat.Management.Lib.Mappers.Converters.Conversations
{
  public class ConversationDocumentToDtoConverter : ITypeConverter<ConversationDocument, ConversationDto>
  {
    public ConversationDto Convert(ConversationDocument source, ConversationDto destination, ResolutionContext context)
    {
      destination ??= new ConversationDto();
      if (source != null)
      {
        destination.Id = source.id;
        destination.EntityId = source.EntityId;
        destination.Details = source.Details;
        destination.LastMessage = source.LastMessage;
        destination.Participants = source.Participants;
        destination.CreatedTimestampUtc = source.CreatedTimestampUtc;
        destination.UpdatedTimestampUtc = source.UpdatedTimestampUtc;
        destination.IsDeleted = source.IsDeleted;
        destination.DeletedTimestampUtc = source.DeletedTimestampUtc;
      }

      return destination;
    }
  }
}
