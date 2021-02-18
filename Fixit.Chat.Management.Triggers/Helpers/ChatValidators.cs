using System;
using Fixit.Core.DataContracts.Chat;
using Newtonsoft.Json;

namespace Fixit.Chat.Management.Triggers.Helpers
{
  public static class ChatValidators
  {
    public static bool IsValidConversationCreateRequestDto(string queueItem, out ConversationCreateRequestDto conversationCreateRequestDto)
    {
      bool isValid = false;
      conversationCreateRequestDto = null;

      try
      {
        var createConversationDeserialized = JsonConvert.DeserializeObject<ConversationCreateRequestDto>(queueItem);
        if (createConversationDeserialized != null)
        {
          isValid = !createConversationDeserialized.FixInstanceId.Equals(Guid.Empty) && createConversationDeserialized.Participants.Count > default(int);

          if (isValid)
          {
            conversationCreateRequestDto = createConversationDeserialized;
          }
        }
      }
      catch
      {
        // Fall through
      }

      return isValid;
    }
  }
}
