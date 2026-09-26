using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class BlockUTF8Names : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Kick any players with UTF-8 chars in name at login";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bun <true/false>\n" +
                   " bun";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bun", "bun", "blockutf8names" };
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
                    bool blockUTF8 = ServerCoreSettings.Instance.BlockUTF8Names_Enabled;
                    SdtdConsole.Instance.Output("Blocking players with UTF-8 chars in playername enabled: " + blockUTF8);
                    return;
                }
                else
                {
                    if (!bool.TryParse(_params[0], out bool blocked))
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid parameter. Must be true or false");
                        return;
                    }
                    else
                    {
                        ServerCoreSettings.Instance.BlockUTF8Names_Enabled = blocked;
                        ServerCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output("Blocking players with UTF-8 chars in playername has been set to " + _params[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in BlockUTF8Names.Exec: {0}.", e));
            }
        }
    }
}
