using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "main_entities");

            migrationBuilder.CreateTable(
                name: "tree_roots",
                schema: "main_entities",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", maxLength: 255, nullable: false),
                    data_storage_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    DataStoragesGuids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_legacy = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    content_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deleted_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_roots_pkey", x => x.uuid);
                });

            migrationBuilder.CreateTable(
                name: "tree_nodes",
                schema: "main_entities",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", maxLength: 255, nullable: false),
                    ParentGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    TreeRootGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_legacy = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    content_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deleted_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_nodes_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_tree_nodes_tree_nodes_ParentGuid",
                        column: x => x.ParentGuid,
                        principalSchema: "main_entities",
                        principalTable: "tree_nodes",
                        principalColumn: "uuid");
                    table.ForeignKey(
                        name: "FK_tree_nodes_tree_roots_ParentGuid",
                        column: x => x.ParentGuid,
                        principalSchema: "main_entities",
                        principalTable: "tree_roots",
                        principalColumn: "uuid");
                    table.ForeignKey(
                        name: "FK_tree_nodes_tree_roots_TreeRootGuid",
                        column: x => x.TreeRootGuid,
                        principalSchema: "main_entities",
                        principalTable: "tree_roots",
                        principalColumn: "uuid");
                });

            migrationBuilder.CreateTable(
                name: "tree_leaves",
                schema: "main_entities",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_legacy = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    content_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deleted_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentGuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_leaves_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_tree_leaves_tree_nodes_ParentGuid",
                        column: x => x.ParentGuid,
                        principalSchema: "main_entities",
                        principalTable: "tree_nodes",
                        principalColumn: "uuid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tree_leaves_ParentGuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentGuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentGuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_TreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "TreeRootGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tree_leaves",
                schema: "main_entities");

            migrationBuilder.DropTable(
                name: "tree_nodes",
                schema: "main_entities");

            migrationBuilder.DropTable(
                name: "tree_roots",
                schema: "main_entities");
        }
    }
}
