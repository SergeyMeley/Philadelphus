using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class fix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.RenameColumn(
                name: "ParentTreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "parent_tree_root_uuid");

            migrationBuilder.RenameColumn(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "parent_tree_node_uuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "IX_tree_nodes_parent_tree_node_uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "parent_tree_node_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_leaves",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_nodes_parent_tree_node_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "parent_tree_node_uuid",
                principalSchema: "main_entities",
                principalTable: "tree_nodes",
                principalColumn: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_nodes_parent_tree_node_uuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropColumn(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.RenameColumn(
                name: "parent_tree_root_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "ParentTreeRootGuid");

            migrationBuilder.RenameColumn(
                name: "parent_tree_node_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "ParentTreeNodeGuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_nodes_parent_tree_node_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "IX_tree_nodes_ParentTreeNodeGuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentTreeNodeGuid",
                principalSchema: "main_entities",
                principalTable: "tree_nodes",
                principalColumn: "uuid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
