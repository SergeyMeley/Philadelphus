using FluentAssertions;

using Philadelphus.Core.Domain.FormulaEngine.Formatting;

namespace Philadelphus.Tests.Domain.FormulaEngine;

public class FormulaReferenceParserTests
{
    private static readonly Guid TreeLeaveUuid = Guid.Parse("019f6c74-6a97-7ea8-a24c-24f284b636c9");

    [Fact]
    public void TryParseTreeLeaveReference_ValidReference_ReturnsUuid()
    {
        FormulaReferenceParser.TryParseTreeLeaveReference($"[{TreeLeaveUuid}]", out var uuid)
            .Should().BeTrue();
        uuid.Should().Be(TreeLeaveUuid);
    }

    [Theory]
    [InlineData("019f6c74-6a97-7ea8-a24c-24f284b636c9")]
    [InlineData("[not-a-uuid]")]
    [InlineData("=[019f6c74-6a97-7ea8-a24c-24f284b636c9]")]
    public void TryParseTreeLeaveReference_InvalidReference_ReturnsFalse(string text)
    {
        FormulaReferenceParser.TryParseTreeLeaveReference(text, out var uuid)
            .Should().BeFalse();
        uuid.Should().Be(Guid.Empty);
    }
}
