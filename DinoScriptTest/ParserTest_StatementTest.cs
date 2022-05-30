using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using Xunit;

namespace DinoScript.Test;

using static InternalCode;

public partial class ParserTest
{
    [Theory]
    [MemberData(nameof(StatementTestDataList))]
    public void StatementTest(string text, InternalCode[] expectedCodes, string? exceptionMessage = null)
    {
        CommonTest(text, expectedCodes, ParserMode.StatementTest, exceptionMessage);
    }

    public static IEnumerable<object[]> StatementTestDataList => new[]
    {
        new object[]
        {
            "let a = 10\n" +
            "a = 20",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToLocal, null, 0),
            }
        },
        new object[]
        {
            "let a = 10; a = 20;",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToLocal, null, 0),
            }
        },
        new object[]
        {
            "let a = 10\n" +
            "let b = 20\n" +
            "a = b",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadFromLocal, null, 1),
                Make(Opcode.StoreToLocal, null, 0)
            }
        },
    };
}