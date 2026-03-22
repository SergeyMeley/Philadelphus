using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class AddAttributesOwningTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_element_attributes_owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes",
                column: "owning_working_tree_uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_element_attributes_working_trees_owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes",
                column: "owning_working_tree_uuid",
                principalSchema: "shrub_members",
                principalTable: "working_trees",
                principalColumn: "uuid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_element_attributes_working_trees_owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes");

            migrationBuilder.DropIndex(
                name: "IX_element_attributes_owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes");

            migrationBuilder.DropColumn(
                name: "owning_working_tree_uuid",
                schema: "shrub_members_content",
                table: "element_attributes");
        }
    }
}
