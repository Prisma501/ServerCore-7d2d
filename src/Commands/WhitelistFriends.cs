using Epic.OnlineServices.Presence;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class WhitelistFriends : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-wlf", "wlf" };
        }

        public override string getDescription()
        {
            return "Adds all friends of a player to whitelist(s) of their claim(s)";
        }

        public override string getHelp()
        {
            return "Adds all friends of a player to whitelist(s) of their claim(s)\n" +
                "Usage:\n" +
                "   wlf add <Name/EntityId/SteamId>\n" +
                "   wlf del <Name/EntityId/SteamId>";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            ExecuteWhiteList(_params);
        }

        private void ExecuteWhiteList(List<string> _params)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2, found " + _params.Count + ".");
                return;
            }

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[1]);
            if (ci == null)
            {
                SdtdConsole.Instance.Output("ERR: Player not found! Player must be online.");
                return;
            }

            PlatformUserIdentifierAbs _steamid = ci.CrossplatformId;
            bool friends = false;
            foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in GameManager.Instance.persistentPlayers.Allies.EnumerateAllies(ci.CrossplatformId))
            {
                    friends = true;
                    string steamId = Database.Instance.GetSteamIdByEOS(platformUserIdentifierAbs.ToString());

                    if (_params[0].ToLower().Equals("add"))
                    {
                        List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();
                        List<string> ownedClaims = new List<string>();

                        foreach (var claim in lstClaims)
                        {
                            if (claim.Id.Contains(ci.PlatformId.ToString()))
                            {
                                ownedClaims.Add(claim.Id);
                            }
                        }

                        if (ownedClaims.Count == 0)
                        {
                            SdtdConsole.Instance.Output("ERR: Player has no advanced claim!");
                            return;
                        }
                        foreach (string ownedClaim in ownedClaims)
                        {
                            DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaim);

                            if (activeClaim.Whitelist.Contains(steamId))
                            {
                                continue;
                            }
                            else
                            {
                                List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                                foreach (DbPlayer player in players)
                                {
                                    string pl = "";

                                    if (player != null)
                                    {
                                        if (player.Id == steamId)
                                        {
                                            pl = player.Name;
                                        }

                                        if (pl != "")
                                        {
                                            string currentWhitelist = activeClaim.Whitelist;
                                            currentWhitelist += pl + "(" + steamId + ")";
                                            Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                            string msg = $"Player {pl} has been added to the whitelist of claim {activeClaim.Id}";
                                            SdtdConsole.Instance.Output(msg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();
                        List<string> ownedClaims = new List<string>();

                        foreach (var claim in lstClaims)
                        {
                            if (claim.Id.Contains(ci.PlatformId.ToString()))
                            {
                                ownedClaims.Add(claim.Id);
                            }
                        }

                        if (ownedClaims.Count == 0)
                        {
                            SdtdConsole.Instance.Output("ERR: Player has no advanced claim!");
                            return;
                        }
                        foreach (string ownedClaim in ownedClaims)
                        {
                            DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaim);

                            if (activeClaim.Whitelist.Contains(steamId))
                            {
                                List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                                foreach (DbPlayer player in players)
                                {
                                    string pl = "";

                                    if (player != null)
                                    {
                                        if (player.Id == steamId)
                                        {
                                            pl = player.Name;
                                        }

                                        if (pl != "")
                                        {
                                            string currentWhitelist = activeClaim.Whitelist;
                                            currentWhitelist = currentWhitelist.Replace(player.Name + "(" + player.Id + ")", "");
                                            Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                            string msg = $"Player {pl} has been removed from the whitelist of claim {activeClaim.Id}";
                                            SdtdConsole.Instance.Output(msg);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
            }
            
            if (!friends)
            {
                string msg = "ERR: Player doesnt have any ingame friends to add/remove from whitelist!";
                SdtdConsole.Instance.Output(msg);
            }
        }
    }
}
