using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaEstoque.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSupplierLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produtos_fornecedores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos_fornecedores", x => x.id);
                    table.ForeignKey(
                        name: "FK_produtos_fornecedores_fornecedores_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_produtos_fornecedores_produtos_product_id",
                        column: x => x.product_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produtos_fornecedores_product_id_status",
                table: "produtos_fornecedores",
                columns: new[] { "product_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_produtos_fornecedores_product_id_supplier_id",
                table: "produtos_fornecedores",
                columns: new[] { "product_id", "supplier_id" },
                unique: true,
                filter: "status = 'Ativa'");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_fornecedores_supplier_id_status",
                table: "produtos_fornecedores",
                columns: new[] { "supplier_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produtos_fornecedores");
        }
    }
}
