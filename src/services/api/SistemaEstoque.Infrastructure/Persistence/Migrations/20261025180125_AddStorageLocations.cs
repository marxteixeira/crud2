using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaEstoque.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estantes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    normalized_code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estantes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prateleiras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rack_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    normalized_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prateleiras", x => x.id);
                    table.ForeignKey(
                        name: "FK_prateleiras_estantes_rack_id",
                        column: x => x.rack_id,
                        principalTable: "estantes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_estantes_normalized_code",
                table: "estantes",
                column: "normalized_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estantes_status",
                table: "estantes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_prateleiras_rack_id_normalized_code",
                table: "prateleiras",
                columns: new[] { "rack_id", "normalized_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prateleiras_status",
                table: "prateleiras",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prateleiras");

            migrationBuilder.DropTable(
                name: "estantes");
        }
    }
}
