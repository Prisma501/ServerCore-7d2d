using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class Release : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-release", "release" };
        }

        public override string getDescription()
        {
            return "Release a player from jail.";
        }

        public override string getHelp()
        {
            return "Release a player from jail.\n" +
                "Usage:\n" +
                "   release <Name/EntityId/SteamId>";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            ExecuteWhiteList(_params);
        }

        private void ExecuteWhiteList(List<string> _params)
        {
            if (_params.Count != 1)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1, found " + _params.Count + ".");
                return;
            }

            DbClaim activeClaim = Database.Instance.GetDbClaim("jail");
            if (activeClaim == null)
            {
                SdtdConsole.Instance.Output("ERR: Reversed claim \"jail\" not found! Make it with ccc command.");
                return;

            }

            string steamID;
            string playerName;

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
            if (ci == null)
            {
                //check if steamid for offline use
                DbPlayer player = Database.Instance.GetDbPlayer(_params[0]);
                if (player == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player not found!");
                    return;

                }
                else
                {
                    steamID = player.Id;
                    playerName = player.Name;
                }
            }
            else
            {
                steamID = ci.PlatformId.ToString();
                playerName = ci.playerName;
            }

            if (!activeClaim.Whitelist.Contains(steamID))
            {
                SdtdConsole.Instance.Output(playerName + " is not in jail.");
                return;
            }
            else
            {
                string currentWhitelist = activeClaim.Whitelist;
                currentWhitelist = currentWhitelist.Replace(playerName + "(" + steamID + ")", "");
                Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                Database.Instance.SetAutoRelease(steamID, false);
                SdtdConsole.Instance.Output(playerName + " has been released from jail.");
                return;

            }
        }
    }
}
