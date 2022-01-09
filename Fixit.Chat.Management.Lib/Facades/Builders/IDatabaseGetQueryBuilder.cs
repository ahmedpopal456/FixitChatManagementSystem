using Fixit.Chat.Management.Lib.Enums;
using Fixit.Core.Database;
using Fixit.Core.Database.DataContracts.Documents;
using Fixit.Core.DataContracts;
using Microsoft.Azure.Cosmos;
using System;
using System.Linq.Expressions;
using System.Threading;

namespace Fixit.Chat.Management.Lib.Facades.Builders
{
  // TODO: Add Summaries
  public interface IDatabaseGetQueryBuilder : IDisposable
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IDatabaseGetQueryBuilder SetThrowOnCancellation(CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="currentPage"></param>
    /// <param name="pageSize"></param>
    /// <param name="expression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="queryRequestOptions"></param>
    /// <param name="extraValidation"></param>
    /// <returns></returns>
    IDatabaseGetMultiQueryExecutorBuilder<T> GetByPageAsync<T>(int currentPage, Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, string orderField = null, OrderDirections orderDirection = OrderDirections.Asc, Func<PagedDocumentCollectionDto<T>, bool> extraValidation = null) where T : DocumentBase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="queryRequestOptions"></param>
    /// <param name="extraValidation"></param>
    /// <returns></returns>
    IDatabaseGetMultiQueryExecutorBuilder<T> GetManyAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, string orderField = null, OrderDirections orderDirection = OrderDirections.Asc, Func<DocumentCollectionDto<T>, bool> extraValidation = null) where T : DocumentBase;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="queryRequestOptions"></param>
    /// <param name="extraValidation"></param>
    /// <returns></returns>
    IDatabaseGetQueryExecutorBuilder<T> GetAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken, QueryRequestOptions queryRequestOptions = null, Func<T, bool> extraValidation = null) where T : DocumentBase, new();
  }
}