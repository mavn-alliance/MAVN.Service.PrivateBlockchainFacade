using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class AddAnotherToOperationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_operations_customer_id_type_status",
                schema: "private_blockchain_facade",
                table: "operations",
                columns: new[] { "customer_id", "type", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operations_customer_id_type_status",
                schema: "private_blockchain_facade",
                table: "operations");

            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
