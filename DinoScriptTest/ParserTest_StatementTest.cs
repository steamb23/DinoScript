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
        new object[]
        {
            "let isRun = true\n" +
            "if (isRun)\n" +
            "    let a = 10",
            new[]
            {
                Make(Opcode.LoadConstantBoolean, null, (long)1),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.BranchIfFalse, null, 6),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
            }
        },
        new object[]
        {
            "let isRun = true\n" +
            "if (isRun)\n" +
            "    let a = 10\n" +
            "else\n" +
            "    let b = 20\n",
            new[]
            {
                Make(Opcode.LoadConstantBoolean, null, (long)1),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.BranchIfFalse, null, 7),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.Branch, null, 9),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToNewLocal, null),
            }
        },
        new object[]
        {
            "let a = 10\n" +
            "let b = 0\n" +
            "if (a == 10)\n" +
            "    b = 10\n" +
            "else if (a == 20)\n" +
            "    b = 20\n" +
            "else\n" +
            "    b = 30\n",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadConstantInteger, null, (long)0),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.Equal, null),
                Make(Opcode.BranchIfFalse, null, 11),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToLocal, null, 1),
                Make(Opcode.Branch, null, 20),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.Equal, null),
                Make(Opcode.BranchIfFalse, null, 18),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToLocal, null, 1),
                Make(Opcode.Branch, null, 20),
                Make(Opcode.LoadConstantInteger, null, (long)30),
                Make(Opcode.StoreToLocal, null, 1),
            }
        },
        // TODO: 변수 스코프 관련 기능 구현 필요
        new object[]
        {
            "let a = 10\n" +
            "if (a == 10)\n" +
            "    let b = 10\n" +
            "else if (a == 20)\n" +
            "    let b = 20\n" +
            "else\n" +
            "    let b = 30\n",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.Equal, null),
                Make(Opcode.BranchIfFalse, null, 11),
                Make(Opcode.LoadConstantInteger, null, (long)10),
                Make(Opcode.StoreToNewLocal, null, 1),
                Make(Opcode.Branch, null, 20),
                Make(Opcode.LoadFromLocal, null, 0),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.Equal, null),
                Make(Opcode.BranchIfFalse, null, 18),
                Make(Opcode.LoadConstantInteger, null, (long)20),
                Make(Opcode.StoreToLocal, null, 1),
                Make(Opcode.Branch, null, 20),
                Make(Opcode.LoadConstantInteger, null, (long)30),
                Make(Opcode.StoreToLocal, null, 1),
            }
        }
    };
}