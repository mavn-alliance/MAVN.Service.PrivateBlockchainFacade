
namespace MAVN.Service.PrivateBlockchainFacade.Auth
{
    /// <summary>
    /// Validator for auth api key
    /// </summary>
    public class ApiKeyService : IApiKeyService
    {
        private readonly string _apiKey;

        public ApiKeyService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public bool ValidateKey(string apiKey)
        {
            return _apiKey == apiKey;
        }
    }
}
