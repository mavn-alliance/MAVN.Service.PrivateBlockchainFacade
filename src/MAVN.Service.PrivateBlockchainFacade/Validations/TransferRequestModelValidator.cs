using FluentValidation;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;

namespace MAVN.Service.PrivateBlockchainFacade.Validations
{
    public class TransferRequestModelValidator
        : AbstractValidator<TransferRequestModel>
    {
        public TransferRequestModelValidator()
        {
            RuleFor(tr => tr.Amount)
                .GreaterThan(0);
        }
    }
}
