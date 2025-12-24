using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentTreeNodeGuid");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tree_nodes_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropIndex(
                name: "IX_tree_nodes_ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.DropColumn(
                name: "ParentTreeNodeGuid",
                schema: "main_entities",
                table: "tree_nodes");

            migrationBuilder.AddForeignKey(
                name: "FK_tree_nodes_tree_nodes_ParentGuid",
                schema: "main_entities",
                table: "tree_nodes",
                column: "ParentGuid",
                principalSchema: "main_entities",
                principalTable: "tree_nodes",
                principalColumn: "uuid");
        }
    }
}
