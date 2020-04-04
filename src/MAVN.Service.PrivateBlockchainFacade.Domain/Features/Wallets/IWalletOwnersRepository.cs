using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public interface IWalletOwnersRepository
    {
        /// <summary>
        /// Add new wallet owner
        /// </summary>
        /// <param name="ownerId">The owner id</param>
        /// <param name="walletAddress">The wallet address</param>
        /// <returns></returns>
        /// <exception cref="WalletOwnerDuplicateException">Thrown when there was an exception while creating new wallet owner in the database</exception>
        Task<IWalletOwner> AddAsync(string ownerId, string walletAddress);

        /// <summary>
        /// Get wallet owner by owner id
        /// </summary>
        /// <param name="ownerId">The owner id</param>
        /// <returns></returns>
        Task<IWalletOwner> GetByOwnerIdAsync(string ownerId);

        /// <summary>
        /// Get wallet owner by wallet id
        /// </summary>
        /// <param name="walletAddress">The wallet address</param>
        /// <returns></returns>
        Task<IWalletOwner> GetByWalletAddressAsync(string walletAddress);
    }
}
