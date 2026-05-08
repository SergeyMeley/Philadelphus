using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Migrations.ShrubMembersContextMigrations
{
    /// <inheritdoc />
    public partial class AddTreeNodeParentForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_parent_tree_node_uuid",
                schema: "shrub_members",
                table: "tree_nodes",
                column: "parent_tree_node_uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_nodes_parent_tree_node_uuid",
                schema: "shrub_members",
                table: "tree_nodes",
                column: "parent_tree_node_uuid",
                principalSchema: "shrub_members",
                principalTable: "tree_nodes",
                principalColumn: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_nodes_parent_tree_node_uuid",
                schema: "shrub_members",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_nodes_parent_tree_node_uuid",
                schema: "shrub_members",
                table: "tree_nodes");
        }
    }
}
