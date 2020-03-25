using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class AddIndexesToOperationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "master_wallet_address",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_operations_master_wallet_address",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "master_wallet_address");

            migrationBuilder.CreateIndex(
                name: "IX_operations_type",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_operations_master_wallet_address_type_status",
                schema: "private_blockchain_facade",
                table: "operations",
                columns: new[] { "master_wallet_address", "type", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operations_master_wallet_address",
                schema: "private_blockchain_facade",
                table: "operations");

            migrationBuilder.DropIndex(
                name: "IX_operations_type",
                schema: "private_blockchain_facade",
                table: "operations");

            migrationBuilder.DropIndex(
                name: "IX_operations_master_wallet_address_type_status",
                schema: "private_blockchain_facade",
                table: "operations");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "master_wallet_address",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
