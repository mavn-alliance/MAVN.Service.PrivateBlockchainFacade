using System.Data.Common;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts
{
    public class PbfContext : PostgreSQLContext
    {
        private const string Schema = "private_blockchain_facade";
        
        internal DbSet<OperationEntity> Operations { get; set; }
        internal DbSet<NonceCounterEntity> NonceCounters { get; set; }
        internal DbSet<WalletOwnerEntity> WalletOwners { get; set; }
        internal DbSet<BonusRewardDeduplicationLogEntity> BonusRewardDeduplicationLog { get; set; }  
        internal DbSet<TransferDeduplicationLogEntity> TransferDeduplicationLog { get; set; }
        internal DbSet<OperationDeduplicationLogEntity> OperationDeduplicationLog { get; set; }
        internal DbSet<WalletLinkingDeduplicationLogEntity> WalletLinkingDeduplicationLog { get; set; }
        internal DbSet<OperationRequestEntity> OperationRequests { get; set; }

        public PbfContext() : base(Schema)
        {
        }

        public PbfContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public PbfContext(DbContextOptions<PbfContext> options, bool isForMocks = false)
            : base(Schema, options, isForMocks)
        {
        }

        public PbfContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }
        
        public PbfContext(
            string connectionString,
            bool isTraceEnabled,
            int commandTimeoutSeconds)
            : base(Schema, connectionString, isTraceEnabled, commandTimeoutSeconds)
        {
        }

        protected override void OnMAVNModelCreating(ModelBuilder modelBuilder)
        {
            var operationEntityBuilder = modelBuilder.Entity<OperationEntity>();
            operationEntityBuilder.Property(c => c.Type).HasConversion(new EnumToStringConverter<OperationType>());
            operationEntityBuilder.Property(c => c.Status).HasConversion(new EnumToStringConverter<OperationStatus>());
            
            operationEntityBuilder.HasIndex(c => c.TransactionHash).IsUnique();
            operationEntityBuilder.HasIndex(c => c.Status);
            operationEntityBuilder.HasIndex(c => c.Timestamp);
            operationEntityBuilder.HasIndex(c => c.MasterWalletAddress);
            operationEntityBuilder.HasIndex(c => c.Type);
            operationEntityBuilder.HasIndex(c => new {c.MasterWalletAddress, c.Type, c.Status});
            operationEntityBuilder.HasIndex(c => new {c.CustomerId, c.Type, c.Status});

            var operationRequestEntityBuilder = modelBuilder.Entity<OperationRequestEntity>();
            operationRequestEntityBuilder.Property(c => c.Type)
                .HasConversion(new EnumToStringConverter<OperationType>());
            operationRequestEntityBuilder.Property(c => c.Status)
                .HasConversion(new EnumToStringConverter<OperationStatus>());
            
            operationRequestEntityBuilder
                .HasIndex(x => x.Timestamp)
                .IsUnique(false);
            
            

            var walletOwnerEntityBuilder = modelBuilder.Entity<WalletOwnerEntity>();
            walletOwnerEntityBuilder.HasIndex(c => c.WalletId).IsUnique();
        }
    }
}
