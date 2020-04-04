using System.Threading.Tasks;
using System.Transactions;
using Lykke.Logs;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Common;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class TransactionScopeHandlerTests
    {
        [Fact]
        public async Task WithTransactionAsync_TransactionAborted_CommitTransactionExceptionRaised()
        {
            var sut = new TransactionScopeHandler(EmptyLogFactory.Instance);

            await Assert.ThrowsAsync<CommitTransactionException>(() =>
                sut.WithTransactionAsync(() => throw new TransactionAbortedException()));
        }
        
        [Fact]
        public async Task WithTransactionAsyncWithResult_TransactionAborted_CommitTransactionExceptionRaised()
        {
            var sut = new TransactionScopeHandler(EmptyLogFactory.Instance);

            await Assert.ThrowsAsync<CommitTransactionException>(() =>
                sut.WithTransactionAsync<object>(() => throw new TransactionAbortedException()));
        }
    }
}
