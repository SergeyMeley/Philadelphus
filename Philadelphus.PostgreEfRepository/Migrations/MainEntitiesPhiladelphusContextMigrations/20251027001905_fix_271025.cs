using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class fix_271025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_roots_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_parent_tree_root_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "parent_tree_root_uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_roots_parent_tree_root_uuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "parent_tree_root_uuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_roots_parent_tree_root_uuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_nodes_parent_tree_root_uuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_roots_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentGuid",
                principalSchema: "main_entities",
                principalTable: "tree_roots",
                principalColumn: "uuid");
        }
    }
}
