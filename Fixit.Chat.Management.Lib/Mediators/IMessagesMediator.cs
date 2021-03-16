using System;
using System.Threading;
using System.Threading.Tasks;
using Fixit.Chat.Management.Lib.Models.Messages;
using Fixit.Chat.Management.Lib.Models.Messages.Operations;

namespace Fixit.Chat.Management.Lib.Mediators
{
  public interface IMessagesMediator
  {
    #region ServerlessApi
    /// <summary>
    /// Retrieves messages according to given parameters
    /// </summary>
    /// <param name="conversationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<GetMessagesResponseDto> GetMessagesAsync(Guid conversationId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    #endregion

    #region Triggers
    /// <summary>
    /// Handles new messages created
    /// </summary>
    /// <param name="userMessageCreateRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleMessageAsync(UserMessageCreateRequestDto userMessageCreateRequestDto, CancellationToken cancellationToken);
    #endregion
  }
}
