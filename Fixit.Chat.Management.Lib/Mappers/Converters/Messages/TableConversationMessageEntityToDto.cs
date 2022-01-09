using System.Collections.Generic;
using AutoMapper;
using Fixit.Core.DataContracts.Chat.Attachments;
using Fixit.Core.DataContracts.Chat.Messages;
using Fixit.Core.DataContracts.Chat.Messages.TableEntities;
using Fixit.Core.DataContracts.Users;
using Newtonsoft.Json;

namespace Fixit.Chat.Management.Lib.Mappers.Converters.Messages
{
  public class TableConversationMessageEntityToDto : ITypeConverter<TableConversationMessageEntity, ConversationMessageDto>
  {
    public ConversationMessageDto Convert(TableConversationMessageEntity source, ConversationMessageDto destination, ResolutionContext context)
    {
      destination ??= new ConversationMessageDto();

      if (source != null)
      {
        destination.Message = source.Message;
        destination.CreatedTimestampUtc = source.CreatedTimestampUtc;
        destination.UpdatedTimestampUtc = source.UpdatedTimestampUtc;
        destination.Id = long.TryParse(source.RowKey, out long result) ? long.Parse(source.RowKey) : destination.Id;
        destination.CreatedByUser = !string.IsNullOrWhiteSpace(source.CreatedByUser) ? JsonConvert.DeserializeObject<UserBaseDto>(source.CreatedByUser) : default(UserBaseDto);
        destination.UpdatedByUser = !string.IsNullOrWhiteSpace(source.UpdatedByUser) ? JsonConvert.DeserializeObject<UserBaseDto>(source.UpdatedByUser) : default(UserBaseDto);
        destination.Attachments = !string.IsNullOrWhiteSpace(source.Attachments) ? JsonConvert.DeserializeObject<IEnumerable<MessageAttachmentDto>>(source.Attachments) : default(IEnumerable<MessageAttachmentDto>);
      }
      return destination;
    }
  }
}