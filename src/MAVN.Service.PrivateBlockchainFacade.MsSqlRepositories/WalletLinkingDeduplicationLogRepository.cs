using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class WalletLinkingDeduplicationLogRepository : IDeduplicationLogRepository<WalletLinkingDeduplicationLogEntity>
    {
        private readonly PostgreSQLContextFactory<PbfContext> _contextFactory;

        public WalletLinkingDeduplicationLogRepository(PostgreSQLContextFactory<PbfContext> contextFactory)
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
                    if (e.InnerException is PostgresException sqlException &&
                        sqlException.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
