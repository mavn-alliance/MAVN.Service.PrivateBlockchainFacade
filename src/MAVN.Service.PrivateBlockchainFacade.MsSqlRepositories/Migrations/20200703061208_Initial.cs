using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "private_blockchain_facade");

            migrationBuilder.CreateTable(
                name: "bonus_reward_deduplication_log",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    deduplication_key = table.Column<string>(nullable: false),
                    retention_starts_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus_reward_deduplication_log", x => x.deduplication_key);
                });

            migrationBuilder.CreateTable(
                name: "nonce_counters",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    master_wallet_address = table.Column<string>(nullable: false),
                    counter_value = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nonce_counters", x => x.master_wallet_address);
                });

            migrationBuilder.CreateTable(
                name: "operation_deduplication_log",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    deduplication_key = table.Column<string>(nullable: false),
                    retention_starts_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_deduplication_log", x => x.deduplication_key);
                });

            migrationBuilder.CreateTable(
                name: "operation_requests",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    transaction_hash = table.Column<string>(nullable: true),
                    customer_id = table.Column<string>(nullable: true),
                    master_wallet_address = table.Column<string>(nullable: false),
                    nonce = table.Column<long>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    type = table.Column<string>(nullable: false),
                    status = table.Column<string>(nullable: false),
                    context_json = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operations",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    transaction_hash = table.Column<string>(nullable: true),
                    customer_id = table.Column<string>(nullable: true),
                    master_wallet_address = table.Column<string>(nullable: false),
                    nonce = table.Column<long>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    type = table.Column<string>(nullable: false),
                    status = table.Column<string>(nullable: false),
                    context_json = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transfer_deduplication_log",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    deduplication_key = table.Column<string>(nullable: false),
                    retention_starts_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_deduplication_log", x => x.deduplication_key);
                });

            migrationBuilder.CreateTable(
                name: "wallet_linking_deduplication_log",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    deduplication_key = table.Column<string>(nullable: false),
                    retention_starts_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_linking_deduplication_log", x => x.deduplication_key);
                });

            migrationBuilder.CreateTable(
                name: "wallet_owners",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    owner_id = table.Column<string>(nullable: false),
                    wallet_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_owners", x => x.owner_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_operation_requests_timestamp",
                schema: "private_blockchain_facade",
                table: "operation_requests",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_operations_master_wallet_address",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "master_wallet_address");

            migrationBuilder.CreateIndex(
                name: "IX_operations_status",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_operations_timestamp",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_operations_transaction_hash",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "transaction_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operations_type",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_operations_customer_id_type_status",
                schema: "private_blockchain_facade",
                table: "operations",
                columns: new[] { "customer_id", "type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_operations_master_wallet_address_type_status",
                schema: "private_blockchain_facade",
                table: "operations",
                columns: new[] { "master_wallet_address", "type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_owners_wallet_id",
                schema: "private_blockchain_facade",
                table: "wallet_owners",
                column: "wallet_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bonus_reward_deduplication_log",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "nonce_counters",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "operation_deduplication_log",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "operation_requests",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "operations",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "transfer_deduplication_log",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "wallet_linking_deduplication_log",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "wallet_owners",
                schema: "private_blockchain_facade");
        }
    }
}
