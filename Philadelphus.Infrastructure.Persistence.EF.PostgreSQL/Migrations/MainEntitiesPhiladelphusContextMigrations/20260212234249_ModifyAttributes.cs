using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class ModifyAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "declaring_owner_uuid",
                schema: "main_entities",
                table: "element_attributes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "declaring_uuid",
                schema: "main_entities",
                table: "element_attributes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "declaring_owner_uuid",
                schema: "main_entities",
                table: "element_attributes");

            migrationBuilder.DropColumn(
                name: "declaring_uuid",
                schema: "main_entities",
                table: "element_attributes");
        }
    }
}
