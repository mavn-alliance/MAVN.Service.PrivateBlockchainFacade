using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class AddTimestampIndexForRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_operation_requests_timestamp",
                schema: "private_blockchain_facade",
                table: "operation_requests",
                column: "timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_operation_requests_timestamp",
                schema: "private_blockchain_facade",
                table: "operation_requests");
        }
    }
}
