using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.MsSql;
using Lykke.Service.PrivateBlockchainFacade.Domain.Common;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories
{
    public class OperationRequestsRepository : IOperationRequestsRepository
    {
        private readonly MsSqlContextFactory<PbfContext> _contextFactory;
        private readonly ISqlRepositoryHelper _sqlRepositoryHelper;
        
        static class NonceCounterSql
        {
            public static string FileName = "GetNonceCounter.sql";
            public static string MasterWalletAddressParam = "masterWalletAddress";
        }

        public OperationRequestsRepository(
            MsSqlContextFactory<PbfContext> contextFactory, 
            ISqlRepositoryHelper sqlRepositoryHelper)
        {
            _contextFactory = contextFactory;
            _sqlRepositoryHelper = sqlRepositoryHelper;
        }

        public async Task<Guid> AddAsync(string customerId, long nonce, string masterWalletAddress, OperationType type, string contextJson)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operationRequest = OperationRequestEntity.Create(
                    customerId,
                    masterWalletAddress,
                    nonce,
                    type,
                    contextJson);

                await context.OperationRequests.AddAsync(operationRequest);

                await context.SaveChangesAsync();

                return operationRequest.Id;
            }
        }

        public async Task AcceptAsync(
            Guid id,
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

                var operationRequest = new OperationRequestEntity { Id = id };
                context.OperationRequests.Remove(operationRequest);

                await context.SaveChangesAsync();
            }
        }

        public async Task AcceptBatchAsync(IEnumerable<IOperationRequest> operationRequests, Dictionary<Guid, string> operationsHashesDict)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operations = operationRequests.Select(r => OperationEntity.Create(r, operationsHashesDict[r.Id]));
                await context.Operations.AddRangeAsync(operations);

                var requestsForRemoval = operationRequests.Select(r => new OperationRequestEntity { Id = r.Id });
                context.OperationRequests.RemoveRange(requestsForRemoval);

                await context.SaveChangesAsync();
            }
        }

        public async Task<INonceCounter> GenerateNextCounterAsync(string masterWalletAddress)
        {
            var masterWalletAddressParam =
                new SqlParameter(NonceCounterSql.MasterWalletAddressParam, masterWalletAddress);

            var sqlText = _sqlRepositoryHelper.LoadSqlFromResource(NonceCounterSql.FileName);

            if (string.IsNullOrEmpty(sqlText))
                return null;

            using (var context = _contextFactory.CreateDataContext())
            {
                var nextNonceResult =
                    await context.NonceCounters.FromSql(sqlText, masterWalletAddressParam).ToListAsync();

                return nextNonceResult.Single();
            }
        }
        
        public async Task<IReadOnlyList<IOperationRequest>> GetAsync(int max)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operations = await context.OperationRequests
                    .OrderBy(x => x.Timestamp)
                    .Take(max)
                    .ToListAsync();

                return operations;
            }
        }
        
        public async Task<IOperationRequest> GetByHashAsync(string hash)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = await context.OperationRequests.SingleOrDefaultAsync(x => x.TransactionHash == hash);

                return operation;
            }
        }
        
        public async Task<IOperationRequest> GetByIdAsync(Guid id)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var operation = await context.OperationRequests.FindAsync(id);

                return operation;
            }
        }

        public async Task<Dictionary<Guid, IOperationRequest>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = new Dictionary<Guid, IOperationRequest>();

                var batches = ids.Batch(10);

                foreach (var batch in batches)
                {
                    var operations = await context.OperationRequests
                        .AsQueryable()
                        .Where(i => batch.Contains(i.Id))
                        .ToListAsync();

                    foreach (var operation in operations)
                    {
                        result.Add(operation.Id, operation);
                    }
                }

                return result;
            }
        }
    }
}
