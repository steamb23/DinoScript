using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace DinoScript.Syntax;

public class TokenDefinition
{
    /// <summary>
    /// 정규표현식을 가져옵니다.
    /// </summary>
    public Regex Regex { get; }

    /// <summary>
    /// 토큰의 타입을 가져옵니다.
    /// </summary>
    public TokenType Type { get; }

    public TokenDefinition(TokenType tokenType,[RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.Compiled| RegexOptions.Singleline)
    {
        Type = tokenType;

        Regex = new Regex(pattern, regexOptions);
    }
}