namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Типы лексических токенов формулы.
    /// </summary>
    public enum FormulaTokenKind
    {
        End,
        FormulaStart,
        Number,
        String,
        Identifier,
        TreeLeaveReference,
        OpenParenthesis,
        CloseParenthesis,
        Dot,
        Semicolon,
        Plus,
        Minus,
        Star,
        Slash,
        Caret,
        Ampersand,
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual,
        Question,
        Colon
    }
}
