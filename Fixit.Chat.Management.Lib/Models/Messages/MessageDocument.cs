using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Core.Database;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models.Messages
{
  [DataContract]
  public class MessageDocument : DocumentBase, IFakeSeederAdapter<MessageDocument>
  {
    [DataMember]
    public Guid ConversationId { get; set; }

    [DataMember]
    public ICollection<MessageDto> Messages { get; set; }

    #region IFakeSeederAdapter
    IList<MessageDocument> IFakeSeederAdapter<MessageDocument>.SeedFakeDtos()
    {
      MessageDocument firstMessageDocument = new MessageDocument
      {
        ConversationId = new Guid("3265a8a0-5d73-497c-b3ae-a914259f3800"),
        Messages = new List<MessageDto>()
        {
          new MessageDto()
          {
            Id = Guid.NewGuid(),
            CreatedTimestampsUtc = DateTimeOffset.Now.ToUnixTimeSeconds(),
            UpdatedTimestampsUtc = 0,
            CreatedByUser = new UserSummaryDto()
            {
              Id = new Guid("8b418766-4a99-42a8-b6d7-9fe52b88ea93"),
              FirstName = "Mike",
              LastName = "Kunk",
            },
            Type = 0,
            Message = "Hello World!"
          }
        }
      };

      MessageDocument secondMessageDocument = null;

      return new List<MessageDocument>
      {
        firstMessageDocument,
        secondMessageDocument
      };
    }
    #endregion
  }
}
