using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Migrations
{
    public partial class AddOperationRequestEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operation_requests",
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
                    context_json = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_requests", x => x.id);
                });

            migrationBuilder.Sql(@"
                insert into private_blockchain_facade.operation_requests(
                  id, transaction_hash, customer_id,
                  master_wallet_address, nonce, timestamp,
                  type, status, context_json
                )
                select
                  id,
                  transaction_hash,
                  customer_id,
                  master_wallet_address,
                  nonce,
                  timestamp,
                  type,
                  status,
                  context_json
                from
                  private_blockchain_facade.operations
                where
                  status = 'Created';

                delete from
                  private_blockchain_facade.operations
                where
                  status = 'Created';

                commit;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                insert into private_blockchain_facade.operations(
                  id, transaction_hash, customer_id,
                  master_wallet_address, nonce, timestamp,
                  type, status, context_json
                )
                select
                  id,
                  transaction_hash,
                  customer_id,
                  master_wallet_address,
                  nonce,
                  timestamp,
                  type,
                  status,
                  context_json
                from
                  private_blockchain_facade.operation_requests;

                delete from
                  private_blockchain_facade.operation_requests;

                commit;
            ");
            
            migrationBuilder.DropTable(
                name: "operation_requests",
                schema: "private_blockchain_facade");
        }
    }
}
