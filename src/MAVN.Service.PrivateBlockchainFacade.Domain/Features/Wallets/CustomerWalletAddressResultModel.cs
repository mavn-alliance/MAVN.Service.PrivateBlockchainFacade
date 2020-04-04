namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public class CustomerWalletAddressResultModel
    {
        public string WalletAddress { get; private set; }
        public CustomerWalletAddressError Error { get; private set; }

        public static CustomerWalletAddressResultModel Succeeded(string walletAddress)
        {
            return new CustomerWalletAddressResultModel
            {
                WalletAddress = walletAddress,
                Error = CustomerWalletAddressError.None
            };
        }

        public static CustomerWalletAddressResultModel Failed(CustomerWalletAddressError error)
        {
            return new CustomerWalletAddressResultModel
            {
                Error = error
            };       
        }
    }
}
