using System;

namespace DinoScript.Runtime
{
    public readonly struct DinoValue
    {
        public DinoValue(DinoType type, long value)
        {
            this.type = type;
            this.value = value;
        }

        private readonly DinoType type;
        private readonly long value;

        public DinoType Type => type;

        public double Double => BitConverter.Int64BitsToDouble(value);

        public long Int64 => value;

        public ulong UInt64 => unchecked((ulong)value);

        public static DinoValue Number(double value) => new DinoValue(DinoType.Number, BitConverter.DoubleToInt64Bits(value));
    }
}