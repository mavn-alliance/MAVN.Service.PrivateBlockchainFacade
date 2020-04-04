using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Common.MsSql;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class WalletOwnersRepository : IWalletOwnersRepository
    {
        private readonly MsSqlContextFactory<PbfContext> _contextFactory;
        private readonly ILog _log;

        public WalletOwnersRepository(MsSqlContextFactory<PbfContext> contextFactory, ILogFactory logFactory)
        {
            _contextFactory = contextFactory;
            _log = logFactory.CreateLog(this);
        }

        public async Task<IWalletOwner> AddAsync(string ownerId, string walletAddress)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var walletOwnerEntity = WalletOwnerEntity.Create(ownerId, walletAddress);
                
                try
                {
                    await context.WalletOwners.AddAsync(walletOwnerEntity);

                    await context.SaveChangesAsync();

                    return walletOwnerEntity;
                }
                catch (DbUpdateException e)
                {
                    _log.Error(e, context: new {ownerId, walletId = walletAddress});

                    throw new WalletOwnerDuplicateException(ownerId, walletAddress);
                }
            }
        }

        public async Task<IWalletOwner> GetByOwnerIdAsync(string ownerId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var walletOwner = await context.WalletOwners.FindAsync(ownerId);

                return walletOwner;
            }
        }

        public async Task<IWalletOwner> GetByWalletAddressAsync(string walletAddress)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var walletOwner = await context.WalletOwners.SingleOrDefaultAsync(x => x.WalletId == walletAddress);

                return walletOwner;
            }
        }
    }
}
