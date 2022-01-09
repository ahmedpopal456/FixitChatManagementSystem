using Fixit.Core.DataContracts.QueryBuilders;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  // TODO: Add Summaries
  public interface ITimeTraceableQueryBuilder<BuilderClass, Source> : IQueryBuilder<BuilderClass, Source>
    where BuilderClass : class
    where Source : class
  {
    public BuilderClass BuildCreatedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc);
      
    public BuilderClass BuildCreatedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc);

    public BuilderClass BuildUpdatedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc);

    public BuilderClass BuildUpdatedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc);
  }
}
