using FluentAssertions;
using Philadelphus.Presentation.Behaviors.Logic;

namespace Philadelphus.Tests.Presentation.Behaviors.Logic
{
    public class FormulaOperandFinderTests
    {
        [Fact]
        public void TryFindOperandAtCaret_Returns_Token_At_Caret()
        {
            var found = FormulaOperandFinder.TryFindOperandAtCaret("abc", 1, out var start, out var length);

            found.Should().BeTrue();
            start.Should().Be(0);
            length.Should().Be(3);
        }

        [Fact]
        public void TryFindOperandAtCaret_Empty_Text_Returns_False()
            => FormulaOperandFinder.TryFindOperandAtCaret(string.Empty, 0, out _, out _).Should().BeFalse();

        [Fact]
        public void TryFindAttributeReferenceAtCaret_Returns_Whole_Call()
        {
            const string text = "АТРИБУТ(x)";

            var found = FormulaOperandFinder.TryFindAttributeReferenceAtCaret(text, 3, out var start, out var length);

            found.Should().BeTrue();
            start.Should().Be(0);
            length.Should().Be(text.Length);
        }

        [Fact]
        public void TryFindAttributeReferenceAtCaret_No_Attribute_Returns_False()
            => FormulaOperandFinder.TryFindAttributeReferenceAtCaret("abc", 1, out _, out _).Should().BeFalse();
    }
}
