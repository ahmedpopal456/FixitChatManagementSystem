using Fixit.Chat.Management.Lib.Schemas.Enums;
using System;
using System.Runtime.Serialization;

namespace Fixit.Chat.Management.Lib.Schemas
{
  [DataContract]
  public class OnConversationDispatchedAction
  {
    [DataMember]
    public OnConversationDispatchedActions Action { get; set; }

    [DataMember]
    public object ActionPayload { get; set; }
  }
}
