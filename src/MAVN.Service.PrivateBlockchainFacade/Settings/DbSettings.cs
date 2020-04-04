using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        
        public string DataConnString { get; set; }
        
        [Optional]
        public int? CommandTimeoutSeconds { get; set; }
    }
}
