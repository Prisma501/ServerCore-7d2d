using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ChatPlayerColor : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Change default player chat color.";
        }

        public override string getHelp()
        {
            return "Change the player`s chat color." +
                   "Usage:\n" +
                   "   pcc <steam id/player name/entity id> <color> <nameOnly>\n" +
                   "the <color> must be a 6 hex characters. Example: FF00FF\n" +
                   "the <nameOnly> must be a 1 to color only name and 0 to color all text\n" +
                   "the default chat color is FFFFFF\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-playerchatcolor", "playerchatcolor", "pcc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 3, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                //PlatformUserIdentifierAbs userId = API.GetUserIdAbs(_params[0]);
                //string steamid = userId.ToString();

                string steamid = string.Empty;
                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (ci == null)
                {
                    if (_params[0].Length != 23)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Player is offline and the parameter for steamid is not a valid steamid."));
                        return;
                    }
                    else
                    {
                        steamid = _params[0].Replace("steam_", "Steam_");
                    }
                }
                else
                {
                    steamid = ci.PlatformId.ToString();
                }

                //if (steamid == null)
                //{
                //    SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                //    return;
                //}

                string chatName = _params[2];
                if (!chatName.Equals("0") && !chatName.Equals("1"))
                {
                    SdtdConsole.Instance.Output("ERR: Invalid value for <nameOnly> parameter");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                string chatColor = _params[1].ToUpper();
                if (chatColor.Length != 6)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid Color");
                    return;
                }

                if (chatColor.ToLower().Equals("ffffff"))
                {
                    Database.Instance.SetChatColor(steamid, string.Empty);
                }
                else
                {
                    Database.Instance.SetChatColor(steamid, "[" + chatColor + "]");
                }
                if (chatName.Equals("1"))
                {
                    Database.Instance.SetChatName(steamid, true);
                }
                else
                {
                    Database.Instance.SetChatName(steamid, false);
                }

                SdtdConsole.Instance.Output("ChatColor for " + steamid + " is set to " + chatColor);


            }
            catch (Exception e)
            {
                Log.Out("ChatPlayerColor.Run: " + e);
            }
        }
    }
}
