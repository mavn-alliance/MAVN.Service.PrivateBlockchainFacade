using Lykke.HttpClientGenerator;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// PrivateBlockchainFacade API aggregating interface.
    /// </summary>
    public class PrivateBlockchainFacadeClient : IPrivateBlockchainFacadeClient
    {
        /// <summary>Interface to Wallets Api.</summary>
        public IWalletsApi WalletsApi { get; private set; }

        /// <summary>Interface to Operations Api.</summary>
        public IOperationsApi OperationsApi { get; }
        
        /// <summary>
        /// Interface to Bonuses Api
        /// </summary>
        public IBonusesApi BonusesApi { get; }

        /// <summary>Interface to Tokens Api.</summary>
        public ITokensApi TokensApi { get; }
        
        /// <summary>
        /// Interface to Customers API
        /// </summary>
        public ICustomersApi CustomersApi { get; }

        /// <summary>
        /// Interface to Transfers API
        /// </summary>
        public ITransfersApi TransfersApi { get; set; }

        /// <summary>
        /// Interface to GenericTransfers API
        /// </summary>
        public IGenericTransfersApi GenericTransfersApi { get; set; }

        /// <summary>
        /// Interface to FeesApi API
        /// </summary>
        public IFeesApi FeesApi { get; set; }

        /// <summary>C-tor</summary>
        public PrivateBlockchainFacadeClient(IHttpClientGenerator httpClientGenerator)
        {
            WalletsApi = httpClientGenerator.Generate<IWalletsApi>();
            OperationsApi = httpClientGenerator.Generate<IOperationsApi>();
            TokensApi = httpClientGenerator.Generate<ITokensApi>();
            BonusesApi = httpClientGenerator.Generate<IBonusesApi>();
            CustomersApi = httpClientGenerator.Generate<ICustomersApi>();
            TransfersApi = httpClientGenerator.Generate<ITransfersApi>();
            GenericTransfersApi = httpClientGenerator.Generate<IGenericTransfersApi>();
            FeesApi = httpClientGenerator.Generate<IFeesApi>();
        }
    }
}
