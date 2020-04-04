using System.Threading.Tasks;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Fees;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Fees;
using Lykke.Service.QuorumOperationExecutor.Client;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class FeesServiceTests
    {
        private const long ValidFeeValue = 1;

        private readonly Mock<IOperationRequestsProducer> _operationRequestProducerMock = new Mock<IOperationRequestsProducer>();
        private readonly Mock<IQuorumOperationExecutorClient> _executorClientMock = new Mock<IQuorumOperationExecutorClient>();

        [Theory]
        [InlineData(-1)]
        [InlineData(long.MaxValue)]
        public async Task SetTransfersToPublicFeeAsync_InvalidFeeValue_ErrorReturned(long feeValue)
        {
            var sut = CreateSutInstance();

            var result = await sut.SetTransfersToPublicFeeAsync(feeValue);

            Assert.Equal(FeesError.InvalidFee, result);
        }

        [Fact]
        public async Task SetTransfersToPublicFeeAsync_ValidFeeValue_OperationProducerCalled()
        {
            _operationRequestProducerMock.Setup(x => x.AddAsync(OperationType.SetTransferToPublicFee,
                    It.Is<SetTransfersToPublicFeeContext>(c => c.Amount == ValidFeeValue), null))
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.SetTransfersToPublicFeeAsync(ValidFeeValue);

            Assert.Equal(FeesError.None, result);
            _operationRequestProducerMock.Verify();
        }

        public FeesService CreateSutInstance()
        {
            return new FeesService(_operationRequestProducerMock.Object, _executorClientMock.Object);
        }
    }
}
