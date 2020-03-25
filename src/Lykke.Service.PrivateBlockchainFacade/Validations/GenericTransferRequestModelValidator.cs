using FluentValidation;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;

namespace Lykke.Service.PrivateBlockchainFacade.Validations
{
    public class GenericTransferRequestModelValidator
    : AbstractValidator<GenericTransferRequestModel>
    {
        public GenericTransferRequestModelValidator()
        {
            RuleFor(gt => gt.Amount)
                .GreaterThan(0);
        }
    }
}
