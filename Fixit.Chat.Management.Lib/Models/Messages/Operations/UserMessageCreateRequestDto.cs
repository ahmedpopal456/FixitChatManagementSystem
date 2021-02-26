using System;
using System.Collections.Generic;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models.Messages.Operations
{
  public class UserMessageCreateRequestDto : IFakeSeederAdapter<UserMessageCreateRequestDto>
  {
    public Guid ConversationId { get; set; }

    public Guid ReceiverUserId { get; set; }

    public MessageDto Message { get; set; }

    #region IFakeSeederAdapter
    public IList<UserMessageCreateRequestDto> SeedFakeDtos()
    {
      UserMessageCreateRequestDto firstMessageCreateRequestDto = new UserMessageCreateRequestDto
      {
        ConversationId = new Guid("3265a8a0-5d73-497c-b3ae-a914259f3800"),
        ReceiverUserId = new Guid("c068fbd8-1c6e-4e78-b51b-2f52048e0518"),
        Message = new MessageDto()
        {
          Id = Guid.NewGuid(),
          CreatedTimestampsUtc = DateTimeOffset.Now.ToUnixTimeSeconds(),
          UpdatedTimestampsUtc = 0,
          CreatedByUser = new UserSummaryDto()
          {
            Id = new Guid("8b418766-4a99-42a8-b6d7-9fe52b88ea93"),
            FirstName = "Mike",
            LastName = "Kunk",
          },
          Type = 0,
          Message = "Hello World!"
        }
      };

      UserMessageCreateRequestDto secondMessageCreateRequestDto = new UserMessageCreateRequestDto
      {
        ConversationId = new Guid("3265a8a0-5d73-497c-b3ae-a914259f3800"),
        ReceiverUserId = new Guid("8b418766-4a99-42a8-b6d7-9fe52b88ea93"),
        Message = new MessageDto()
        {
          Id = Guid.NewGuid(),
          CreatedTimestampsUtc = DateTimeOffset.Now.ToUnixTimeSeconds(),
          UpdatedTimestampsUtc = 0,
          CreatedByUser = new UserSummaryDto()
          {
            Id = new Guid("c068fbd8-1c6e-4e78-b51b-2f52048e0518"),
            FirstName = "Mary",
            LastName = "Sue",
          },
          Type = 0,
          Message = "Hello World Again!"
        }
      };

      return new List<UserMessageCreateRequestDto>
      {
        firstMessageCreateRequestDto,
        secondMessageCreateRequestDto
      };
    }
    #endregion
  }
}
