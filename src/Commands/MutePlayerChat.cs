using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class MutePlayerChat : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "mute a player on public chat";
        }

        public override string getHelp()
        {
            return "Mute a player on public chat." +
            "Usage:\n" +
            "   mcp <steam id/player name/entity id> [true/false]\n" +
            "If the optional parameter is not given the command will show the current status.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-mutechatplayer", "mutechatplayer", "mcp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 2)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1 or 2, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
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

                DbPlayer cpm_p = Database.Instance.GetDbPlayer(steamid.ToString());

                if (cpm_p == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player not found.");
                    return;
                }

                bool muted = false;

                if (_params.Count > 1)
                {
                    bool mute = false;
                    if (_params[1].ToLower() == "true")
                    {
                        mute = true;
                    }
                    else if (_params[1].ToLower() == "false")
                    {
                        mute = false;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: Wrong param 2. It must be \"true\" or \"false\"");
                        return;
                    }
                    Database.Instance.SetChatMuted(cpm_p.Id, mute);
                    muted = mute;
                }
                else
                {
                    muted = cpm_p.ChatMuted;
                }
                SdtdConsole.Instance.Output(cpm_p.Name + " " + (muted ? "muted" : "unmuted"));

            }
            catch (Exception e)
            {
                Log.Out("Error in MutePlayerChat: " + e);
            }
        }
    }
}
