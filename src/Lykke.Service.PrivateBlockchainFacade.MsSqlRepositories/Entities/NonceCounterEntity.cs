using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities
{
    [Table("nonce_counters")]
    public class NonceCounterEntity : INonceCounter
    {
        [Key]
        [Column("master_wallet_address")] 
        public string MasterWalletAddress { get; set; }
        
        [Column("counter_value")] 
        [Required] public long CounterValue { get; set; }
        
        internal static NonceCounterEntity Create(
            string masterWalletAddress, 
            long counterValue)
        {
            return new NonceCounterEntity
            {
                MasterWalletAddress = masterWalletAddress,
                CounterValue = counterValue
            };
        }
    }
}
