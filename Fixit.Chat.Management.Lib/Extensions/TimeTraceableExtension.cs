
using Fixit.Chat.Management.Lib.Constants;
using Fixit.Core.DataContracts.Auditables;

namespace Fixit.Chat.Management.Lib.Extensions
{
  public static class TimeTraceableExtension
  {
    public static ITimeTraceableEntity StampAllTimestampUtc(this ITimeTraceableEntity timeTraceableItem)
    {
      long currentTime = EpochHelper.GetTimestampSecondsUtcNow();
      timeTraceableItem.CreatedTimestampUtc = currentTime;
      timeTraceableItem.UpdatedTimestampUtc = currentTime;

      return timeTraceableItem;
    }

    public static ITimeTraceableEntity StampUpdatedTimestampUtc(this ITimeTraceableEntity timeTraceableItem)
    {
      long currentTime = EpochHelper.GetTimestampSecondsUtcNow();
      timeTraceableItem.UpdatedTimestampUtc = currentTime;

      return timeTraceableItem;
    }
  }
}
