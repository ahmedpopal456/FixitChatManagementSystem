using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqKit;

namespace Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos
{
  public abstract class CosmosBaseQueryBuilder<TSourceDocument> : IQueryBuilder<CosmosBaseQueryBuilder<TSourceDocument>, TSourceDocument>
    where TSourceDocument : class
  {
    protected Expression<Func<TSourceDocument, bool>> _query;
    protected string _searchText;

    public CosmosBaseQueryBuilder(string searchText)
    {
      _searchText = searchText;
    }

    public Expression<Func<TSourceDocument, bool>> BuildAndQuery(IList<Expression<Func<TSourceDocument, bool>>> expressions)
    {
      Expression<Func<TSourceDocument, bool>> query = null;
      foreach (var expression in expressions)
      {
        query = query == null ? expression : query.And(expression);
      }
      return query;
    }

    public Expression<Func<TSourceDocument, bool>> BuildOrQuery(IList<Expression<Func<TSourceDocument, bool>>> expressions)
    {
      Expression<Func<TSourceDocument, bool>> query = null;
      foreach (var expression in expressions)
      {
        query = query == null ? expression : query.Or(expression);
      }
      return query;
    }

    public abstract Expression<Func<TSourceDocument, bool>> GetQuery<FilteredDto>(FilteredDto filteredDto) where FilteredDto : class;

    public Expression<Func<TSourceDocument, bool>> Reset()
    {
      return null;
    }
  }
}
