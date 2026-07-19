using ClosedXML.Excel;
using FluentAssertions;
using Philadelphus.Infrastructure.ImportExport.Excel;
using System.Text.Json.Nodes;

namespace Philadelphus.Tests.Domain.ImportExport
{
    public class ExcelImportMaterializationTests
    {
        [Fact]
        public void ProcessSchema_Should_Materialize_Fk_As_Temporary_Parent_Correlation()
        {
            var filePath = CreateWorkbook();

            try
            {
                var sourceReader = new ExcelImportSourceReader();
                var service = new ConversionService(
                    new ExcelDataTypeDetector(),
                    sourceReader,
                    new ExcelImportProfileResolver(new ExcelImportSettingsReader(sourceReader)));
                var schema = CreateSchema(filePath);

                var json = service.ProcessSchema(schema);
                var root = JsonNode.Parse(json)!.AsObject();
                var contentRoot = root["contentRoot"]!.AsObject();
                var rootNodes = contentRoot["childNodes"]!.AsArray();
                var parentNode = rootNodes.Single(x => x!["name"]!.GetValue<string>() == "ParentsNode")!.AsObject();
                var childNode = parentNode["childNodes"]!.AsArray()
                    .Single(x => x!["name"]!.GetValue<string>() == "ChildrenNode")!
                    .AsObject();

                contentRoot["name"]!.GetValue<string>().Should().Be("WorkbookRoot");
                parentNode["childLeaves"]!.AsArray().Select(x => x!["name"]!.GetValue<string>())
                    .Should().BeEquivalentTo("Parent A", "Parent B");
                childNode["childLeaves"]!.AsArray().Select(x => x!["name"]!.GetValue<string>())
                    .Should().BeEquivalentTo("Child 1", "Child 2");

                var child1 = childNode["childLeaves"]!.AsArray()
                    .Single(x => x!["name"]!.GetValue<string>() == "Child 1")!
                    .AsObject();
                var parentA = parentNode["childLeaves"]!.AsArray()
                    .Single(x => x!["name"]!.GetValue<string>() == "Parent A")!
                    .AsObject();
                var parentCorrelationId = parentA["importCorrelationId"]!.GetValue<Guid>();

                child1["importCorrelationId"]!.GetValue<Guid>()
                    .Should().NotBe(Guid.Empty);
                child1["polymorphicParentImportCorrelationId"]!.GetValue<Guid>()
                    .Should().Be(parentCorrelationId);
                childNode["attributes"]!.AsArray()
                    .Should().NotContain(x => x!["name"]!.GetValue<string>() == "ParentId");
                child1["attributes"]!.AsArray()
                    .Should().NotContain(x => x!["name"]!.GetValue<string>() == "ParentId");

                json.Should().NotContain("\"isCollectionValue\"");
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        private static ExcelImportSchema CreateSchema(string filePath)
        {
            return new ExcelImportSchema
            {
                SourceFilePath = filePath,
                RootName = "WorkbookRoot",
                Sheets =
                {
                    new ExcelImportSheetSchema
                    {
                        SourceName = "Parents",
                        SourceType = ExcelPreviewSourceType.Worksheet,
                        DisplayName = "ParentsNode",
                        EntityKind = ExcelImportEntityKind.Leaf,
                        Profile = new ExcelImportProfile
                        {
                            SourceSelection = new ExcelImportSourceSelection
                            {
                                SourceName = "Parents",
                                SourceType = ExcelPreviewSourceType.Worksheet
                            },
                            EntityKind = ExcelImportEntityKind.Leaf,
                            DataStartRowOffset = 1,
                            Columns =
                            {
                                new ExcelImportColumnProfile { ColumnIndex = 1, HeaderName = "Id", Role = ExcelImportColumnRole.Attribute },
                                new ExcelImportColumnProfile { ColumnIndex = 2, HeaderName = "Name", Role = ExcelImportColumnRole.SystemName },
                                new ExcelImportColumnProfile { ColumnIndex = 3, HeaderName = "Comment", Role = ExcelImportColumnRole.Attribute }
                            }
                        }
                    },
                    new ExcelImportSheetSchema
                    {
                        SourceName = "Children",
                        SourceType = ExcelPreviewSourceType.Worksheet,
                        DisplayName = "ChildrenNode",
                        EntityKind = ExcelImportEntityKind.Root,
                        Profile = new ExcelImportProfile
                        {
                            SourceSelection = new ExcelImportSourceSelection
                            {
                                SourceName = "Children",
                                SourceType = ExcelPreviewSourceType.Worksheet
                            },
                            EntityKind = ExcelImportEntityKind.Root,
                            Relation = new ExcelImportRelationProfile
                            {
                                ParentSourceName = "Parents",
                                ParentKeyColumnName = "Id",
                                ChildKeyColumnName = "ParentId"
                            },
                            DataStartRowOffset = 1,
                            Columns =
                            {
                                new ExcelImportColumnProfile { ColumnIndex = 1, HeaderName = "Id", Role = ExcelImportColumnRole.Attribute },
                                new ExcelImportColumnProfile { ColumnIndex = 2, HeaderName = "ParentId", Role = ExcelImportColumnRole.Attribute },
                                new ExcelImportColumnProfile { ColumnIndex = 3, HeaderName = "Name", Role = ExcelImportColumnRole.SystemName },
                                new ExcelImportColumnProfile { ColumnIndex = 4, HeaderName = "Value", Role = ExcelImportColumnRole.Attribute }
                            }
                        }
                    }
                }
            };
        }

        private static string CreateWorkbook()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");

            using var workbook = new XLWorkbook();
            var parents = workbook.AddWorksheet("Parents");
            parents.Cell(1, 1).Value = "Id";
            parents.Cell(1, 2).Value = "Name";
            parents.Cell(1, 3).Value = "Comment";
            parents.Cell(2, 1).Value = "P1";
            parents.Cell(2, 2).Value = "Parent A";
            parents.Cell(2, 3).Value = "Root row A";
            parents.Cell(3, 1).Value = "P2";
            parents.Cell(3, 2).Value = "Parent B";
            parents.Cell(3, 3).Value = "Root row B";

            var children = workbook.AddWorksheet("Children");
            children.Cell(1, 1).Value = "Id";
            children.Cell(1, 2).Value = "ParentId";
            children.Cell(1, 3).Value = "Name";
            children.Cell(1, 4).Value = "Value";
            children.Cell(2, 1).Value = "C1";
            children.Cell(2, 2).Value = "P1";
            children.Cell(2, 3).Value = "Child 1";
            children.Cell(2, 4).Value = "100";
            children.Cell(3, 1).Value = "C2";
            children.Cell(3, 2).Value = "P2";
            children.Cell(3, 3).Value = "Child 2";
            children.Cell(3, 4).Value = "200";

            workbook.SaveAs(filePath);
            return filePath;
        }
    }
}
