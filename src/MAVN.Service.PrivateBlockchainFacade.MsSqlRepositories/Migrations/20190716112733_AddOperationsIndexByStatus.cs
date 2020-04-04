using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class AddOperationsIndexByStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_operations_status",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operations_status",
                schema: "private_blockchain_facade",
                table: "operations");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
