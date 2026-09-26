using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class CollectGarbage : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Invoke the garbagecollector. Free some memory.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " gc";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-gc", "gc" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (ThreadManager.IsMainThread())
                {
                    SdtdConsole.Instance.Output("Garbagecollector has been invoked.");
                }
                else
                {
                    Log.Out("Garbagecollector has been invoked.");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in CollectGarbage.Exec: {0}.", e));
            }
        }
    }
}
