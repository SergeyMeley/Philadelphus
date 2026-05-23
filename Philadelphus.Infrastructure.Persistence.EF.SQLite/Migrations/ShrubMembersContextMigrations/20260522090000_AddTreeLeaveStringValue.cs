using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Migrations.ShrubMembersContextMigrations
{
    /// <inheritdoc />
    public partial class AddTreeLeaveStringValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "string_value",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "TEXT",
                nullable: false,
                defaultValue: "<empty>");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "string_value",
                schema: "shrub_members",
                table: "tree_leaves");
        }
    }
}
