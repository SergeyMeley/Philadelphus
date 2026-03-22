using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Migrations.MainEntitiesPhiladelphusContextMigrations
{
    /// <inheritdoc />
    public partial class audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "shrub_members",
                table: "tree_roots",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "session_user");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "shrub_members",
                table: "tree_roots",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "session_user");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "shrub_members",
                table: "tree_roots",
                type: "text",
                nullable: false,
                defaultValue: "session_user",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "shrub_members",
                table: "tree_roots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "text",
                nullable: false,
                defaultValue: "session_user",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "shrub_members",
                table: "tree_leaves",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
