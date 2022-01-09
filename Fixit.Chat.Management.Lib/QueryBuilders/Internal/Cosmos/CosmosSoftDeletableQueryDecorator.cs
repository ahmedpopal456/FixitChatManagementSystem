using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Auditables;
using Fixit.Core.DataContracts.QueryBuilders;
using Fixit.Core.DataContracts.QueryBuilders.Auditables;
using LinqKit;
using System;
using System.Linq.Expressions;

namespace Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos
{
  public class CosmosSoftDeletableQueryDecorator<SourceDocument> : IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>
    where SourceDocument : DocumentBase, ISoftDeletableEntity
  {
    private readonly IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> _queryBuilder;

    public CosmosSoftDeletableQueryDecorator(IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> queryBuilder)
    {
      _queryBuilder = queryBuilder;
    }

    public Expression<Func<SourceDocument, bool>> BuildDeletedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = queryOperator switch
      {
        QueryBuilderOperators.Equal => document => document.DeletedTimestampUtc == timestampUtc,
        QueryBuilderOperators.GreaterThan => document => document.DeletedTimestampUtc > timestampUtc,
        QueryBuilderOperators.GreaterThanEqual => document => document.DeletedTimestampUtc >= timestampUtc,
        QueryBuilderOperators.LessThan => document => document.DeletedTimestampUtc < timestampUtc,
        QueryBuilderOperators.LessThanEqual => d1ocument => d1ocument.DeletedTimestampUtc <= timestampUtc,
        _ => document => document.DeletedTimestampUtc == timestampUtc,
      };
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildDeletedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = document => minTimestampUtc <= document.DeletedTimestampUtc
                                                                      && document.DeletedTimestampUtc <= maxTimestampUtc;

      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildDeletedTimestampUtcQuery(TimestampsQueryDto timestampsQueryDto)
    {
      Expression<Func<SourceDocument, bool>> expression = null;
      if (timestampsQueryDto != null)
      {
        if (timestampsQueryDto.MaxTimestampUtc != null)
        {
          expression = BuildDeletedTimestampUtcQuery(timestampsQueryDto.MinTimestampUtc, timestampsQueryDto.MaxTimestampUtc.Value);
        }
        else
        {
          expression = BuildDeletedTimestampUtcQuery(timestampsQueryDto.QueryBuilderOperator.Value, timestampsQueryDto.MinTimestampUtc);
        }
      }
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildIsDeletedQuery(bool? isDeleted)
    {
      Expression<Func<SourceDocument, bool>> expression = document => document.IsDeleted == isDeleted.Value;
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

      IQuerySoftDeletable filteredSoftDeletableDto = filteredDto as IQuerySoftDeletable;
      expression = filteredSoftDeletableDto.DeletedTimestampUtcQuery == null ? expression : expression.And(BuildDeletedTimestampUtcQuery(filteredSoftDeletableDto.DeletedTimestampUtcQuery));
      if (filteredSoftDeletableDto.IsDeleted.HasValue)
      {
        expression = expression == null ? BuildIsDeletedQuery(filteredSoftDeletableDto.IsDeleted) : expression.And(BuildIsDeletedQuery(filteredSoftDeletableDto.IsDeleted));
      }

      return expression;
    }
  }
}
