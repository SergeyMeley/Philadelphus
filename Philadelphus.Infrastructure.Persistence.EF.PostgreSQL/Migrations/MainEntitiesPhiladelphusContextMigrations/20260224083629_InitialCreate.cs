using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shrub_members_content");

            migrationBuilder.EnsureSchema(
                name: "shrub_members");

            migrationBuilder.CreateTable(
                name: "element_attributes",
                schema: "shrub_members_content",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    declaring_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    declaring_owner_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    value_type_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    value_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_collection_value = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    values_uuids = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    visibility_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    override_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("element_attributes_pkey", x => x.uuid);
                });

            migrationBuilder.CreateTable(
                name: "working_trees",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    data_storage_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("working_trees_pkey", x => x.uuid);
                });

            migrationBuilder.CreateTable(
                name: "tree_roots",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "session_user"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_roots_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_tree_roots_working_trees_owning_working_tree_uuid",
                        column: x => x.owning_working_tree_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "working_trees",
                        principalColumn: "uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tree_nodes",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_tree_root_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_tree_node_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    data_type_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_nodes_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_tree_nodes_tree_roots_parent_tree_root_uuid",
                        column: x => x.parent_tree_root_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "tree_roots",
                        principalColumn: "uuid");
                    table.ForeignKey(
                        name: "FK_tree_nodes_working_trees_owning_working_tree_uuid",
                        column: x => x.owning_working_tree_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "working_trees",
                        principalColumn: "uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tree_leaves",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_tree_node_uuid = table.Column<Guid>(type: "uuid", nullable: true),
                    data_type_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sequence = table.Column<long>(type: "bigint", nullable: true),
                    alias = table.Column<string>(type: "text", nullable: true),
                    custom_code = table.Column<string>(type: "text", nullable: true),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "session_user"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_leaves_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_tree_leaves_tree_nodes_parent_tree_node_uuid",
                        column: x => x.parent_tree_node_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "tree_nodes",
                        principalColumn: "uuid");
                    table.ForeignKey(
                        name: "FK_tree_leaves_working_trees_owning_working_tree_uuid",
                        column: x => x.owning_working_tree_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "working_trees",
                        principalColumn: "uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tree_leaves_owning_working_tree_uuid",
                schema: "shrub_members",
                table: "tree_leaves",
                column: "owning_working_tree_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_leaves_parent_tree_node_uuid",
                schema: "shrub_members",
                table: "tree_leaves",
                column: "parent_tree_node_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_owning_working_tree_uuid",
                schema: "shrub_members",
                table: "tree_nodes",
                column: "owning_working_tree_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_parent_tree_root_uuid",
                schema: "shrub_members",
                table: "tree_nodes",
                column: "parent_tree_root_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_roots_owning_working_tree_uuid",
                schema: "shrub_members",
                table: "tree_roots",
                column: "owning_working_tree_uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "element_attributes",
                schema: "shrub_members_content");

            migrationBuilder.DropTable(
                name: "tree_leaves",
                schema: "shrub_members");

            migrationBuilder.DropTable(
                name: "tree_nodes",
                schema: "shrub_members");

            migrationBuilder.DropTable(
                name: "tree_roots",
                schema: "shrub_members");

            migrationBuilder.DropTable(
                name: "working_trees",
                schema: "shrub_members");
        }
    }
}
