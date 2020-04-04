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
                name: "operations",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    transaction_hash = table.Column<string>(nullable: true),
                    customer_id = table.Column<string>(nullable: false),
                    master_wallet_address = table.Column<string>(nullable: false),
                    nonce = table.Column<long>(nullable: false),
                    timestamp = table.Column<DateTime>(nullable: false),
                    type = table.Column<string>(nullable: false),
                    status = table.Column<string>(nullable: false),
                    context_json = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.id);
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
                name: "IX_operations_transaction_hash",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "transaction_hash",
                unique: true,
                filter: "[transaction_hash] IS NOT NULL");

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
                name: "nonce_counters",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "operations",
                schema: "private_blockchain_facade");

            migrationBuilder.DropTable(
                name: "wallet_owners",
                schema: "private_blockchain_facade");
        }
    }
}
