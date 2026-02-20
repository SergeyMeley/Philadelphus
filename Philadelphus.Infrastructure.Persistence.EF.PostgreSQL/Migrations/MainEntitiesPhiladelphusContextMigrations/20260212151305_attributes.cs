using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class attributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataStoragesUuids",
                schema: "main_entities",
                table: "tree_roots");

            migrationBuilder.AddColumn<int>(
                name: "override_id",
                schema: "main_entities",
                table: "element_attributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "visibility_id",
                schema: "main_entities",
                table: "element_attributes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "override_id",
                schema: "main_entities",
                table: "element_attributes");

            migrationBuilder.DropColumn(
                name: "visibility_id",
                schema: "main_entities",
                table: "element_attributes");

            migrationBuilder.AddColumn<Guid[]>(
                name: "DataStoragesUuids",
                schema: "main_entities",
                table: "tree_roots",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }
    }
}
