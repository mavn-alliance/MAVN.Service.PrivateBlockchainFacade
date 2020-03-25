using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// Event which is raised when we have validation error when trying to initiate TransferToExternal
    /// </summary>
    public class TransferToExternalFailedEvent
    {
        /// <summary>
        /// Id of the customer who has requested tha external transfer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Amount of tokens
        /// </summary>
        public Money18 Amount { get; set; }
    }
}
