namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Error codes
    /// </summary>
    public enum AddOperationError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        /// <summary>
        /// The same request was already processed
        /// </summary>
        DuplicateRequest,
    }
}
