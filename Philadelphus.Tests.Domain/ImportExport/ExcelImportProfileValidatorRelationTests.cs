using ClosedXML.Excel;
using FluentAssertions;
using Philadelphus.Infrastructure.ImportExport.Excel;

namespace Philadelphus.Tests.Domain.ImportExport;

/// <summary>
/// Проверяет валидацию значений ключей связей Excel-импорта.
/// </summary>
public sealed class ExcelImportProfileValidatorRelationTests
{
    /// <summary>
    /// Повторяющийся ключ родителя не может однозначно определить строку.
    /// </summary>
    [Fact]
    public void ValidateProfiles_DuplicateParentKey_ReturnsError()
    {
        var result = Validate(["P1", "P1"], ["P1"]);

        result.Errors.Should().ContainSingle(error =>
            error.SourceName == "Parents"
            && error.ColumnName == "Id"
            && error.Message.Contains("повторяющиеся значения"));
    }

    /// <summary>
    /// Отдельная дочерняя строка без родителя допустима, если связь в целом работает.
    /// </summary>
    [Fact]
    public void ValidateProfiles_UnmatchedChildAlongsideMatch_DoesNotReturnRelationError()
    {
        var result = Validate(["P1"], ["P1", "Missing"]);

        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Полное отсутствие совпадений указывает на ошибочно выбранные колонки связи.
    /// </summary>
    [Fact]
    public void ValidateProfiles_WithoutAnyMatches_ReturnsError()
    {
        var result = Validate(["P1"], ["Missing"]);

        result.Errors.Should().ContainSingle(error =>
            error.SourceName == "Children"
            && error.ColumnName == "ParentId"
            && error.Message.Contains("не дала ни одного совпадения"));
    }

    /// <summary>
    /// Создаёт временную книгу и выполняет полную проверку двух связанных профилей.
    /// </summary>
    private static ExcelImportValidationResult Validate(
        IReadOnlyCollection<string> parentIds,
        IReadOnlyCollection<string> childParentIds)
    {
        var filePath = CreateWorkbook(parentIds, childParentIds);
        try
        {
            var sourceReader = new ExcelImportSourceReader();
            var validator = new ExcelImportProfileValidator(
                sourceReader,
                new ExcelDataTypeDetector(),
                new ExcelImportInheritanceResolver());
            return validator.ValidateProfiles(filePath, CreateProfiles());
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    /// <summary>
    /// Создаёт профили родительского и дочернего листов с настроенным FK.
    /// </summary>
    private static IReadOnlyList<ExcelImportProfile> CreateProfiles()
    {
        return
        [
            new ExcelImportProfile
            {
                SourceSelection = new()
                {
                    SourceName = "Parents",
                    SourceType = ExcelPreviewSourceType.Worksheet
                },
                EntityKind = ExcelImportEntityKind.Node,
                Columns =
                [
                    CreateColumn(1, "Id"),
                    CreateColumn(2, "Name", ExcelImportColumnRole.SystemName)
                ]
            },
            new ExcelImportProfile
            {
                SourceSelection = new()
                {
                    SourceName = "Children",
                    SourceType = ExcelPreviewSourceType.Worksheet
                },
                Relation = new()
                {
                    ParentSourceName = "Parents",
                    ParentKeyColumnName = "Id",
                    ChildKeyColumnName = "ParentId"
                },
                EntityKind = ExcelImportEntityKind.Leaf,
                Columns =
                [
                    CreateColumn(1, "ParentId"),
                    CreateColumn(2, "Name", ExcelImportColumnRole.SystemName)
                ]
            }
        ];
    }

    /// <summary>
    /// Создаёт описание колонки профиля с поддерживаемым типом данных.
    /// </summary>
    private static ExcelImportColumnProfile CreateColumn(
        int index,
        string name,
        ExcelImportColumnRole role = ExcelImportColumnRole.Attribute) =>
        new()
        {
            ColumnIndex = index,
            HeaderName = name,
            Role = role,
            DataTypeNodeName = "Текст"
        };

    /// <summary>
    /// Создаёт временную Excel-книгу с заданными значениями PK и FK.
    /// </summary>
    private static string CreateWorkbook(
        IEnumerable<string> parentIds,
        IEnumerable<string> childParentIds)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        using var workbook = new XLWorkbook();

        var parents = workbook.AddWorksheet("Parents");
        parents.Cell(1, 1).Value = "Id";
        parents.Cell(1, 2).Value = "Name";
        var parentRow = 2;
        foreach (var parentId in parentIds)
        {
            parents.Cell(parentRow, 1).Value = parentId;
            parents.Cell(parentRow, 2).Value = $"Parent {parentRow - 1}";
            parentRow++;
        }

        var children = workbook.AddWorksheet("Children");
        children.Cell(1, 1).Value = "ParentId";
        children.Cell(1, 2).Value = "Name";
        var childRow = 2;
        foreach (var parentId in childParentIds)
        {
            children.Cell(childRow, 1).Value = parentId;
            children.Cell(childRow, 2).Value = $"Child {childRow - 1}";
            childRow++;
        }

        workbook.SaveAs(filePath);
        return filePath;
    }
}
