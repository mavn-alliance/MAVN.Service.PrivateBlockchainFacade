using Microsoft.AspNetCore.Authentication;

namespace Lykke.Service.PrivateBlockchainFacade.Auth
{
    public class KeyAuthOptions : AuthenticationSchemeOptions
    {
        public const string DefaultHeaderName = "api-key";
        public const string AuthenticationScheme = "Automatic";
    }
}
