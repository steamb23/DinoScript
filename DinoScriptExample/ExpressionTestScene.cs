using System.Diagnostics;
using TerraText;
using DinoScript;
using DinoScript.Parser;
using DinoScript.Runtime;

namespace DinoScriptExample;

public class ExpressionTestScene : Scene
{
    public override void Execute()
    {
        Console.Clear();
        Console.WriteLine("Expression Test...");
        Console.WriteLine();
        while (true)
        {
            Console.WriteLine("식을 입력하세요. (나가려면 exit 입력)");
            Console.Write('>');
            var input = ConsoleEx.ReadLineWithCursor();

            switch (input)
            {
                case "exit":
                    return;
                case "":
                    continue;
            }

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                stopwatch.Restart();
                var machine = Script.Run(input, new VirtualMachineOptions(
                    ParserMode.ExpressionTest)
                );
                stopwatch.Stop();
                Console.WriteLine($"결과: {machine.Result.Double}");
                Console.WriteLine($"실행 시간: {stopwatch.Elapsed.TotalMilliseconds} ms");
                machine.Dispose();
            }
            catch (Exception e)
            {
                // 예외 표시
                Console.WriteLine("실행중 예외가 발생했습니다:");
                Console.WriteLine(e);
            }
        }
    }
}