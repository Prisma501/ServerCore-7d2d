using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class WriteLog : ConsoleCmdAbstract
    {
        //public static List<string> commandsForOutput = new List<string>();
        public static Dictionary<string, bool> commandsForOutput = new Dictionary<string, bool>();

        public override string getDescription()
        {
            return "Write to console.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
            "  1. w2l <msg>\n" +
            "  2. w2l command <command> [splitlog]\n" +
            "1. Write string <msg> to log\n" +
            "2. Write output of command <command> to log. Use parameter \"splitlog\" for 1 logline per outputline.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-w2l", "w2l" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2 && _params.Count != 3)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1,2 or 3 found {0}", _params.Count));
                    return;
                }

                if (_params.Count == 1)
                {
                    Log.Out(_params[0]);
                }
                else if (_params.Count == 2 && _params[0].ToLower().Equals("command"))
                {
                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                    if (commandsForOutput.ContainsKey(_params[1].ToLower()))
                    {
                        commandsForOutput.Remove(_params[1].ToLower());
                        commandsForOutput.Add(_params[1].ToLower(), false);
                    }
                    else
                    {
                        commandsForOutput.Add(_params[1].ToLower(), false);
                    }

                    SdtdConsole.Instance.ExecuteAsync(_params[1], iConsole);
                }
                else if (_params.Count == 3 && _params[0].ToLower().Equals("command") && _params[2].ToLower().Equals("splitlog"))
                {
                    //each outputline on 1 logline
                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                    if (commandsForOutput.ContainsKey(_params[1].ToLower()))
                    {
                        commandsForOutput.Remove(_params[1].ToLower());
                        commandsForOutput.Add(_params[1].ToLower(), true);
                    }
                    else
                    {
                        commandsForOutput.Add(_params[1].ToLower(), true);
                    }

                    SdtdConsole.Instance.ExecuteAsync(_params[1], iConsole);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in Write2Log.Run: {0}.", e));
            }
        }
    }
}
