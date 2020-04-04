using System;
using System.Threading.Tasks;
using System.Transactions;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Common
{
    public class TransactionScopeHandler : ITransactionScopeHandler
    {
        private readonly ILog _log;

        public TransactionScopeHandler(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }
        public async Task<T> WithTransactionAsync<T>(Func<Task<T>> func)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await func();

                    scope.Complete();

                    return result;
                }
            }
            catch (TransactionAbortedException e)
            {
                _log.Error(e, "Error occured while commiting transaction");

                throw new CommitTransactionException();
            }
        }
        
        public async Task WithTransactionAsync(Func<Task> action)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await action();

                    scope.Complete();
                }
            }
            catch (TransactionAbortedException e)
            {
                _log.Error(e, "Error occured while commiting transaction");

                throw new CommitTransactionException();
            }
        }
    }
}
