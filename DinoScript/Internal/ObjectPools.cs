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
using Microsoft.Extensions.ObjectPool;

namespace DinoScript.Internal;

/// <summary>
/// 객체 풀을 관리하는 static 클래스입니다.
/// </summary>
/// <remarks>
/// 주로 재사용 가능한 객체를 효율적으로 관리하기 위해 객체 풀링을 제공합니다.
/// 이 클래스에서는 StringBuilder 객체를 재사용하기 위한 풀을 제공합니다.
/// </remarks>
internal static class ObjectPools
{
    /// <summary>
    /// StringBuilder 객체를 풀링하여 효율적으로 재사용하기 위한 풀입니다.
    /// </summary>
    /// <remarks>
    /// 다수의 문자열 결합 작업에서 StringBuilder의 인스턴스 생성 및 GC 부담을 줄이기 위해 사용됩니다.
    /// 생성된 StringBuilder 인스턴스는 작업 후 반드시 반환되어야 하며, 반환되지 않으면 리소스 누수가 발생할 수 있습니다.
    /// </remarks>
    public static readonly ObjectPool<StringBuilder> StringBuilderPool =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
}