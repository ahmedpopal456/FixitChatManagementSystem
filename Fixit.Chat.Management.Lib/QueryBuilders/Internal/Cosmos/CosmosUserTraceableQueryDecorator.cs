using LinqKit;
using System;
using System.Linq.Expressions;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Auditables;
using Fixit.Core.DataContracts.QueryBuilders;
using Fixit.Core.DataContracts.QueryBuilders.Auditables;

namespace Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos
{
  public class CosmosUserTraceableQueryDecorator<SourceDocument> : IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>
    where SourceDocument : DocumentBase, IUserTraceableEntity
  {
    private readonly IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> _queryBuilder;

    public CosmosUserTraceableQueryDecorator(IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> queryBuilder)
    {
      _queryBuilder = queryBuilder;
    }

    public Expression<Func<SourceDocument, bool>> BuildCreatedByUserQuery(UserQueryDto userShortDto)
    {
      Expression<Func<SourceDocument, bool>> expression = null;
      if (userShortDto != default)
      {
        expression = document => (userShortDto.Id == null || document.CreatedByUser.Id == userShortDto.Id)
                                 && (userShortDto.FirstName == null || document.CreatedByUser.FirstName == userShortDto.FirstName)
                                 && (userShortDto.LastName == null || document.CreatedByUser.LastName == userShortDto.LastName);
      }
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildUpdatedByUserQuery(UserQueryDto userShortDto)
    {
      Expression<Func<SourceDocument, bool>> expression = null;
      if (userShortDto != default)
      {
        expression = document => (userShortDto.Id == null || document.CreatedByUser.Id == userShortDto.Id)
                                 && (userShortDto.FirstName == null || document.CreatedByUser.FirstName == userShortDto.FirstName)
                                 && (userShortDto.LastName == null || document.CreatedByUser.LastName == userShortDto.LastName);
      }
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> Reset()
    {
      return _queryBuilder.Reset();
    }

    public Expression<Func<SourceDocument, bool>> GetQuery<FilteredDto>(FilteredDto filteredDto)
      where FilteredDto : class
    {
      var expression = _queryBuilder.GetQuery(filteredDto);

      IQueryUserTraceable filteredUserTraceableDto = filteredDto as IQueryUserTraceable;
      expression = filteredUserTraceableDto.CreatedByUser == null ? expression : expression.And(BuildCreatedByUserQuery(filteredUserTraceableDto.CreatedByUser));
      expression = filteredUserTraceableDto.UpdatedByUser == null ? expression : expression.And(BuildUpdatedByUserQuery(filteredUserTraceableDto.UpdatedByUser));

      // accept exp is null
      return expression;
    }
  }
}
