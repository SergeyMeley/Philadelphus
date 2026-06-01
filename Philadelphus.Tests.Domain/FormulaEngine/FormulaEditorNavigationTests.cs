using FluentAssertions;
using Philadelphus.Core.Domain.FormulaEngine.Editing;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    public class FormulaEditorNavigationTests
    {
        [Theory]
        [InlineData("=СУММ(1;2)", 5, 10)]
        [InlineData("=СУММ(1;2)", 10, 5)]
        [InlineData("=СУММ(ПРОИЗВ(2;3);4)", 12, 17)]
        [InlineData("=СУММ(ПРОИЗВ(2;3);4)", 16, 12)]
        public void TryGetMatchingParenthesisCaretIndex_Returns_Matching_Parenthesis(
            string source,
            int caretIndex,
            int expectedCaretIndex)
        {
            var result = FormulaEditorNavigation.TryGetMatchingParenthesisCaretIndex(
                source,
                caretIndex,
                out var matchingCaretIndex);

            result.Should().BeTrue();
            matchingCaretIndex.Should().Be(expectedCaretIndex);
        }

        [Theory]
        [InlineData("СУММ(1;2)", 5)]
        [InlineData("=СУММ(1;2", 5)]
        [InlineData("=СУММ(1;2)", 2)]
        public void TryGetMatchingParenthesisCaretIndex_Returns_False_When_No_Matching_Parenthesis(
            string source,
            int caretIndex)
        {
            var result = FormulaEditorNavigation.TryGetMatchingParenthesisCaretIndex(
                source,
                caretIndex,
                out _);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("=СУММ(1;2)", 7, 1, 9)]
        [InlineData("=СУММ(ПРОИЗВ(2;3);4)", 13, 6, 11)]
        [InlineData("=ЕСЛИ(1=1;СУММ(2;3);0)", 17, 10, 9)]
        [InlineData("=[9769db19-89a2-49ed-a3fd-f1e539a652b1].СВОЙСТВО(Name)", 50, 40, 14)]
        public void TryGetCurrentFormulaCallSelection_Returns_Current_Function_Call(
            string source,
            int caretIndex,
            int expectedStart,
            int expectedLength)
        {
            var result = FormulaEditorNavigation.TryGetCurrentFormulaCallSelection(
                source,
                caretIndex,
                out var selection);

            result.Should().BeTrue();
            selection.Start.Should().Be(expectedStart);
            selection.Length.Should().Be(expectedLength);
        }

        [Theory]
        [InlineData("СУММ(1;2)", 7)]
        [InlineData("=1+2", 2)]
        [InlineData("=СУММ(1;2)", 11)]
        public void TryGetCurrentFormulaCallSelection_Returns_False_When_Cursor_Is_Outside_Call(
            string source,
            int caretIndex)
        {
            var result = FormulaEditorNavigation.TryGetCurrentFormulaCallSelection(
                source,
                caretIndex,
                out _);

            result.Should().BeFalse();
        }
    }
}
