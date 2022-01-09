using System;
using System.Linq.Expressions;

namespace Fixit.Chat.Management.Lib.QueryBuilders
{
  // TODO: Add Summaries
  public interface IQueryBuilder<TBuilderClass, TSource>
    where TBuilderClass : class
    where TSource : class
  {
    public Expression<Func<TSource, bool>> GetQuery<TFilteredDto>(TFilteredDto filteredDto) where TFilteredDto : class;

    public Expression<Func<TSource, bool>> Reset();
  }
}
