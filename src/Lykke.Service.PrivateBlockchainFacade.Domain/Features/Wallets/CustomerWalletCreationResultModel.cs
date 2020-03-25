namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public class CustomerWalletCreationResultModel
    {
        public CustomerWalletCreationError Error { get; private set; }

        public static CustomerWalletCreationResultModel Succeeded()
        {
            return new CustomerWalletCreationResultModel
            {
                Error = CustomerWalletCreationError.None
            };
        }

        public static CustomerWalletCreationResultModel Failed(CustomerWalletCreationError error)
        {
            return new CustomerWalletCreationResultModel
            {
                Error = error
            };       
        }
    }
}
