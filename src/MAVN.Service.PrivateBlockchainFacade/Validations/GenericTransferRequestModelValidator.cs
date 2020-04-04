using FluentValidation;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;

namespace MAVN.Service.PrivateBlockchainFacade.Validations
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
