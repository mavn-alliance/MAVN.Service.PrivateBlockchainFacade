using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class OperationsAddIndexByTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_operations_timestamp",
                schema: "private_blockchain_facade",
                table: "operations",
                column: "timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operations_timestamp",
                schema: "private_blockchain_facade",
                table: "operations");
        }
    }
}
