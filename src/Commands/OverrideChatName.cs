using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class OverrideChatName : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Change a player's chat name.";
        }

        public override string getHelp()
        {
            return "Change a player's chat name.\n" +
                   "Usage:\n" +
                   "   ocn <steamId / entityId / playerName> <newName>\n" +
                   "   ocn <steamId / entityId / playerName> clear\n" +
                   "   ocn list\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-overridechatname", "overridechatname", "ocn" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1 or 2, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                if (_params.Count == 1)
                {
                    if (_params[0] == "list")
                    {
                        SdtdConsole.Instance.Output("Players with overridden chatname (Original : NewName)");
                        foreach (var player in Database.Instance.GetAllDbPlayers())
                        {
                            string nameNew = string.Empty;
                            string nameOld = string.Empty;

                            try
                            {
                                nameOld = player.Name;
                            }
                            catch
                            {
                                nameOld = player.Id;
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(player.ChatNameOverride))
                                {
                                    nameNew = player.ChatNameOverride;
                                }
                            }
                            catch
                            {
                                nameNew = player.Id;
                            }

                            if (!string.IsNullOrEmpty(nameNew))
                            {
                                SdtdConsole.Instance.Output(string.Format("{0} : {1}", nameOld, nameNew));
                            }
                        }
                    }
                    else SdtdConsole.Instance.Output("ERR: Argument not recognized as subcommand.");
                    return;
                }

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

                string newName = _params[1];

                if (newName.ToLower() == "clear")
                {
                    Database.Instance.SetChatNameOverride(steamid, string.Empty);

                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ChatNameOverride has been cleared for " + steamid);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ChatNameOverride has been cleared for " + ci.playerName);
                    }
                }
                else
                {
                    Database.Instance.SetChatNameOverride(steamid, newName);

                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("New chatname for " + steamid + " is " + newName);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("New chatname for " + ci.playerName + " is " + newName);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Out("OverrideChatName.Run: " + e);
            }
        }
    }
}
