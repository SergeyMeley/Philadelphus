using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.SQLite.Migrations.PhiladelphusRepositoriesContextMigrations
{
    /// <inheritdoc />
    public partial class repo_storages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "available_data_storage_uuids",
                schema: "repositories",
                table: "philadelphus_repositories",
                type: "uuid[]",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "default_data_storage_uuids",
                schema: "repositories",
                table: "philadelphus_repositories",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "available_data_storage_uuids",
                schema: "repositories",
                table: "philadelphus_repositories");

            migrationBuilder.DropColumn(
                name: "default_data_storage_uuids",
                schema: "repositories",
                table: "philadelphus_repositories");
        }
    }
}
