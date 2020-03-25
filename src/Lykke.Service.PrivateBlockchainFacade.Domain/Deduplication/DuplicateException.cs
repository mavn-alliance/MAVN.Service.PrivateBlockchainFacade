using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication
{
    public class DuplicateException : Exception
    {
        public DuplicateException()
        {
        }

        public DuplicateException(string key, string message = null): base(message ?? "There is a duplicate")
        {
            Key = key;
        }
        
        public string Key { get; }
    }
}
