using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.ShrubMembersContextMigrations
{
    /// <inheritdoc />
    [DbContext(typeof(PostgreEfShrubMembersContext))]
    [Migration("20260522090000_AddTreeLeaveStringValue")]
    public partial class AddTreeLeaveStringValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "string_value",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "text",
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
