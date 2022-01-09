using AutoMapper;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Operations.Requests;

namespace Fixit.Chat.Management.Lib.Mappers.Converters.Messages
{
  public class ConversationMessageInsertRequestToDto : ITypeConverter<ConversationMessageUpsertRequestDto, ConversationMessageDto>
  {
    public ConversationMessageDto Convert(ConversationMessageUpsertRequestDto source, ConversationMessageDto destination, ResolutionContext context)
    {
      destination ??= new ConversationMessageDto();

      if (source != null)
      {
        destination.Message = source.Message;
        destination.Attachments = source.Attachments;
        destination.UpdatedByUser = destination.CreatedByUser = source.SentByUser;
      }
      return destination;
    }
  }
}