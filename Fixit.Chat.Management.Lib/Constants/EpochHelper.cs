using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixit.Chat.Management.Lib.Constants
{
  //TODO Move Helpers to an Lib project
  public static class EpochHelper
  {
    public static DateTime ConvertTimestampToDateTime(string timestamp)
    {
      var result = default(DateTimeOffset);
      if (long.TryParse(timestamp, out long parsedTimestamp))
      {
        result = DateTimeOffset.FromUnixTimeSeconds(parsedTimestamp);
      }

      return result.DateTime;
    }

    public static string ConvertDateTimeToTimestamp(DateTime? dateTime)
    {
      string result = string.Empty;

      if (dateTime != null)
      {
        var dateTimeToConvert = new DateTimeOffset(dateTime.Value);
        result = dateTimeToConvert.ToUnixTimeSeconds().ToString();
      }

      return result;
    }

    public static long GetTimestampSecondsUtcNow()
    {
      return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static long GetTimestampMilliSecondsUtcNow()
    {
      return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }


    #region Helpers

    private static bool IsValidBusinessDay(DayOfWeek candidateDay)
    {
      var inValidBusinessDays = new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday };
      return !inValidBusinessDays.Contains(candidateDay);
    }

    #endregion
  }
}
