using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Services
{
    public interface ITokensService
    {
        Task<Money18> GetTotalTokensSupplyAsync();
        Task<Money18> GetTokenGatewayBalance();
    }
}
