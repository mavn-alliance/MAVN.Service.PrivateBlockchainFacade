using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class BonusRewardDeduplicationStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bonus_charge_deduplication_log",
                schema: "private_blockchain_facade",
                columns: table => new
                {
                    deduplication_key = table.Column<string>(nullable: false),
                    retention_starts_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus_charge_deduplication_log", x => x.deduplication_key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bonus_charge_deduplication_log",
                schema: "private_blockchain_facade");
        }
    }
}
