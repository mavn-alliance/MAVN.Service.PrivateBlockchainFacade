namespace MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication
{
    public interface IDeduplicatable
    {
        string DeduplicationKey { get; set; }
    }
}
