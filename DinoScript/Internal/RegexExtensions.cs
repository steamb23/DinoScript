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

using System.Text.RegularExpressions;

namespace DinoScript.Internal;

/// <summary>
/// RegexExtensions 클래스는 Regex 관련 기능을 확장하는 정적 메서드를 제공합니다.
/// 이 클래스는 내부적으로 사용되며, 정규 표현식을 활용한 문자열 매칭 작업을 수행합니다.
/// </summary>
internal static class RegexExtensions
{
    /// <summary>
    /// Regex에서 첫 번째 일치하는 ValueMatch를 찾습니다.
    /// </summary>
    /// <param name="regex">정규식 객체</param>
    /// <param name="input">검색할 문자열</param>
    /// <param name="match">찾은 매치 결과</param>
    /// <returns>매치가 발견되면 true, 아니면 false</returns>
    public static bool FirstValueMatch(this Regex regex, ReadOnlySpan<char> input, out ValueMatch match)
    {
        var enumerator = regex.EnumerateMatches(input);
        if (enumerator.MoveNext())
        {
            match = enumerator.Current;
            return true;
        }

        match = default;
        return false;
    }
}