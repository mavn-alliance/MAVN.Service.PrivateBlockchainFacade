using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Transfer response details
    /// </summary>
    [PublicAPI]
    public class TransferResponseModel
    {
        /// <summary>
        /// Transfer error
        /// </summary>
        public TransferError Error { get; set; }

        /// <summary>
        /// Id of the created operation
        /// </summary>
        public string OperationId { get; set; }
    }
}
