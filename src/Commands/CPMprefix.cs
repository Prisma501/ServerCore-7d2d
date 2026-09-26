using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class PrismaCorePrefix : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set prefix for PrismaCore chatcommands.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " prismacoreprefix <prefix>\n" +
                   " prismacoreprefix";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-prismacoreprefix", "prismacoreprefix" };
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
                    string pf = ServerCoreSettings.Instance.PrismaCorePrefix;
                    SdtdConsole.Instance.Output("Current PrismaCore chatcommand prefix: " + pf);
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(_params[0]))
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid or no prefix given.");
                        return;
                    }
                    else
                    {
                        ServerCoreSettings.Instance.PrismaCorePrefix = _params[0];
                        ServerCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output("PrismaCore chatcommand prefix has been set to " + _params[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in PrismaCorePrefix.Exec: {0}.", e));
            }
        }
    }
}
