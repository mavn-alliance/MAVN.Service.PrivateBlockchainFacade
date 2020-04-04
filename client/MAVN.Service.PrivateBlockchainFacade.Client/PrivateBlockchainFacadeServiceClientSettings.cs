using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PrivateBlockchainFacade.Client 
{
    /// <summary>
    /// PrivateBlockchainFacade client settings.
    /// </summary>
    public class PrivateBlockchainFacadeServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}

        /// <summary>Api key.</summary>
        [Optional]
        public string ApiKey { get; set; }
    }
}
