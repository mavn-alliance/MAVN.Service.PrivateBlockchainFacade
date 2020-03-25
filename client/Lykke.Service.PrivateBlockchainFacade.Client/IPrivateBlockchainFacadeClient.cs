using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// PrivateBlockchainFacade client interface.
    /// </summary>
    [PublicAPI]
    public interface IPrivateBlockchainFacadeClient
    {
        /// <summary>
        /// Wallets API interface
        /// </summary>
        IWalletsApi WalletsApi { get; }
        
        /// <summary>
        /// Operations API interface
        /// </summary>
        IOperationsApi OperationsApi { get; }

        /// <summary>
        /// Tokens API interface
        /// </summary>
        ITokensApi TokensApi { get; }
        
        /// <summary>
        /// Bonuses API interface
        /// </summary>
        IBonusesApi BonusesApi { get; }
        
        /// <summary>
        /// Customers API interface
        /// </summary>
        ICustomersApi CustomersApi { get; }
        
        /// <summary>
        /// Transfers API interface
        /// </summary>
        ITransfersApi TransfersApi { get; set; }

        /// <summary>
        /// Interface to GenericTransfers API
        /// </summary>
        IGenericTransfersApi GenericTransfersApi { get; set; }

        /// <summary>
        /// Interface to FeesApi API
        /// </summary>
        IFeesApi FeesApi { get; set; }
    }
}
