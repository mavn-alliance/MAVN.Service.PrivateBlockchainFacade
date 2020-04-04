using System;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Common
{
    public interface IRetentionable
    {
        // todo: create background job to clean old records.
        DateTime RetentionStartsAt { get; set; }
    }
}
