using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Philadelphus.PostgreEfRepository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    ValueType = table.Column<string>(type: "text", nullable: false),
                    ValueIds = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.repository_element_id);
                });

            migrationBuilder.CreateTable(
                name: "AttributeValues",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeValues", x => x.repository_element_id);
                });

            migrationBuilder.CreateTable(
                name: "audit_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedOn = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedContentOn = table.Column<string>(type: "text", nullable: false),
                    UpdatedContentBy = table.Column<string>(type: "text", nullable: false),
                    DeletedOn = table.Column<string>(type: "text", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_info_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "repository_elements",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentGuid = table.Column<string>(type: "text", nullable: false),
                    DirectoryPath = table.Column<string>(type: "text", nullable: false),
                    DirectoryFullPath = table.Column<string>(type: "text", nullable: false),
                    ConfigPath = table.Column<string>(type: "text", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    HasContent = table.Column<bool>(type: "boolean", nullable: false),
                    IsOriginal = table.Column<bool>(type: "boolean", nullable: false),
                    IsLegacy = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repository_elements", x => x.repository_element_id);
                    table.ForeignKey(
                        name: "FK_repository_elements_audit_info_AuditInfoId",
                        column: x => x.AuditInfoId,
                        principalTable: "audit_info",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leave_details",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    ParentTreeNodeGuid = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AttributeGuids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_details", x => x.repository_element_id);
                    table.ForeignKey(
                        name: "FK_leave_details_repository_elements_repository_element_id",
                        column: x => x.repository_element_id,
                        principalTable: "repository_elements",
                        principalColumn: "repository_element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "node_details",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    ParentTreeRootGuid = table.Column<long>(type: "bigint", nullable: false),
                    ParentTreeNodeGuid = table.Column<long>(type: "bigint", nullable: false),
                    AttributeGuids = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    ChildTreeNodeGuids = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    ChildTreeLeaveGuids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_node_details", x => x.repository_element_id);
                    table.ForeignKey(
                        name: "FK_node_details_repository_elements_repository_element_id",
                        column: x => x.repository_element_id,
                        principalTable: "repository_elements",
                        principalColumn: "repository_element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repositories",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    ChildTreeRootGuids = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repositories", x => x.repository_element_id);
                    table.ForeignKey(
                        name: "FK_repositories_repository_elements_repository_element_id",
                        column: x => x.repository_element_id,
                        principalTable: "repository_elements",
                        principalColumn: "repository_element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "root_details",
                columns: table => new
                {
                    repository_element_id = table.Column<int>(type: "integer", nullable: false),
                    ParentTreeRepositoryGuid = table.Column<long>(type: "bigint", nullable: false),
                    AttributeGuids = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    ChildTreeNodeGuids = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_root_details", x => x.repository_element_id);
                    table.ForeignKey(
                        name: "FK_root_details_repository_elements_repository_element_id",
                        column: x => x.repository_element_id,
                        principalTable: "repository_elements",
                        principalColumn: "repository_element_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_repository_elements_AuditInfoId",
                table: "repository_elements",
                column: "AuditInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "AttributeValues");

            migrationBuilder.DropTable(
                name: "leave_details");

            migrationBuilder.DropTable(
                name: "node_details");

            migrationBuilder.DropTable(
                name: "repositories");

            migrationBuilder.DropTable(
                name: "root_details");

            migrationBuilder.DropTable(
                name: "repository_elements");

            migrationBuilder.DropTable(
                name: "audit_info");
        }
    }
}
