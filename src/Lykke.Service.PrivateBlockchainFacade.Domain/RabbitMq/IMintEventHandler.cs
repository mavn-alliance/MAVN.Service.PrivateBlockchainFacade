using System;
using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface IMintEventHandler
    {
        Task HandleAsync(string transactionHash, Money18 amount, string walletAddress, DateTime observedAt);
    }
}
