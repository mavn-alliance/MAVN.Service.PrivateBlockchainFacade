namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface INonceCounter
    {
        string MasterWalletAddress { get; set; }
        
        long CounterValue { get; set; }
    }
}
