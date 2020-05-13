using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MAVN.Common.MsSql;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class WalletLinkingDeduplicationLogRepository : IDeduplicationLogRepository<WalletLinkingDeduplicationLogEntity>
    {
        private readonly MsSqlContextFactory<PbfContext> _contextFactory;

        public WalletLinkingDeduplicationLogRepository(MsSqlContextFactory<PbfContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> IsDuplicateAsync(string key)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var deduplicationEntry = WalletLinkingDeduplicationLogEntity.Create(key, DateTime.UtcNow);

                try
                {
                    await context.WalletLinkingDeduplicationLog.AddAsync(deduplicationEntry);

                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is SqlException sqlException &&
                        sqlException.Number == MsSqlErrorCodes.PrimaryKeyConstraintViolation)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
