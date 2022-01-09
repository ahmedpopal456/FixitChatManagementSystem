using Fixit.Core.DataContracts.QueryBuilders;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  // TODO: Add Summaries
  public interface ISoftDeletableQueryBuilder<BuilderClass> where BuilderClass : class
  {
    public BuilderClass BuildDeletedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc);

    public BuilderClass BuildDeletedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc);

    public BuilderClass BuildIsDeletedQuery(bool isDeleted);
  }
}
