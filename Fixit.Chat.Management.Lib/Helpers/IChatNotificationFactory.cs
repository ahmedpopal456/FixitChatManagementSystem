using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.DataContracts.Notifications.Operations;

namespace Fixit.Chat.Management.Lib.Helpers
{
  public interface IChatNotificationFactory
  {
    /// <summary>
    /// Create new conversation notification to be sent to the client
    /// </summary>
    /// <param name="conversationDocument"></param>
    /// <returns></returns>
    public EnqueueNotificationRequestDto CreateClientConversationNotificationDto(ConversationDocument conversationDocument);

    /// <summary>
    /// Creates new conversation notification to be sent to craftsmen
    /// </summary>
    /// <param name="conversationDocument"></param>
    /// <returns></returns>
    public EnqueueNotificationRequestDto CreateCraftsmanConversationNotificationDto(ConversationDocument conversationDocument);
  }
}
