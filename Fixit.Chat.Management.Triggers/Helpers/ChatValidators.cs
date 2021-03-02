using System;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;
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
          isValid = !createConversationDeserialized.FixInstanceId.Equals(Guid.Empty) && createConversationDeserialized.Participants != null && createConversationDeserialized.Participants.Count > default(int);

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

    public static bool IsValidUserMessageCreateRequestDto(string queueItem, out UserMessageCreateRequestDto userMessageCreateRequestDto)
    {
      bool isValid = false;
      userMessageCreateRequestDto = null;

      try
      {
        var createUserMessageDeserialized = JsonConvert.DeserializeObject<UserMessageCreateRequestDto>(queueItem);
        if (createUserMessageDeserialized != null)
        {
          isValid = !createUserMessageDeserialized.ConversationId.Equals(Guid.Empty)
                 && !createUserMessageDeserialized.ReceiverUserId.Equals(Guid.Empty)
                 && createUserMessageDeserialized.Message != null
                 && !string.IsNullOrWhiteSpace(createUserMessageDeserialized.Message.Message);

          if (isValid)
          {
            userMessageCreateRequestDto = createUserMessageDeserialized;
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
