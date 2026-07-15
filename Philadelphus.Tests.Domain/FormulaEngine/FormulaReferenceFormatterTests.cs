using FluentAssertions;

using Philadelphus.Core.Domain.FormulaEngine.Formatting;

namespace Philadelphus.Tests.Domain.FormulaEngine;

public class FormulaReferenceFormatterTests
{
    private static readonly Guid TreeLeaveUuid = Guid.Parse("019f6c74-6a97-7ea8-a24c-24f284b636c9");

    [Fact]
    public void CreateTreeLeaveReference_ReturnsExpression()
    {
        FormulaReferenceFormatter.CreateTreeLeaveReference(TreeLeaveUuid)
            .Should().Be($"[{TreeLeaveUuid}]");
    }

    [Fact]
    public void CreateTreeLeaveReferenceFormula_ReturnsFormula()
    {
        FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(TreeLeaveUuid)
            .Should().Be($"=[{TreeLeaveUuid}]");
    }

    [Fact]
    public void CreateRelativeAttributeReference_EscapesQuotes()
    {
        FormulaReferenceFormatter.CreateRelativeAttributeReference("Цвет \"основной\"")
            .Should().Be("АТРИБУТ(\"Цвет \"\"основной\"\"\")");
    }
}
