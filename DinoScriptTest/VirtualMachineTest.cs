using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using DinoScript.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace DinoScript.Test;

public class VirtualMachineTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public VirtualMachineTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public static IEnumerable<object[]> ExpressionTestDataList => new[]
    {
        new object[]
        {
            "1 + 2 * 3 + 4",
            DinoValue.Integer(1 + 2 * 3 + 4)
        },
        new object[]
        {
            "10*0+10",
            DinoValue.Integer(10 * 0 + 10)
        },
        new object[]
        {
            "1.0 / 3.0",
            DinoValue.Number(1.0 / 3.0)
        },
        new object[]
        {
            "1.0 % 3.0",
            DinoValue.Number(1.0 % 3.0)
        },
        new object[]
        {
            "4.0 % 3.0",
            DinoValue.Number(4.0 % 3.0)
        },
        new object[]
        {
            "4.5 % 3.1",
            DinoValue.Number(4.5 % 3.1)
        },
        new object[]
        {
            "-1.0",
            DinoValue.Number(-1.0)
        },
        new object[]
        {
            "1 == 1",
            DinoValue.Boolean(true)
        },
        new object[]
        {
            "1 == 2",
            DinoValue.Boolean(false)
        },
        new object[]
        {
            "1 != 1",
            DinoValue.Boolean(false)
        },
        new object[]
        {
            "1 != 2",
            DinoValue.Boolean(true)
        }
    };

    [Theory]
    [MemberData(nameof(ExpressionTestDataList))]
    public void ExpressionTest(string text, DinoValue expectedValue)
    {
        var vm = new VirtualMachine(new StringReader(text), new VirtualMachineOptions(
            parserMode: ParserMode.ExpressionTest)
        );

        vm.Run();

        // 결과값 가져오기
        var result = vm.Result.Value;

        testOutputHelper.WriteLine($"expectedValue Type: {expectedValue.Type}, actualValue Type: {result.Type}");
        Assert.Equal(result.Type, expectedValue.Type);
        switch (result.Type)
        {
            case DinoType.Integer:
                testOutputHelper.WriteLine($"expectedValue: {(int)expectedValue}, actualValue: {(int)result}");
                Assert.Equal((int)expectedValue, (int)result);
                break;
            case DinoType.Boolean:
                testOutputHelper.WriteLine($"expectedValue: {(bool)expectedValue}, actualValue: {(bool)result}");
                Assert.Equal((bool)expectedValue, (bool)result);
                break;
            case DinoType.Number:
                testOutputHelper.WriteLine($"expectedValue: {(double)expectedValue}, actualValue: {(double)result}");
                Assert.Equal((double)expectedValue, (double)result, 8);
                break;
            case DinoType.Object:
            case DinoType.Array:
            case DinoType.String:
            default:
                throw new NotSupportedException();
        }
    }
}