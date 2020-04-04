using FluentValidation;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;

namespace MAVN.Service.PrivateBlockchainFacade.Validations
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
