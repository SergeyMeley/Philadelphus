using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.ShrubMembersContextMigrations
{
    /// <inheritdoc />
    public partial class add_formilas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "value_formula",
                schema: "shrub_members_content",
                table: "element_attributes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "value_formula",
                schema: "shrub_members_content",
                table: "element_attributes");
        }
    }
}
