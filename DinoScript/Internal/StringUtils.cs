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

using System.Text;

namespace DinoScript.Internal;

public static class StringUtils
{
    public static string Escape(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var builder = ObjectPools.StringBuilderPool.Get();
        try
        {
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\t':
                        builder.Append(@"\t");
                        break;
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    case '"':
                        builder.Append(@"\""");
                        break;
                    case '\0':
                        builder.Append(@"\0");
                        break;
                    default:
                        // 스페이스(공백)는 그대로 유지
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
        finally
        {
            ObjectPools.StringBuilderPool.Return(builder);
        }
    }
}