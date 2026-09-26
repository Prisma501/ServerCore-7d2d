using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ListPrismaCorePlayers : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Lists all PrismaCore players ever online.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. listpcplayers\n" +
                   "  2. listpcplayers online\n" +
                   "  3. listpcplayers <player name / UserId>\n" +
                   "1. Lists all players that have ever been online\n" +
                   "2. Lists only the players that are currently online\n" +
                   "3. Lists all players whose name contains the given string or matches the given UserId(Steam/EOS)";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-lpcp", "lpcp", "listpcplayers" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            AdminTools admTools = GameManager.Instance.adminTools;

            bool onlineOnly = false;
            string nameFilter = string.Empty;
            PlatformUserIdentifierAbs userIdFilter = null;

            if (_params.Count == 1)
            {
                if (_params[0].EqualsCaseInsensitive("online"))
                {
                    onlineOnly = true;
                }
                else if (PlatformUserIdentifierAbs.TryFromCombinedString(_params[0], out userIdFilter))
                {

                }
                else
                {
                    nameFilter = _params[0];
                }
            }

            if (userIdFilter != null)
            {
                if (userIdFilter.ToString().StartsWith("Steam_") || userIdFilter.ToString().StartsWith("XBL_"))
                {
                    DbPlayer dbp = Database.Instance.GetDbPlayer(userIdFilter.ToString());
                    if (dbp != null)
                    {
                        SdtdConsole.Instance.Output($"1: {dbp.Name}, id={dbp.EntityId}, PlatformId={dbp.Id}, EOS_Id={dbp.EOS_Id}, Online={dbp.Online}, ip={dbp.IP}, Playtime={dbp.TotalPlaytime / 60} m, ChatNameOverride={dbp.ChatNameOverride}, ColorGroup={dbp.MemberOfGroup}, ChatColor={dbp.ChatColor}, ChatMuted={dbp.ChatMuted}");
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"ERR: Player not found in PrismaCore db.");
                    }
                }
                else if (userIdFilter.ToString().StartsWith("EOS_"))
                {
                    string steamId = Database.Instance.GetSteamIdByEOS(userIdFilter.ToString());
                    DbPlayer dbp = Database.Instance.GetDbPlayer(steamId);
                    if (dbp != null)
                    {
                        SdtdConsole.Instance.Output($"1: {dbp.Name}, id={dbp.EntityId}, PlatformId={dbp.Id}, EOS_Id={dbp.EOS_Id}, Online={dbp.Online}, ip={dbp.IP}, Playtime={dbp.TotalPlaytime / 60} m, ChatNameOverride={dbp.ChatNameOverride}, ColorGroup={dbp.MemberOfGroup}, ChatColor={dbp.ChatColor}, ChatMuted={dbp.ChatMuted}");
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"ERR: Player not found in PrismaCore db.");
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output($"ERR: Unknown UserIdentifier: {userIdFilter}");
                }
            }
            else
            {
                int num = 0;

                List<DbPlayer> lstDbPlayers = Database.Instance.GetAllDbPlayers();

                foreach (DbPlayer dbp in lstDbPlayers)
                {
                    if ((!onlineOnly || dbp.Online) && (nameFilter.Length == 0 || dbp.Name.ContainsCaseInsensitive(nameFilter)))
                    {
                        SdtdConsole.Instance.Output($"{++num}: {dbp.Name}, id={dbp.EntityId}, PlatformId={dbp.Id}, EOS_Id={dbp.EOS_Id}, Online={dbp.Online}, ip={dbp.IP}, Playtime={dbp.TotalPlaytime / 60} m, ChatNameOverride={dbp.ChatNameOverride}, ColorGroup={dbp.MemberOfGroup}, ChatColor={dbp.ChatColor}, ChatMuted={dbp.ChatMuted}");
                    }
                }

                SdtdConsole.Instance.Output($"Total of {lstDbPlayers.Count} players in PrismaCore db.");
            }
        }
    }
}