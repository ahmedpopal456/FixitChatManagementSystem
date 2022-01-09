using Fixit.Core.DataContracts.Auditables;
using System;

namespace Fixit.Chat.Management.Lib.Extensions
{
  public static class SoftDeletableExtension
  {
    public static ISoftDeletableEntity UpdateSoftDeletableProperties(this ISoftDeletableEntity softDeletableItem, bool isDeleted)
    {
      softDeletableItem.IsDeleted = isDeleted;
      softDeletableItem.DeletedTimestampUtc = DateTimeOffset.Now.ToUnixTimeSeconds();

      return softDeletableItem;
    }
  }
}
