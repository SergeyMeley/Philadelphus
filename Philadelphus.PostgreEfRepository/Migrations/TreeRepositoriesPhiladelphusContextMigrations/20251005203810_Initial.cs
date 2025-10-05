using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations.TreeRepositoriesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "repositories");

            migrationBuilder.CreateTable(
                name: "tree_repositories",
                schema: "repositories",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_updated_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    content_updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deleted_at = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_storage_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    data_storages_uuids = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    child_tree_roots_uuids = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tree_repositories_pkey", x => x.uuid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tree_repositories",
                schema: "repositories");
        }
    }
}
