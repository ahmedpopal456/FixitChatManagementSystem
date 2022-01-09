using System.Linq;
using AutoMapper;
using Fixit.Core.DataContracts.Chat.Messages.TableEntities;
using Fixit.Core.DataContracts.Chat.Operations.Requests;
using Newtonsoft.Json;

namespace Fixit.Chat.Management.Lib.Mappers.Converters.Messages
{
  public class ConversationMessageInsertRequestToTableConversationEntity : ITypeConverter<ConversationMessageUpsertRequestDto, TableConversationMessageEntity>
  {
    public TableConversationMessageEntity Convert(ConversationMessageUpsertRequestDto source, TableConversationMessageEntity destination, ResolutionContext context)
    {
      destination ??= new TableConversationMessageEntity();

      if (source != null)
      {
        destination.Message = source.Message;
        destination.PartitionKey = destination.RowKey;
        destination.Attachments = source.Attachments != null && source.Attachments.Any() ? JsonConvert.SerializeObject(source.Attachments) : default;
        destination.UpdatedByUser = destination.CreatedByUser = source.SentByUser != null ? JsonConvert.SerializeObject(source.SentByUser) : string.Empty;
      }
      return destination;
    }
  }
}