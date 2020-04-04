namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses
{
    public class BonusRewardResultModel
    {
        public BonusRewardError Error { get; private set; }
        
        public static BonusRewardResultModel Succeeded()
        {
            return new BonusRewardResultModel
            {
                Error = BonusRewardError.None
            };
        }

        public static BonusRewardResultModel Failed(BonusRewardError error)
        {
            return new BonusRewardResultModel
            {
                Error = error
            };       
        }
    }
}
