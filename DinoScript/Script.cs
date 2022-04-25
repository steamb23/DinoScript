using System.IO;
using DinoScript.Runtime;

namespace DinoScript
{
    public class Script
    {
        /// <summary>
        /// 실행 후 가상 머신을 가져옵니다.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static VirtualMachine Run(string script, VirtualMachineOptions? options = null)
        {
            var virtualMachine = new VirtualMachine(new StringReader(script), options);
            virtualMachine.Run();
            return virtualMachine;
        }
    }
}