using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Core.Database;

namespace Fixit.Chat.Management.Lib.Models.Messages
{
  [DataContract]
  public class MessageDocument : DocumentBase
  {
    [DataMember]
    public Guid ConversationId { get; set; }

    [DataMember]
    public int MessageCount { get; set; }

    [DataMember]
    public ICollection<MessageDto> Messages { get; set; }
  }
}
