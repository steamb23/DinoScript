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

        public static DinoValue Integer(long value) => new DinoValue(DinoType.Integer, value);

        public static DinoValue Boolean(bool value) => new DinoValue(DinoType.Boolean, value ? 1 : 0);

        public static DinoValue Boolean(long value) => new DinoValue(DinoType.Boolean, value);

        public static implicit operator DinoValue(double value) => Number(value);

        public static implicit operator DinoValue(long value) => Integer(value);

        public static implicit operator DinoValue(bool value) => Boolean(value);

        public static explicit operator double(DinoValue value) => value.Double;

        public static explicit operator long(DinoValue value) => value.Int64;

        public static explicit operator bool(DinoValue value) => value.Int64 != 0;
    }
}