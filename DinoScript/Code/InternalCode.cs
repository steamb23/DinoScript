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

namespace DinoScript.Code;

public class InternalCode
{
    /*
     * 0x00 (1 byte) Opcode
     */
    Memory<byte> Code { get; }

    public Opcode Opcode => Code.Length > 0 ? (Opcode)Code.Span[0] : Opcode.Error;

    public Memory<byte> OperandBytes => Code[1..];

    public long GetOperandLong()
    {
        var operands = OperandBytes.Span;
        if (operands.Length < 8)
            throw new InvalidOperationException("Invalid operand size.");
        return BitConverter.ToInt64(operands);
    }

    public double GetOperandDouble()
    {
        return BitConverter.Int64BitsToDouble(GetOperandLong());
    }

    public bool GetOperandBool()
    {
        var operands = OperandBytes.Span;
        if (operands.Length < 1)
            throw new InvalidOperationException("Invalid operand size.");
        return operands[0] != 0;
    }

    public int GetOperandAddress()
    {
        var operands = OperandBytes.Span;
        if (operands.Length < 4)
            throw new InvalidOperationException("Invalid operand size.");
        return BitConverter.ToInt32(operands);
    }
}