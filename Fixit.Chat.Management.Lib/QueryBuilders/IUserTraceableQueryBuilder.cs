using Fixit.Core.DataContracts.Users;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  // TODO: Add Summaries
  public interface IUserTraceableQueryBuilder<BuilderClass> where BuilderClass : class
  {
    public BuilderClass BuildCreatedByUserQuery(UserSummaryDto userShortDto);

    public BuilderClass BuildUpdatedByUserQuery(UserSummaryDto userShortDto);
  }
}
