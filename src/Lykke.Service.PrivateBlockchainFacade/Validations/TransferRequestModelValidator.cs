using FluentValidation;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;

namespace Lykke.Service.PrivateBlockchainFacade.Validations
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
