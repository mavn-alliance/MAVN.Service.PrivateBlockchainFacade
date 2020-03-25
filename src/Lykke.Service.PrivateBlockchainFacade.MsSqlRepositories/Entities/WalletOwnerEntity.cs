using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities
{
    [Table("wallet_owners")]
    public class WalletOwnerEntity : IWalletOwner
    {
        [Key] 
        [Column("owner_id")] 
        public string OwnerId { get; set; }
        
        [Required]
        [Column("wallet_id")] 
        public string WalletId { get; set; }
        
        internal static WalletOwnerEntity Create(
            string ownerId, 
            string walletId)
        {
            return new WalletOwnerEntity
            {
                OwnerId = ownerId,
                WalletId = walletId
            };
        }
    }
}
