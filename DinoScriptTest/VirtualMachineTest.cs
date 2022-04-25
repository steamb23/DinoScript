using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using DinoScript.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace DinoScript.Test;

using static InternalCode;

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
            1 + 2 * 3 + 4
        },
        new object[]
        {
            "10*0+10",
            10 * 0 + 10
        },
        new object[]
        {
            "1.0 / 3.0",
            1.0 / 3.0
        }
    };

    [Theory]
    [MemberData(nameof(ExpressionTestDataList))]
    public void ExpressionTest(string text, double expectedValue)
    {
        var vm = new VirtualMachine(new StringReader(text), new VirtualMachineOptions(
            parserMode: ParserMode.ExpressionTest)
        );

        vm.Run();

        // 결과값 가져오기
        var result = vm.Result.Double ?? double.NaN;

        testOutputHelper.WriteLine($"expectedValue: {expectedValue}, actualValue: {result}");

        Assert.Equal(expectedValue, result, 8);
    }
}