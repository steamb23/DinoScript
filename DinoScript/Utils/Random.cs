using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DinoScript.Utils
{
    internal static class Random
    {
        /*
         * 이 정적 함수들은 내부 메모리 관리에만 관여됩니다.
         */
#if NET6_0_OR_GREATER
        public static System.Random Shared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.Random.Shared;
        }
#else
        public static System.Random Shared { get; } = new System.Random();
#endif

        public static ulong NextUInt64(this System.Random @this)
        {
            Span<byte> resultBytes = stackalloc byte[8];
            @this.NextBytes(resultBytes);
            return BitConverter.ToUInt64(resultBytes);
        }
    }
}