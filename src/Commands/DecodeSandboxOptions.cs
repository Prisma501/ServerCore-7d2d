using SandboxOptions;
using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class DecodeSandboxOptions : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-decodesandboxoptions", "decodesandboxoptions", "dsbo" };
        }

        public override string getDescription()
        {
            return "Display the active sandbox options in readable format in console.";
        }

        public override string getHelp()
        {
            return "Display the active sandbox options in readable format in console.\n" +
                "Usage:\n" +
                "   dsbo";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            SdtdConsole.Instance.Output("Active Sandboxoptions\nOption: Value");
            foreach (KeyValuePair<SandboxOptions.SandboxOptions, BaseSandboxOption> kvp in SandboxOptionManager.Current.SandboxOptionsDict)
            {
                SdtdConsole.Instance.Output($"{kvp.Key}: {kvp.Value.GetValueTextFromIndex(kvp.Value.GetValueIndex())} ({kvp.Value.GetValueText()})"); 
            }
        }
    }
}
