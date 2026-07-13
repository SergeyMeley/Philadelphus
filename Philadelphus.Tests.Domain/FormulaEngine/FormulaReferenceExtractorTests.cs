using FluentAssertions;
using Philadelphus.Core.Domain.Relations;

namespace Philadelphus.Tests.Domain.FormulaEngine;

public sealed class FormulaReferenceExtractorTests
{
    [Fact]
    public void GetTreeLeaveUuids_TraversesNestedAstAndDeduplicates()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var result = FormulaReferenceExtractor.GetTreeLeaveUuids(
            $"=[{first}]=[{second}]?[{first}]:[{second}]");

        result.Should().BeEquivalentTo(new[] { first, second });
    }

    [Fact]
    public void GetTreeLeaveUuids_DoesNotTreatStringLiteralAsReference()
    {
        var uuid = Guid.NewGuid();

        FormulaReferenceExtractor.GetTreeLeaveUuids($"=\"[{uuid}]\"").Should().BeEmpty();
    }
}
