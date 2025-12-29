using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class rename_guid_to_uuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_leaves_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_tree_leaves_tree_roots_ParentTreeRootGuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_roots_TreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_leaves_ParentGuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.RenameColumn(
                name: "DataStoragesGuids",
                schema: "main_entities",
                table: "tree_roots",
                newName: "DataStoragesUuids");

            migrationBuilder.RenameColumn(
                name: "TreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "TreeRootUuid");

            migrationBuilder.RenameColumn(
                name: "ParentGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "ParentUuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_nodes_TreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "IX_tree_nodes_TreeRootUuid");

            migrationBuilder.RenameColumn(
                name: "ParentTreeRootGuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentUuid");

            migrationBuilder.RenameColumn(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentTreeRootUuid");

            migrationBuilder.RenameColumn(
                name: "ParentGuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentTreeNodeUuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_leaves_ParentTreeRootGuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "IX_tree_leaves_ParentUuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_leaves_ParentTreeRootUuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentTreeRootUuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_leaves_tree_nodes_ParentUuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentUuid",
                principalSchema: "main_entities",
                principalTable: "tree_nodes",
                principalColumn: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_leaves_tree_roots_ParentTreeRootUuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentTreeRootUuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_roots_TreeRootUuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "TreeRootUuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_leaves_tree_nodes_ParentUuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_tree_leaves_tree_roots_ParentTreeRootUuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_roots_TreeRootUuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_leaves_ParentTreeRootUuid",
                schema: "main_entities",
                table: "tree_leaves");

            migrationBuilder.RenameColumn(
                name: "DataStoragesUuids",
                schema: "main_entities",
                table: "tree_roots",
                newName: "DataStoragesGuids");

            migrationBuilder.RenameColumn(
                name: "TreeRootUuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "TreeRootGuid");

            migrationBuilder.RenameColumn(
                name: "ParentUuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "ParentGuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_nodes_TreeRootUuid",
                schema: "main_entities",
                table: "tree_nodes",
                newName: "IX_tree_nodes_TreeRootGuid");

            migrationBuilder.RenameColumn(
                name: "ParentUuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentTreeRootGuid");

            migrationBuilder.RenameColumn(
                name: "ParentTreeRootUuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentTreeNodeGuid");

            migrationBuilder.RenameColumn(
                name: "ParentTreeNodeUuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "ParentGuid");

            migrationBuilder.RenameIndex(
                name: "IX_tree_leaves_ParentUuid",
                schema: "main_entities",
                table: "tree_leaves",
                newName: "IX_tree_leaves_ParentTreeRootGuid");

            migrationBuilder.CreateIndex(
                name: "IX_tree_leaves_ParentGuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_leaves_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentGuid",
                principalSchema: "main_entities",
                principalTable: "tree_nodes",
                principalColumn: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_leaves_tree_roots_ParentTreeRootGuid",
                schema: "main_entities",
                table: "tree_leaves",
                column: "ParentTreeRootGuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_roots_TreeRootGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "TreeRootGuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");
        }
    }
}
