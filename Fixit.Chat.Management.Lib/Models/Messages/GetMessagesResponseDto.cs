using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models.Messages
{
  [DataContract]
  public class GetMessagesResponseDto : OperationStatus, IFakeSeederAdapter<GetMessagesResponseDto>
  {
    private ICollection<MessageDto> _messages;

    [DataMember]
    public ICollection<MessageDto> Messages
    {
      get => _messages ??= new List<MessageDto>();
      set => _messages = value;
    }

    public IList<GetMessagesResponseDto> SeedFakeDtos()
    {
      GetMessagesResponseDto firstMessagesResponseDto = new GetMessagesResponseDto
      {
        Messages = new List<MessageDto>()
        {
          new MessageDto()
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
          },
          new MessageDto()
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
            Message = "Hello World Again!"
          },
          new MessageDto()
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
            Message = "Hello World Yet Again!"
          },
          new MessageDto()
          {
            Id = Guid.NewGuid(),
            CreatedTimestampsUtc = DateTimeOffset.Now.ToUnixTimeSeconds(),
            UpdatedTimestampsUtc = 0,
            CreatedByUser = new UserSummaryDto()
            {
              Id = new Guid("c068fbd8-1c6e-4e78-b51b-2f52048e0518"),
              FirstName = "Mary",
              LastName = "Sue"
            },
            Type = 0,
            Message = "Hello World!!"
          }
        }
      };

      GetMessagesResponseDto secondMessagesResponseDto = null;

      return new List<GetMessagesResponseDto>
      {
        firstMessagesResponseDto,
        secondMessagesResponseDto
      };
    }
  }
}
