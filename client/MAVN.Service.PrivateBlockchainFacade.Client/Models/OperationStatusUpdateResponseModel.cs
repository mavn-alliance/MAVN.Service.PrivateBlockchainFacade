using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Operation status update response details
    /// </summary>
    [PublicAPI]
    public class OperationStatusUpdateResponseModel
    {
        /// <summary>
        /// Operation status update error
        /// </summary>
        public OperationStatusUpdateError Error { get; set; }  
    }
}
