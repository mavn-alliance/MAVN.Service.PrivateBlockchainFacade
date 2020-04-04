using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class RenameChargeToReward : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_bonus_charge_deduplication_log",
                schema: "private_blockchain_facade",
                table: "bonus_charge_deduplication_log");

            migrationBuilder.RenameTable(
                name: "bonus_charge_deduplication_log",
                schema: "private_blockchain_facade",
                newName: "bonus_reward_deduplication_log",
                newSchema: "private_blockchain_facade");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bonus_reward_deduplication_log",
                schema: "private_blockchain_facade",
                table: "bonus_reward_deduplication_log",
                column: "deduplication_key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_bonus_reward_deduplication_log",
                schema: "private_blockchain_facade",
                table: "bonus_reward_deduplication_log");

            migrationBuilder.RenameTable(
                name: "bonus_reward_deduplication_log",
                schema: "private_blockchain_facade",
                newName: "bonus_charge_deduplication_log",
                newSchema: "private_blockchain_facade");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bonus_charge_deduplication_log",
                schema: "private_blockchain_facade",
                table: "bonus_charge_deduplication_log",
                column: "deduplication_key");
        }
    }
}
