using FluentValidation;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;

namespace Lykke.Service.PrivateBlockchainFacade.Validations
{
    public class BonusRewardRequestModelValidator
        : AbstractValidator<BonusRewardRequestModel>
    {
        public BonusRewardRequestModelValidator()
        {
            RuleFor(br => br.Amount)
                .GreaterThan(0);
        }
    }
}
