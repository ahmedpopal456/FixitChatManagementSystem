using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fixit.Core.Database;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Models.Messages
{
  [DataContract]
  public class ConversationMessagesDocument : DocumentBase, IFakeSeederAdapter<ConversationMessagesDocument>
  {
    [DataMember]
    public Guid ConversationId { get; set; }

    private ICollection<MessageDto> _messages;

    [DataMember]
    public ICollection<MessageDto> Messages
    {
      get => _messages ??= new List<MessageDto>();
      set => _messages = value;
    }

    #region IFakeSeederAdapter
    IList<ConversationMessagesDocument> IFakeSeederAdapter<ConversationMessagesDocument>.SeedFakeDtos()
    {
      ConversationMessagesDocument firstMessagesDocument = new ConversationMessagesDocument
      {
        ConversationId = new Guid("3265a8a0-5d73-497c-b3ae-a914259f3800"),
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
          }
        }
      };

      ConversationMessagesDocument secondMessagesDocument = new ConversationMessagesDocument
      {
        ConversationId = new Guid("3265a8a0-5d73-497c-b3ae-a914259f3800"),
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
            Message = "1"
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
              LastName = "Sue",
            },
            Type = 0,
            Message = "2"
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
            Message = "3"
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
              LastName = "Sue",
            },
            Type = 0,
            Message = "4"
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
            Message = "5"
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
              LastName = "Sue",
            },
            Type = 0,
            Message = "6"
          },
        }
      };

      return new List<ConversationMessagesDocument>
      {
        firstMessagesDocument,
        secondMessagesDocument
      };
    }
    #endregion
  }
}
