using System.Runtime.Serialization;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models
{
  [DataContract]
  public class ParticipantDto
  {
    [DataMember]
    public UserSummaryDto User { get; set; }

    [DataMember]
    public long CreatedTimestampsUtc { get; set; }

    [DataMember]
    public long UpdatedTimestampsUtc { get; set; }

    [DataMember]
    public int UnreadCount { get; set; }

    [DataMember]
    public long LastRead { get; set; }
  }
}
