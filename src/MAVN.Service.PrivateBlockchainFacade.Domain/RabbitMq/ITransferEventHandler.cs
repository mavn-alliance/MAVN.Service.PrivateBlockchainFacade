using System;
using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransferEventHandler
    {
        /// <summary>
        /// Handle transfer event from blockchain watcher 
        /// </summary>
        /// <param name="sourceAddress">The source wallet address</param>
        /// <param name="targetAddress">The target wallet address</param>
        /// <param name="amount">The amount of transfer</param>
        /// <param name="transactionHash">The transfer transaction hash</param>
        /// <param name="observedAt">The date and time transaction happened at</param>
        /// <returns></returns>
        Task HandleAsync(
            string sourceAddress, 
            string targetAddress, 
            Money18 amount, 
            string transactionHash,
            DateTime observedAt);
    }
}
