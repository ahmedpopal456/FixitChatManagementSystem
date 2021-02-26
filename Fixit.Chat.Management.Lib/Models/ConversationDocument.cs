using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Core.Database;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models
{
  [DataContract]
  public class ConversationDocument : DocumentBase, IFakeSeederAdapter<ConversationDocument>
  {
    [DataMember]
    public Guid FixInstanceId { get; set; }

    [DataMember]
    public ICollection<ParticipantDto> Participants { get; set; }

    [DataMember]
    public MessageDto LastMessage { get; set; }

    [DataMember]
    public long CreatedTimestampsUtc { get; set; }

    [DataMember]
    public long UpdatedTimestampsUtc { get; set; }

    #region IFakeSeederAdapter
    IList<ConversationDocument> IFakeSeederAdapter<ConversationDocument>.SeedFakeDtos()
    {
      ConversationDocument firstConversationDocument = new ConversationDocument
      {
        FixInstanceId = new Guid("b7152747-132f-40ed-a4af-5de3ea397ab2"),
        Participants = new List<ParticipantDto>()
        {
          new ParticipantDto()
          {
            User = new UserSummaryDto()
            {
              Id = new Guid("8b418766-4a99-42a8-b6d7-9fe52b88ea93"),
              FirstName = "Mike",
              LastName = "Kunk"
            }
          },
          new ParticipantDto()
          {
            User = new UserSummaryDto()
            {
              Id = new Guid("c068fbd8-1c6e-4e78-b51b-2f52048e0518"),
              FirstName = "Mary",
              LastName = "Sue"
            }
          }
        }
      };

      ConversationDocument secondConversationDocument = null;

      return new List<ConversationDocument>
      {
        firstConversationDocument,
        secondConversationDocument
      };
    }
    #endregion
  }
}
