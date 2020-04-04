using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Operations
{
    /// <summary>
    /// Represents information of seize event.
    /// </summary>
    public class SeizeToInternalContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SeizeToInternalContext"/>. 
        /// </summary>
        public SeizeToInternalContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SeizeToInternalContext"/> with parameters. 
        /// </summary>
        /// <param name="account">The customer account.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="reason">The reason of seizing.</param>
        public SeizeToInternalContext(string account, Money18 amount, string reason)
        {
            Account = account;
            Amount = amount;
            Reason = reason;
        }

        /// <summary>
        /// The customer account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// The amount.
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// The reason of seizing.
        /// </summary>
        public string Reason { get; set; }
    }
}
