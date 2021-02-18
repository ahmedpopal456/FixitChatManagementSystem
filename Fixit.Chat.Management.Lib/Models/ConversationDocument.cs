using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Core.Database;

namespace Fixit.Chat.Management.Lib.Models
{
  [DataContract]
  public class ConversationDocument : DocumentBase
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
  }
}
