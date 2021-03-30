using System;
using System.Linq;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.DataContracts.Notifications.Enums;
using Fixit.Core.DataContracts.Notifications.Operations;
using Fixit.Core.DataContracts.Notifications.Payloads;
using Fixit.Core.DataContracts.Users.Enums;

namespace Fixit.Chat.Management.Lib.Helpers
{
  public class ChatNotificationFactory : IChatNotificationFactory
  {
    public EnqueueNotificationRequestDto CreateClientConversationNotificationDto(ConversationDocument conversationDocument)
    {
      EnqueueNotificationRequestDto notificationDto = new EnqueueNotificationRequestDto()
      {
        Payload = new ConversationPayloadDto()
        {
          Id = new Guid(conversationDocument.id),
          FixInstanceId = conversationDocument.FixInstanceId
        },
        Action = NotificationTypes.NewConversation,
        Recipients = conversationDocument.Participants.Select(participant => participant.User).Where(user => user.Role == UserRole.Client),
        Retries = 1
      };
      return notificationDto;
    }

    public EnqueueNotificationRequestDto CreateCraftsmanConversationNotificationDto(ConversationDocument conversationDocument)
    {
      EnqueueNotificationRequestDto notificationDto = new EnqueueNotificationRequestDto()
      {
        Payload = new ConversationPayloadDto()
        {
          Id = new Guid(conversationDocument.id),
          FixInstanceId = conversationDocument.FixInstanceId,
          SentByUser = conversationDocument.Participants.Select(participant => participant.User).First(user => user.Role == UserRole.Client)
        },
        Action = NotificationTypes.NewConversation,
        Recipients = conversationDocument.Participants.Select(participant => participant.User).Where(user => user.Role == UserRole.Craftsman),
        Silent = false,
        Retries = 1
      };
      return notificationDto;
    }
  }
}
