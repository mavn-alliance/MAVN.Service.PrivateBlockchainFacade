using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Service.PrivateBlockchainFacade.Auth
{
    public class KeyAuthHandler : AuthenticationHandler<KeyAuthOptions>
    {
        private readonly IApiKeyService _apiKeyService;

        public KeyAuthHandler(IApiKeyService apiKeyService,
            IOptionsMonitor<KeyAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _apiKeyService = apiKeyService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            if (Context.Request.Path.ToString().ToLower() != "/api/isalive" || Context.Request.Method != "GET")
            {
                if (!Context.Request.Headers.TryGetValue(KeyAuthOptions.DefaultHeaderName, out var headerValue))
                    return AuthenticateResult.Fail("No api key header.");

                var apiKey = headerValue.First();
                if (!_apiKeyService.ValidateKey(apiKey))
                    return AuthenticateResult.Fail("Invalid API key.");
            }

            var identity = new ClaimsIdentity("apikey");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, "apikey");
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
