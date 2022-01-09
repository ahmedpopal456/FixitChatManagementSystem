using Fixit.Core.Database;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Auditables;
using Fixit.Core.DataContracts.QueryBuilders;
using Fixit.Core.DataContracts.QueryBuilders.Auditables;
using LinqKit;
using System;
using System.Linq.Expressions;

namespace Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos
{
  public class CosmosTimeTraceableQueryDecorator<SourceDocument> : IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>
    where SourceDocument : DocumentBase, ITimeTraceableEntity
  {
    private readonly IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> _queryBuilder;

    public CosmosTimeTraceableQueryDecorator(IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> queryBuilder)
    {
      _queryBuilder = queryBuilder;
    }

    public Expression<Func<SourceDocument, bool>> BuildCreatedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = queryOperator switch
      {
        QueryBuilderOperators.Equal => document => document.CreatedTimestampUtc == timestampUtc,
        QueryBuilderOperators.GreaterThan => document => document.CreatedTimestampUtc > timestampUtc,
        QueryBuilderOperators.GreaterThanEqual => document => document.CreatedTimestampUtc >= timestampUtc,
        QueryBuilderOperators.LessThan => document => document.CreatedTimestampUtc < timestampUtc,
        QueryBuilderOperators.LessThanEqual => d1ocument => d1ocument.CreatedTimestampUtc <= timestampUtc,
        _ => document => document.CreatedTimestampUtc == timestampUtc,
      };
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildCreatedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = document => minTimestampUtc <= document.CreatedTimestampUtc
                                                                      && document.CreatedTimestampUtc <= maxTimestampUtc;

      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildCreatedTimestampUtcQuery(TimestampsQueryDto timestampsQueryDto)
    {
      Expression<Func<SourceDocument, bool>> expression = null;
      if (timestampsQueryDto != null)
      {
        if (timestampsQueryDto.MaxTimestampUtc != null)
        {
          expression = BuildCreatedTimestampUtcQuery(timestampsQueryDto.MinTimestampUtc, timestampsQueryDto.MaxTimestampUtc.Value);
        }
        else
        {
          expression = BuildCreatedTimestampUtcQuery(timestampsQueryDto.QueryBuilderOperator.Value, timestampsQueryDto.MinTimestampUtc);
        }
      }
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildUpdatedTimestampUtcQuery(QueryBuilderOperators queryOperator, long timestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = queryOperator switch
      {
        QueryBuilderOperators.Equal => document => document.UpdatedTimestampUtc == timestampUtc,
        QueryBuilderOperators.GreaterThan => document => document.UpdatedTimestampUtc > timestampUtc,
        QueryBuilderOperators.GreaterThanEqual => document => document.UpdatedTimestampUtc >= timestampUtc,
        QueryBuilderOperators.LessThan => document => document.UpdatedTimestampUtc < timestampUtc,
        QueryBuilderOperators.LessThanEqual => document => document.UpdatedTimestampUtc <= timestampUtc,
        _ => document => document.UpdatedTimestampUtc == timestampUtc,
      };
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildUpdatedTimestampUtcQuery(long minTimestampUtc, long maxTimestampUtc)
    {
      Expression<Func<SourceDocument, bool>> expression = document => minTimestampUtc <= document.UpdatedTimestampUtc
                                                                            && document.UpdatedTimestampUtc <= maxTimestampUtc;

      return expression;
    }

    public Expression<Func<SourceDocument, bool>> BuildUpdatedTimestampUtcQuery(TimestampsQueryDto timestampsQueryDto)
    {
      Expression<Func<SourceDocument, bool>> expression = null;
      if (timestampsQueryDto != null)
      {
        if (timestampsQueryDto.MaxTimestampUtc != null)
        {
          expression = BuildUpdatedTimestampUtcQuery(timestampsQueryDto.MinTimestampUtc, timestampsQueryDto.MaxTimestampUtc.Value);
        }
        else
        {
          expression = BuildUpdatedTimestampUtcQuery(timestampsQueryDto.QueryBuilderOperator.Value, timestampsQueryDto.MinTimestampUtc);
        }
      }
      return expression;
    }

    public Expression<Func<SourceDocument, bool>> GetQuery<FilteredDto>(FilteredDto filteredDto)
      where FilteredDto : class
    {
      var expression = _queryBuilder?.GetQuery(filteredDto);

      IQueryTimeTraceable filteredTimeTraceableDto = filteredDto as IQueryTimeTraceable;
      expression = filteredTimeTraceableDto.CreatedTimestampUtcQuery == null ? expression : expression.And(BuildCreatedTimestampUtcQuery(filteredTimeTraceableDto.CreatedTimestampUtcQuery));
      expression = filteredTimeTraceableDto.UpdatedTimestampUtcQuery == null ? expression : expression.And(BuildUpdatedTimestampUtcQuery(filteredTimeTraceableDto.UpdatedTimestampUtcQuery));

      return expression;
    }

    public Expression<Func<SourceDocument, bool>> Reset()
    {
      return _queryBuilder.Reset();
    }
  }
}
