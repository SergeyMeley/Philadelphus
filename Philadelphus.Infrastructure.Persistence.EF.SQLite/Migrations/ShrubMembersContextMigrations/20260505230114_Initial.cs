using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Migrations.ShrubMembersContextMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shrub_members_content");

            migrationBuilder.EnsureSchema(
                name: "shrub_members");

            migrationBuilder.CreateTable(
                name: "working_trees",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    data_storage_uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    sequence = table.Column<long>(type: "INTEGER", nullable: true),
                    alias = table.Column<string>(type: "TEXT", nullable: true),
                    custom_code = table.Column<string>(type: "TEXT", nullable: true),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("working_trees_pkey", x => x.uuid);
                });

            migrationBuilder.CreateTable(
                name: "element_attributes",
                schema: "shrub_members_content",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    declaring_uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    owner_uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    declaring_owner_uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    value_type_uuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    value_uuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_collection_value = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    values_uuids = table.Column<string>(type: "TEXT", nullable: true),
                    visibility_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    override_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    sequence = table.Column<long>(type: "INTEGER", nullable: true),
                    alias = table.Column<string>(type: "TEXT", nullable: true),
                    custom_code = table.Column<string>(type: "TEXT", nullable: true),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("element_attributes_pkey", x => x.uuid);
                    table.ForeignKey(
                        name: "FK_element_attributes_working_trees_owning_working_tree_uuid",
                        column: x => x.owning_working_tree_uuid,
                        principalSchema: "shrub_members",
                        principalTable: "working_trees",
                        principalColumn: "uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tree_roots",
                schema: "shrub_members",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    sequence = table.Column<long>(type: "INTEGER", nullable: true),
                    alias = table.Column<string>(type: "TEXT", nullable: true),
                    custom_code = table.Column<string>(type: "TEXT", nullable: true),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "TEXT", nullable: false)
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
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_tree_root_uuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    parent_tree_node_uuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    data_type_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    sequence = table.Column<long>(type: "INTEGER", nullable: true),
                    alias = table.Column<string>(type: "TEXT", nullable: true),
                    custom_code = table.Column<string>(type: "TEXT", nullable: true),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "TEXT", nullable: false)
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
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_tree_node_uuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    data_type_id = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    sequence = table.Column<long>(type: "INTEGER", nullable: true),
                    alias = table.Column<string>(type: "TEXT", nullable: true),
                    custom_code = table.Column<string>(type: "TEXT", nullable: true),
                    is_hidden = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_by = table.Column<string>(type: "TEXT", nullable: true),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deleted_by = table.Column<string>(type: "TEXT", nullable: true),
                    owning_working_tree_uuid = table.Column<Guid>(type: "TEXT", nullable: false)
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
                name: "IX_element_attributes_owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes",
                column: "owning_working_tree_uuid");

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
                column: "owning_working_tree_uuid",
                unique: true);
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
