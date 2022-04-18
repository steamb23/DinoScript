namespace DinoScript.Syntax;

/// <summary>
/// 토큰의 타입을 열거합니다.
/// </summary>
public enum TokenType
{
    Error = -1,
    /// <summary>
    /// 어느 토큰 타입에도 속하지 않는 토큰의 타입입니다.
    /// </summary>
    UnexpectedToken = 0,
    WhiteSpace,
    EndOfLine,
    Identifier,
    Keyword,
    Operator,
    Mark,
    BooleanLiteral,
    NumberLiteral,
    CharacterLiteral,
    StringLiteral,
    NullLiteral,
    UndefinedLiteral,
}