using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  // TODO: Add Summaries
  public interface IBaseQueryBuilder<BuilderClass, Source> : IQueryBuilder<BuilderClass, Source>
    where BuilderClass : class
    where Source : class
  {
    public BuilderClass BuildAndQuery(IList<Expression<Func<Source, bool>>> expressions);

    public BuilderClass BuildBaseQuery<FilteredDto>(FilteredDto filteredDto) where FilteredDto : class;

    public BuilderClass BuildOrQuery(IList<Expression<Func<Source, bool>>> expressions);

    public BuilderClass BuildSearchTextQuery(string searchText);
  }
}
