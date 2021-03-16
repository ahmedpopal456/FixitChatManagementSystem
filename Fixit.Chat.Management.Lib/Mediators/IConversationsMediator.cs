using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.DataContracts.Chat;

namespace Fixit.Chat.Management.Lib.Mediators
{
  public interface IConversationsMediator
  {
    #region ServerlessApi
    /// <summary>
    /// Retrieves all conversations for a specific user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DocumentCollectionDto<ConversationDocument>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken);
    #endregion

    #region Triggers
    /// <summary>
    /// Creates a conversation in the CosmosDB
    /// </summary>
    /// <param name="conversationCreateRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateConversationAsync(ConversationCreateRequestDto conversationCreateRequestDto, CancellationToken cancellationToken);
    #endregion
  }
}
