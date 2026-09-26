using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class PreventFallingBlocks : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Prevent falling blocks on server.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. pfb <logBlockCount> (0 = disabled, blocks do fall)\n" +
                   "  2. pfb\n" +
                   "1. Enable/Disable prevention of falling blocks. Log when number of blocks falling at once exceed <logBlockCount>.\n" +
                   "2. Show the active <logBlockCount> setting.";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-pfb", "pfb", "preventfallingblocks" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 0 or 1, found {0}", _params.Count));
                    return;
                }

                if (_params.Count == 0)
                {
                    //show active
                    int mb = ServerCoreSettings.Instance.PreventFallingBlocks;
                    SdtdConsole.Instance.Output("Current logBlockCount (0 = disabled, blocks do fall): " + mb);
                    return;
                }
                else
                {
                    if (!int.TryParse(_params[0], out int maxBlocks))
                    {
                        SdtdConsole.Instance.Output("ERR: The logBlockCount parameter is not a valid integer!");
                        return;
                    }
                    else
                    {
                        ServerCoreSettings.Instance.PreventFallingBlocks = maxBlocks;
                        ServerCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output("logBlockCount of falling blocks has been set to " + maxBlocks);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in MaxNumberOffBlocks.Exec: {0}.", e));
            }
        }
    }
}
