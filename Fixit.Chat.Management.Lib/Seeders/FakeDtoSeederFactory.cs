using Fixit.Chat.Management.Lib.Seeders.Conversations;
using Fixit.Chat.Management.Lib.Models;
using Fixit.Core.DataContracts.Chat;
using Fixit.Core.DataContracts.Seeders;

namespace Fixit.Chat.Management.Lib.Seeders
{
  public class FakeDtoSeederFactory : IFakeSeederFactory
  {
    public IFakeSeederAdapter<T> CreateFakeSeeder<T>() where T : class
    {
      string type = typeof(T).Name;

      switch (type)
      {
        case nameof(ConversationDocument):
          return (IFakeSeederAdapter<T>)new FakeConversationDocumentSeeder();
        case nameof(ConversationCreateRequestDto):
          return (IFakeSeederAdapter<T>)new FakeConversationCreateRequestDtoSeeder();
        default:
          return null;
      }
    }
  }
}
