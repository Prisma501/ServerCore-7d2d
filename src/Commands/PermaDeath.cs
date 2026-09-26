using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class PermaDeathCommand : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Manage permadeath players";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                "   pd add <steamId>\n" +
                "   pd remove <steamId>\n" +
                "   pd list";

        }

        public override string[] getCommands()
        {
            return new[] { "pc-pd", "pd", "permadeath" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                DbPlayer player = null;
                if (_params.Count == 2)
                {
                    if (_params[1].Length != 23)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Can not add SteamId. Invalid UserId: {0}", _params[1]));
                        return;
                    }

                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                    foreach (DbPlayer pl in players)
                    {
                        if (pl.Id.Equals(_params[1].Replace("steam_", "Steam_")))
                        {
                            player = pl;
                            break;
                        }

                    }

                    if (player == null)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The player cannot be found. SteamId: {0}", _params[1]));
                        return;
                    }
                }

                switch (_params[0].ToLower())
                {
                    case "add":
                        AddPD(player);
                        break;
                    case "remove":
                        RemovePD(player);
                        break;
                    case "list":
                        ListPD();
                        break;
                    default:
                        SdtdConsole.Instance.Output("ERR: Invalid subcommand given");
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in PermaDeathCmd.Execute: " + e);
            }
        }

        private static void AddPD(DbPlayer pl)
        {
            if (!PermaDeathClass.DictPermaDeath.ContainsKey(pl.Id))
            {
                PermaDeathClass.DictPermaDeath.Add(pl.Id, pl.Name);
                PermaDeathClass.UpdateXml();
                SdtdConsole.Instance.Output(string.Format("{0} has been added to the permadeath list!", pl.Name));
            }
            else
                SdtdConsole.Instance.Output("ERR: The player is already playing permadeath!");

        }

        private static void RemovePD(DbPlayer pl)
        {
            if (PermaDeathClass.DictPermaDeath.ContainsKey(pl.Id))
            {
                PermaDeathClass.DictPermaDeath.Remove(pl.Id);
                PermaDeathClass.UpdateXml();
                SdtdConsole.Instance.Output(string.Format("{0} has been removed from the permadeath list!", pl.Name));
            }
            else
                SdtdConsole.Instance.Output("ERR: The player is not playing permadeath!");
        }

        private static void ListPD()
        {
            if (PermaDeathClass.DictPermaDeath.Count > 0)
            {
                foreach (KeyValuePair<string, string> _key in PermaDeathClass.DictPermaDeath)
                {
                    SdtdConsole.Instance.Output(string.Format(" SteamId =\"{0}\" Name=\"{1}\"", _key.Key, _key.Value));
                }
            }
            else
                SdtdConsole.Instance.Output("No players are listed for permadeath.");

        }
    }
}
