using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class ChatCommandPermissions : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set permission levels on admin chatcommands";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   "    ccp <command> <pemissionlevel>\n" +
                   "    ccp list\n" +
                   "<command> can be ft, ftw, mv, mvw, tb, rt, get, listwp, setwp, delwp, ls, bag, day7, hostiles, bed, loctrack and bubble";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-ccp", "ccp", "chatcommandpermissions" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1 or 2, found {0}", _params.Count));
                    return;
                }

                int level = 0;
                if (_params.Count == 2)
                {
                    if (!int.TryParse(_params[1], out level))
                    {
                        SdtdConsole.Instance.Output("ERR: Level is not a valid integer.");
                        return;
                    }
                }

                switch (_params[0].ToLower())
                {
                    case "ft":
                        ServerCoreSettings.Instance.ChatCommandPermissions_ft = level;
                        break;
                    case "ftw":
                        ServerCoreSettings.Instance.ChatCommandPermissions_ftw = level;
                        break;
                    case "mv":
                        ServerCoreSettings.Instance.ChatCommandPermissions_mv = level;
                        break;
                    case "mvw":
                        ServerCoreSettings.Instance.ChatCommandPermissions_mvw = level;
                        break;
                    case "listwp":
                        ServerCoreSettings.Instance.ChatCommandPermissions_listwp = level;
                        break;
                    case "setwp":
                        ServerCoreSettings.Instance.ChatCommandPermissions_setwp = level;
                        break;
                    case "delwp":
                        ServerCoreSettings.Instance.ChatCommandPermissions_delwp = level;
                        break;
                    case "bubble":
                        ServerCoreSettings.Instance.ChatCommandPermissions_bubble = level;
                        break;
                    case "tb":
                        ServerCoreSettings.Instance.ChatCommandPermissions_tb = level;
                        break;
                    case "rt":
                        ServerCoreSettings.Instance.ChatCommandPermissions_rt = level;
                        break;
                    case "get":
                        ServerCoreSettings.Instance.ChatCommandPermissions_get = level;
                        break;
                    case "bag":
                        ServerCoreSettings.Instance.ChatCommandPermissions_bag = level;
                        break;
                    case "ls":
                        ServerCoreSettings.Instance.ChatCommandPermissions_ls = level;
                        break;
                    case "day7":
                        ServerCoreSettings.Instance.ChatCommandPermissions_day7 = level;
                        break;
                    case "hostiles":
                        ServerCoreSettings.Instance.ChatCommandPermissions_hostiles = level;
                        break;
                    case "bed":
                        ServerCoreSettings.Instance.ChatCommandPermissions_bed = level;
                        break;
                    case "loctrack":
                        ServerCoreSettings.Instance.ChatCommandPermissions_loctrack = level;
                        break;
                    case "list":
                        SdtdConsole.Instance.Output("Chat Command Permission Levels:");
                        SdtdConsole.Instance.Output("ft: " + ServerCoreSettings.Instance.ChatCommandPermissions_ft);
                        SdtdConsole.Instance.Output("ftw: " + ServerCoreSettings.Instance.ChatCommandPermissions_ftw);
                        SdtdConsole.Instance.Output("mv: " + ServerCoreSettings.Instance.ChatCommandPermissions_mv);
                        SdtdConsole.Instance.Output("mvw: " + ServerCoreSettings.Instance.ChatCommandPermissions_mvw);
                        SdtdConsole.Instance.Output("tb: " + ServerCoreSettings.Instance.ChatCommandPermissions_tb);
                        SdtdConsole.Instance.Output("rt: " + ServerCoreSettings.Instance.ChatCommandPermissions_rt);
                        SdtdConsole.Instance.Output("get: " + ServerCoreSettings.Instance.ChatCommandPermissions_get);
                        SdtdConsole.Instance.Output("bag: " + ServerCoreSettings.Instance.ChatCommandPermissions_bag);
                        SdtdConsole.Instance.Output("ls: " + ServerCoreSettings.Instance.ChatCommandPermissions_ls);
                        SdtdConsole.Instance.Output("listwp: " + ServerCoreSettings.Instance.ChatCommandPermissions_listwp);
                        SdtdConsole.Instance.Output("setwp: " + ServerCoreSettings.Instance.ChatCommandPermissions_setwp);
                        SdtdConsole.Instance.Output("delwp: " + ServerCoreSettings.Instance.ChatCommandPermissions_delwp);
                        SdtdConsole.Instance.Output("bubble: " + ServerCoreSettings.Instance.ChatCommandPermissions_bubble);
                        SdtdConsole.Instance.Output("day7: " + ServerCoreSettings.Instance.ChatCommandPermissions_day7);
                        SdtdConsole.Instance.Output("hostiles: " + ServerCoreSettings.Instance.ChatCommandPermissions_hostiles);
                        SdtdConsole.Instance.Output("bed: " + ServerCoreSettings.Instance.ChatCommandPermissions_bed);
                        SdtdConsole.Instance.Output("loctrack: " + ServerCoreSettings.Instance.ChatCommandPermissions_loctrack);
                        break;
                    default:
                        SdtdConsole.Instance.Output("ERR: Command is not a valid admin chatcommand or invalid subcommand.");
                        return;
                }

                if (_params.Count == 2)
                {
                    SdtdConsole.Instance.Output("Permission level of command " + _params[0] + " has been set to " + _params[1]);
                    ServerCoreSettings.Instance.Save();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ChatCommandPermissions.Execute: {0}.", e));
            }
        }
    }
}
