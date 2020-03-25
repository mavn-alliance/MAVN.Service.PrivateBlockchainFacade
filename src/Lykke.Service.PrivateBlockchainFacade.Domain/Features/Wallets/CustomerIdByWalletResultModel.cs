namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public class CustomerIdByWalletResultModel
    {
        public string CustomerId { get; private set; }
        public CustomerWalletAddressError Error { get; private set; }

        public static CustomerIdByWalletResultModel Succeeded(string customerId)
        {
            return new CustomerIdByWalletResultModel
            {
                CustomerId = customerId,
                Error = CustomerWalletAddressError.None
            };
        }

        public static CustomerIdByWalletResultModel Failed(CustomerWalletAddressError error)
        {
            return new CustomerIdByWalletResultModel
            {
                Error = error
            };
        }
    }
}
