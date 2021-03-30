using System;
using System.Runtime.Serialization;
using Fixit.Core.DataContracts.Chat.Enums;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models.Messages
{
  [DataContract]
  public class MessageDto
  {
    [DataMember]
    public Guid Id { get; set; }

    [DataMember]
    public long CreatedTimestampsUtc { get; set; }

    [DataMember]
    public long UpdatedTimestampsUtc { get; set; }

    [DataMember]
    public UserSummaryDto CreatedByUser { get; set; }

    [DataMember]
    public MessageType Type { get; set; }

    [DataMember]
    public string Message { get; set; }
  }
}
