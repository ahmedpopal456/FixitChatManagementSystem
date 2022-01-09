using Fixit.Core.Database;
using Fixit.Core.DataContracts;
using Fixit.Core.DataContracts.Auditables;
using System;
using System.Linq.Expressions;

namespace Fixit.Chat.Management.Lib.QueryBuilders.Internal.Cosmos
{
  public class CosmosQueryBuilderFactory<SourceDocument> : IQueryBuilder<CosmosQueryBuilderFactory<SourceDocument>, SourceDocument>
    where SourceDocument : DocumentBase
  {
    private IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument> _cosmosBaseQueryBuilder;

    public CosmosQueryBuilderFactory<SourceDocument> AddBaseQueryBuilder<ACosmosBaseQueryBuilder>()
      where ACosmosBaseQueryBuilder : CosmosBaseQueryBuilder<SourceDocument>
    {
      /* Temporary Solution */
      _cosmosBaseQueryBuilder = Activator.CreateInstance<ACosmosBaseQueryBuilder>();
      return this;
    }

    public CosmosQueryBuilderFactory<SourceDocument> AddBaseQueryBuilder<TCosmosBaseQueryBuilder>(string searchText)
      where TCosmosBaseQueryBuilder : CosmosBaseQueryBuilder<SourceDocument>
    {
      /* Temporary Solution */
      _cosmosBaseQueryBuilder = (IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>)Activator.CreateInstance(typeof(TCosmosBaseQueryBuilder), searchText);
      return this;
    }

    public CosmosQueryBuilderFactory<SourceDocument> AddSubQueryBuilder<TCosmosBaseQueryBuilder>(string searchText)
      where TCosmosBaseQueryBuilder : IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>
    {
      _cosmosBaseQueryBuilder = (IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>)Activator.CreateInstance(typeof(TCosmosBaseQueryBuilder), new object[] { _cosmosBaseQueryBuilder, searchText });
      return this;
    }

    public CosmosQueryBuilderFactory<SourceDocument> AddTimeTraceableQueryBuilder<SourceDocumentT>()
      where SourceDocumentT : DocumentBase, ITimeTraceableEntity
    {
      _cosmosBaseQueryBuilder = new CosmosTimeTraceableQueryDecorator<SourceDocumentT>((IQueryBuilder<CosmosBaseQueryBuilder<SourceDocumentT>, SourceDocumentT>)_cosmosBaseQueryBuilder)
                                    as IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>;
      return this;
    }

    public CosmosQueryBuilderFactory<SourceDocument> AddUserTraceableQueryBuilder<SourceDocumentU>()
      where SourceDocumentU : DocumentBase, IUserTraceableEntity
    {
      _cosmosBaseQueryBuilder = new CosmosUserTraceableQueryDecorator<SourceDocumentU>((IQueryBuilder<CosmosBaseQueryBuilder<SourceDocumentU>, SourceDocumentU>)_cosmosBaseQueryBuilder)
                                  as IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>;
      return this;
    }

    public CosmosQueryBuilderFactory<SourceDocument> AddSoftDeletableQueryBuilder<SourceDocumentS>()
      where SourceDocumentS : DocumentBase, ISoftDeletableEntity
    {
      _cosmosBaseQueryBuilder = new CosmosSoftDeletableQueryDecorator<SourceDocumentS>((IQueryBuilder<CosmosBaseQueryBuilder<SourceDocumentS>, SourceDocumentS>)_cosmosBaseQueryBuilder)
                                  as IQueryBuilder<CosmosBaseQueryBuilder<SourceDocument>, SourceDocument>;
      return this;
    }

    public Expression<Func<SourceDocument, bool>> GetQuery<FilteredDto>(FilteredDto filteredDto)
      where FilteredDto : class
    {
      return _cosmosBaseQueryBuilder.GetQuery(filteredDto);
    }

    public Expression<Func<SourceDocument, bool>> Reset()
    {
      return _cosmosBaseQueryBuilder.Reset();
    }
  }
}
