using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Common
{
    public interface IRetentionable
    {
        // todo: create background job to clean old records.
        DateTime RetentionStartsAt { get; set; }
    }
}
