using Fixit.Chat.Management.Lib.Enums;
using Fixit.Chat.Management.Lib.Facades.Contexts;
using Fixit.Chat.Management.Lib.Facades.Enums;
using Fixit.Chat.Management.Lib.Facades.Options;
using Fixit.Core.Database;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.Database.Mediators;
using Fixit.Core.DataContracts;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fixit.Chat.Management.Lib.Facades.Builders.Internal
{
  internal class FixitDatabaseGetQueryBuilder : IDatabaseGetQueryBuilder
  {
    private bool _disposedValue;

    private FunctionBuilderOptions _functionBuilderOptions;

    readonly IDatabaseTableEntityMediator _documentTable;
    readonly IServiceProvider _serviceProvider;

    public FixitDatabaseGetQueryBuilder(IServiceProvider serviceProvider,
                                          IDatabaseTableEntityMediator documentTable,
                                          FunctionBuilderOptions functionBuilderOptions)
    {
      _functionBuilderOptions = functionBuilderOptions is null ? new FunctionBuilderOptions() : functionBuilderOptions;

      _documentTable = documentTable ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetQueryBuilder)} expects a value for {nameof(_documentTable)}... null argument was provided");
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException($"{nameof(FixitDatabaseGetQueryBuilder)} expects a value for {nameof(serviceProvider)}... null argument was provided");
    }

    public IDatabaseGetQueryBuilder SetThrowOnCancellation(CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return this;
    }

    public IDatabaseGetQueryExecutorBuilder<T> GetAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, Func<T, bool> extraValidation = null) where T : DocumentBase, new()
    {
      Func<Task<OperationStatusWithObject<DatabaseGetRequestContext<T>>>> builtFunction = async () => 
      {
        var executionResult = new OperationStatusWithObject<DatabaseGetRequestContext<T>>();

        var result = default(T);

        var (documentResponse, token) = await _documentTable.GetItemQueryableAsync<T>(null, cancellationToken, expression, queryRequestOptions);
        var document = documentResponse.Results.FirstOrDefault();
        if (documentResponse != null && ( extraValidation == null || extraValidation(document)))
        {
          result = document;
        }

        executionResult.Error = documentResponse.Error;
        executionResult.IsOperationSuccessful = documentResponse.IsOperationSuccessful;
        executionResult.OperationException = documentResponse.OperationException; 
        executionResult.Result = new DatabaseGetRequestContext<T>()
        {
          FetchedResult = result,
        };

        return executionResult;
      };

      return new FixitDatabaseGetQueryExecutorBuilder<T>(_serviceProvider, builtFunction, _functionBuilderOptions);
    }

    public IDatabaseGetMultiQueryExecutorBuilder<T> GetByPageAsync<T>(int currentPage, Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, string orderField = null, OrderDirections orderDirection = OrderDirections.Asc, Func<PagedDocumentCollectionDto<T>, bool> extraValidation = null) where T : DocumentBase
    {
      Func<Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>>> builtFunction = async () =>
      {
        var executionResult = new OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>();

        var result = new PagedDocumentCollectionDto<T>();

        PagedDocumentCollectionDto<T> documentResponse = await _documentTable.GetItemQueryableByPageAsync<T>(currentPage, queryRequestOptions, cancellationToken, expression);
        if (documentResponse != null)
        {
          result.PageNumber = documentResponse.PageNumber;
          result.Results = documentResponse.Results;
          result.IsOperationSuccessful = documentResponse.IsOperationSuccessful;
        }

        executionResult.Error = documentResponse.Error;
        executionResult.IsOperationSuccessful = documentResponse.IsOperationSuccessful;
        executionResult.OperationException = documentResponse.OperationException; 
        executionResult.Result = new DatabaseMultiGetRequestContext<T>()
        {
          FetchedResultsByPage = result,
          FetchTypes = MultiFetchTypes.ByPage
        };

        return executionResult;
      };

      return new FixitDatabaseGetMultiQueryExecutorBuilder<T>(_serviceProvider, builtFunction, _functionBuilderOptions);
    }

    public IDatabaseGetMultiQueryExecutorBuilder<T> GetManyAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, string orderField = null, OrderDirections orderDirection = OrderDirections.Asc, Func<DocumentCollectionDto<T>, bool> extraValidation = null) where T : DocumentBase
    {
      Func<Task<OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>>> builtFunction = async () =>
      {
        var executionResult = new OperationStatusWithObject<DatabaseMultiGetRequestContext<T>>();

        var fetchedResults = new List<T>();

        string currentContinuationToken = "";
        while (currentContinuationToken != null)
        {
          var (pagedDocumentCollection, continuationToken) = await _documentTable.GetItemQueryableAsync<T>(string.IsNullOrWhiteSpace(currentContinuationToken) ? null : currentContinuationToken, cancellationToken, expression, queryRequestOptions);

          currentContinuationToken = continuationToken;
          if (pagedDocumentCollection.IsOperationSuccessful && (extraValidation == null || extraValidation(pagedDocumentCollection)))
          {
            fetchedResults.AddRange(pagedDocumentCollection.Results);
          }

          executionResult.Error = pagedDocumentCollection.Error;
          executionResult.IsOperationSuccessful = pagedDocumentCollection.IsOperationSuccessful;
          executionResult.OperationException = pagedDocumentCollection.OperationException;
        }

        executionResult.Result = new DatabaseMultiGetRequestContext<T>()
        {
          FetchedResultsMany = fetchedResults,
          FetchTypes = MultiFetchTypes.Many
        };

        return executionResult;
      };

      return new FixitDatabaseGetMultiQueryExecutorBuilder<T>(_serviceProvider, builtFunction, _functionBuilderOptions);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _functionBuilderOptions = null;
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}