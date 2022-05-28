using TerraText;
using TerraText.UI;

namespace DinoScriptExample;

public class MainScene : Scene
{
    public override void Execute()
    {
        Console.Clear();
        Option option = new Option("Expression Test", "Statement Test", "나가기");

        switch (option.SelectWithInputForm())
        {
            case 0:
                SceneManager!.ReserveChildScene(new ExpressionTestScene());
                break;
            case 1:
                SceneManager!.ReserveChildScene(new StatementTestScene());
                break;
            default:
                SceneManager!.ReserveExit();
                break;
        }
    }
}