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
    public void CreateCollectionFormulaFromScalarFormula_WrapsExpression()
    {
        FormulaReferenceFormatter.CreateCollectionFormulaFromScalarFormula("=ФУНКЦИЯ(1; 2)")
            .Should().Be("={ФУНКЦИЯ(1; 2)}");
    }

    [Fact]
    public void TryCreateScalarFormulaFromCollectionFormula_UnwrapsSingleExpression()
    {
        FormulaReferenceFormatter.TryCreateScalarFormulaFromCollectionFormula(
                $"={{ЛИСТ(0;\"{TreeLeaveUuid}\")}}",
                out var scalarFormula)
            .Should().BeTrue();
        scalarFormula.Should().Be($"=ЛИСТ(0;\"{TreeLeaveUuid}\")");
    }

    [Fact]
    public void TryCreateScalarFormulaFromCollectionFormula_RejectsMultipleExpressions()
    {
        FormulaReferenceFormatter.TryCreateScalarFormulaFromCollectionFormula(
                "={1+1;2+2}",
                out _)
            .Should().BeFalse();
    }

    [Fact]
    public void CreateRelativeAttributeReference_EscapesQuotes()
    {
        FormulaReferenceFormatter.CreateRelativeAttributeReference("Цвет \"основной\"")
            .Should().Be("АТРИБУТ(\"Цвет \"\"основной\"\"\")");
    }
}
