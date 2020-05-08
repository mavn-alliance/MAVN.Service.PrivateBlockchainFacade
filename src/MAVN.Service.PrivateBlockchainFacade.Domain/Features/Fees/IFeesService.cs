using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Fees
{
    public interface IFeesService
    {
       Task<FeesError> SetTransfersToPublicFeeAsync(Money18 fee);

       Task<Money18> GetTransfersToPublicFeeAsync();
    }
}
