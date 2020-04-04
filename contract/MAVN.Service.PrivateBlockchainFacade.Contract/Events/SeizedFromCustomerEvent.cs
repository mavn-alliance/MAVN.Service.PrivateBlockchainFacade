using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    public class SeizedFromCustomerEvent
    {
        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Amount of tokens seized
        /// </summary>
        public Money18 Amount { get; set; }
    }
}
