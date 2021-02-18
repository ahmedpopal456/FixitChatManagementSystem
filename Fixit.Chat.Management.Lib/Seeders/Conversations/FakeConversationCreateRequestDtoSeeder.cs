using System;
using System.Collections.Generic;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Seeders;
using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.Seeders.Conversations
{
  public class FakeConversationCreateRequestDtoSeeder : IFakeSeederAdapter<ConversationCreateRequestDto>
  {
    public IList<ConversationCreateRequestDto> SeedFakeDtos()
    {
      ConversationCreateRequestDto firstConversationCreateRequest = new ConversationCreateRequestDto
      {
        FixInstanceId = new Guid("b7152747-132f-40ed-a4af-5de3ea397ab2"),
        Participants = new List<UserSummaryDto>()
        {
          new UserSummaryDto()
          {
            Id = new Guid("8b418766-4a99-42a8-b6d7-9fe52b88ea93"),
            FirstName = "Mike",
            LastName = "Kunk"
          },
          new UserSummaryDto()
          {
            Id = new Guid("c068fbd8-1c6e-4e78-b51b-2f52048e0518"),
            FirstName = "Mary",
            LastName = "Sue"
          },
        }
      };

      ConversationCreateRequestDto secondConversationCreateRequest = null;

      return new List<ConversationCreateRequestDto>
      {
        firstConversationCreateRequest,
        secondConversationCreateRequest
      };
    }
  }
}
