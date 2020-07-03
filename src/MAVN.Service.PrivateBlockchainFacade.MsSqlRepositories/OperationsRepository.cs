using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks; 
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Npgsql;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class OperationsRepository : IOperationsRepository
    {
        private readonly PostgreSQLContextFactory<PbfContext> _contextFactory;

        public OperationsRepository(PostgreSQLContextFactory<PbfContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Guid> AddAsync(Guid id,
            string customerId,
            long nonce,
            string masterWalletAddress,
            OperationType type,
            string contextJson,
            DateTime createdAt, 
            string transactionHash)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = OperationEntity.Create(
                    id,
                    customerId,
                    masterWalletAddress,
                    nonce,
                    type,
                    contextJson,
                    createdAt,
                    transactionHash);

                await context.Operations.AddAsync(operation);

                await context.SaveChangesAsync();

                return operation.Id;
            }
        }

        public async Task SetStatusAsync(Guid id, OperationStatus status)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = new OperationEntity {Id = id};

                context.Operations.Attach(operation);

                operation.Status = status;
                operation.Timestamp = DateTime.UtcNow;

                await context.SaveChangesAsync();
            }
        }

        public async Task SetStatusAsync(Guid id, OperationStatus status, string hash)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = new OperationEntity {Id = id};

                context.Operations.Attach(operation);

                operation.Status = status;
                operation.TransactionHash = hash;
                operation.Timestamp = DateTime.UtcNow;

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    if (e.InnerException is PostgresException sqlException &&
                        sqlException.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        throw new DuplicateException(hash);
                    }

                    throw;
                }
            }
        }

        // todo: replace with GetByConditionAsync
        public async Task<IReadOnlyList<IOperation>> GetByStatusAsync(OperationStatus status, int max)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operations = await context.Operations
                    .Where(x => x.Status == status)
                    .OrderBy(x => x.Timestamp)
                    .Take(max)
                    .ToListAsync();

                return operations;
            }
        }

        public async Task<IOperation> GetByIdAsync(Guid id)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = await context.Operations.FindAsync(id);

                return operation;
            }
        }

        public async Task<List<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = new List<Guid>();

                var batches = ids.Batch(10);
                foreach (var batch in batches)
                {
                    var operations = await context.OperationRequests
                        .AsQueryable()
                        .Where(i => batch.Contains(i.Id))
                        .ToListAsync();

                    foreach (var operation in operations)
                    {
                        result.Add(operation.Id);
                    }
                }

                return result;
            }
        }

        public async Task<IOperation> GetByHashAsync(string hash)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = await context.Operations.SingleOrDefaultAsync(x => x.TransactionHash == hash);

                return operation;
            }
        }

        public async Task<IReadOnlyList<IOperation>> GetByConditionAsync(Expression<Func<IOperation, bool>> condition)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operations = await context.Operations
                    .Where(condition)
                    .ToListAsync();

                return operations;
            }
        }
    }
}
