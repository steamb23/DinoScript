using DinoScript.Runtime;

namespace DinoScript;

public class DinoScript
{
    /// <summary>
    /// 실행 후 가상 머신을 가져옵니다.
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static VirtualMachine Run(string script)
    {
        var virtualMachine = new VirtualMachine(new StringReader(script));
        virtualMachine.Run();
        return virtualMachine;
    }
}