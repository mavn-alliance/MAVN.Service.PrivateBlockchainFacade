using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class MakeCustomerIdNotNullableInOperations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "customer_id",
                schema: "private_blockchain_facade",
                table: "operations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
