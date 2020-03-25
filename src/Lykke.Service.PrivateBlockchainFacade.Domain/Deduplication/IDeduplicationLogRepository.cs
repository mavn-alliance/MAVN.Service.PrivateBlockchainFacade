using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication
{
    public interface IDeduplicationLogRepository<T> where T : IDeduplicatable
    {
        Task<bool> IsDuplicateAsync(string key);
    }
}
