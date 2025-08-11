#region License

// Copyright (c) 2025 Choi Jiheon (steamb23@outlook.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

namespace DinoScript.Syntax;

/// <summary>
/// 토큰을 나타내는 구조체입니다.
/// </summary>
/// <param name="Type">토큰의 유형입니다.</param>
/// <param name="RawText">토큰의 원시 텍스트입니다.</param>
/// <param name="RedefinedText">토큰의 재정의된 텍스트입니다.</param>
/// <param name="Lines">토큰이 위치한 줄 번호입니다.</param>
/// <param name="Columns">토큰이 위치한 열 번호입니다.</param>
public readonly record struct Token(
    TokenType Type,
    ReadOnlyMemory<char> RawText,
    string? RedefinedText,
    long Lines,
    long Columns)
{
    /// <summary>
    /// 토큰의 유형입니다.
    /// </summary>
    public TokenType Type { get; } = Type;
    /// <summary>
    /// 토큰의 원시 텍스트입니다.
    /// </summary>
    public ReadOnlyMemory<char> RawText { get; } = RawText;
    /// <summary>
    /// 토큰의 재정의된 텍스트입니다.
    /// </summary>
    public string? RedefinedText { get; } = RedefinedText;
    /// <summary>
    /// 토큰이 위치한 줄 번호입니다.
    /// </summary>
    public long Lines { get; } = Lines;
    /// <summary>
    /// 토큰이 위치한 열 번호입니다.
    /// </summary>
    public long Columns { get; } = Columns;
    /// <summary>
    /// 토큰이 나타내는 실제 값입니다. RedefinedText가 null인 경우 RawText를 반환합니다.
    /// </summary>
    public ReadOnlyMemory<char> Value => RedefinedText?.AsMemory() ?? RawText;

    /// <summary>
    /// 토큰이 유효한지 확인합니다.
    /// </summary>
    /// <returns>토큰이 오류나 예상치 못한 토큰이 아닌 경우 true를 반환합니다.</returns>
    public bool IsValid()
    {
        return Type is not TokenType.Error and not TokenType.UnexpectedToken;
    }
}