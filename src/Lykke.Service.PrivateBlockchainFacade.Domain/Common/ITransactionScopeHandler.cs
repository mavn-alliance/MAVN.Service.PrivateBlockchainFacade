using System;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Common
{
    public interface ITransactionScopeHandler
    {
        Task<TResult> WithTransactionAsync<TResult>(Func<Task<TResult>> func);
        Task WithTransactionAsync(Func<Task> action);
    }
}
