using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.TreeRepositoriesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "general");

            migrationBuilder.EnsureSchema(
                name: "repositories");

            migrationBuilder.CreateTable(
                name: "audit_infos",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RepositoryElementUuid = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_on = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    updated_content_on = table.Column<string>(type: "text", nullable: true),
                    updated_content_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<string>(type: "text", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_infos_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "main_entity",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentGuid = table.Column<string>(type: "text", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    HasContent = table.Column<bool>(type: "boolean", nullable: false),
                    IsOriginal = table.Column<bool>(type: "boolean", nullable: false),
                    is_legacy = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("main_entity_base_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_main_entity_audit_infos_AuditInfoId",
                        column: x => x.AuditInfoId,
                        principalSchema: "general",
                        principalTable: "audit_infos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tree_repositories",
                schema: "repositories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    own_data_storage_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildTreeRootGuids = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("main_entity_base_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_tree_repositories_main_entity_id",
                        column: x => x.id,
                        principalSchema: "general",
                        principalTable: "main_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_main_entity_AuditInfoId",
                schema: "general",
                table: "main_entity",
                column: "AuditInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tree_repositories",
                schema: "repositories");

            migrationBuilder.DropTable(
                name: "main_entity",
                schema: "general");

            migrationBuilder.DropTable(
                name: "audit_infos",
                schema: "general");
        }
    }
}
