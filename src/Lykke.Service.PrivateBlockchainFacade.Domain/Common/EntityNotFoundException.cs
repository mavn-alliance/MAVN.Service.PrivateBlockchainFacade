using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Common
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string name, string identity, string message = null) : base(message ?? "Entity not found")
        {
            Identity = identity;
            Name = name;
        }

        public string Name { get; }
        
        public string Identity { get; }
    }
}
