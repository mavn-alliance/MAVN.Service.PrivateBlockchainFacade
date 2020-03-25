using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Operations;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class OperationsProducerTests
    {
        private readonly Mock<IOperationRequestsRepository> _operationsRequestsRepositoryMock = new Mock<IOperationRequestsRepository>();

        private const string FakeMasterWalletAddress = "c8cd7daa-864e-4e71-bded-15e92e852264";
        
        private const string FakeCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";

        [Theory]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData(FakeCustomerId, null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public async Task AddAsync_InvalidInputParameters_RaisesException(string customerId, object context)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.AddAsync(customerId, default(OperationType), context));

        }
        private IOperationRequestsProducer CreateSutInstance()
        {
            return new OperationRequestsProducer(
                FakeMasterWalletAddress,
                _operationsRequestsRepositoryMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
