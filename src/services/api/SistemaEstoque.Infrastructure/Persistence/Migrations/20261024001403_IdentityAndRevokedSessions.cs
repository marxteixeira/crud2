using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaEstoque.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAndRevokedSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contas_de_usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_subject = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    pending_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contas_de_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_revogadas",
                columns: table => new
                {
                    provider_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessoes_revogadas", x => x.provider_session_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contas_de_usuario_email",
                table: "contas_de_usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contas_de_usuario_identity_subject",
                table: "contas_de_usuario",
                column: "identity_subject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contas_de_usuario_pending_email",
                table: "contas_de_usuario",
                column: "pending_email",
                unique: true,
                filter: "\"pending_email\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contas_de_usuario");

            migrationBuilder.DropTable(
                name: "sessoes_revogadas");
        }
    }
}
