using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class Serverchatname : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set server chatname globally";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " scn <name>\n" +
                   " scn";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-scn", "scn", "serverchatname" };
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
                    string cn = PrismaCoreStrings.Instance.ServerChatName;
                    SdtdConsole.Instance.Output("Current global servername for PrismaCore: " + cn);
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(_params[0]))
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid or no servername given.");
                        return;
                    }
                    else
                    {
                        PrismaCoreStrings.Instance.ServerChatName = _params[0];
                        PrismaCoreStrings.Instance.Save();
                        SdtdConsole.Instance.Output("Global servername for PrismaCore has been set to " + _params[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in Serverchatname.Exec: {0}.", e));
            }
        }
    }
}
