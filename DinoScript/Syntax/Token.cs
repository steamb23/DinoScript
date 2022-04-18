using DinoScript.Parser;

namespace DinoScript.Syntax;

/// <summary>
/// 토큰을 나타냅니다.
/// </summary>
public class Token
{
    /// <summary>
    /// 토큰의 타입을 가져오거나 초기화합니다.
    /// </summary>
    public TokenType Type { get; init; }

    /// <summary>
    /// 토큰의 값을 가져오거나 초기화합니다.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// 가공되지 않은 토큰의 원본 값을 가져오거나 초기화합니다.
    /// </summary>
    public string Text { get; init; } = "";

    /// <summary>
    /// 현재 토큰의 열 위치를 가져옵니다.
    /// </summary>
    public long Columns { get; init; }

    /// <summary>
    /// 현재 토큰의 줄 위치를 가져옵니다.
    /// </summary>
    public long Lines { get; init; }
}