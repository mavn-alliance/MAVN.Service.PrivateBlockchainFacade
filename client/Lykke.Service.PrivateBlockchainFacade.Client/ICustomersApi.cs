using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Customers API interface
    /// </summary>
    [PublicAPI]
    public interface ICustomersApi
    {
        /// <summary>
        /// Get customer's balance
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <returns></returns>
        [Get("/api/customers/{customerId}/balance")]
        Task<CustomerBalanceResponseModel> GetBalanceAsync(Guid customerId);

        /// <summary>
        /// Get customer's wallet address
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <returns></returns>
        [Get("/api/customers/{customerId}/walletAddress")]
        Task<CustomerWalletAddressResponseModel> GetWalletAddress(Guid customerId);

        /// <summary>
        /// Get customer id by wallet address
        /// </summary>
        /// <param name="walletAddress">BC address of the wallet</param>
        /// <returns></returns>
        [Get("/api/customers/{walletAddress}/customerId")]
        Task<CustomerIdByWalletAddressResponse> GetCustomerIdByWalletAddress(string walletAddress);
    }
}
