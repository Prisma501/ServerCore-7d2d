using Platform.EOS;
using System;
using System.Collections.Generic;
using static AllyStore;

namespace PrismaCore.CustomCommands
{
    public class AddFriend : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Add yourself to a player friends list and vice versa.";
        }
        public override string getHelp()
        {
            return "Usage: af <platformID>\n" +
                   "    Add yourself to the friendlist of <platformID> and vice versa";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-af", "af", "addfriend" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }

                ClientInfo _cInfo = _senderInfo.RemoteClientInfo;

                if (_cInfo != null)
                {
                    ////player online

                    PersistentPlayerData persistentPlayerData = (GameManager.Instance.persistentPlayers != null) ? GameManager.Instance.persistentPlayers.GetPlayerData(_cInfo.CrossplatformId) : null;
                    if (persistentPlayerData == null)
                    {
                        SdtdConsole.Instance.Output($"Cannot find data for {_cInfo.PlatformId}. Aborting.");
                        return;
                    }

                    if (_params[0].Trim().Length != 23 && _params[0].Trim().Length != 44)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Player is offline. You MUST use steamID/XblId: Invalid SteamId/XblId {0}", _params[0].Trim()));
                        return;
                    }

                    DbPlayer dbplayer = null;

                    if (_params[0].Trim().ToLower().StartsWith("steam_"))
                    {
                        dbplayer = Database.Instance.GetDbPlayer(_params[0].Trim().Replace("steam_", "Steam_"));
                    }
                    else if (_params[0].Trim().ToLower().StartsWith("xbl_"))
                    {
                        dbplayer = Database.Instance.GetDbPlayer(_params[0].Trim().Replace("xbl_", "XBL_"));
                    }

                    if (dbplayer == null)
                    {
                        SdtdConsole.Instance.Output($"ERR: Player with steamId/XblId {_params[0]} can not be found.");
                        return;
                    }

                    PlatformUserIdentifierAbs steamid = new UserIdentifierEos(dbplayer.EOS_Id.Replace("EOS_", string.Empty));

                    if (steamid == null)
                    {
                        SdtdConsole.Instance.Output($"ERR: Player with platformID {_params[0]} can not be found.");
                        return;
                    }

                    PersistentPlayerData persistentPlayerData2 = (_params[0] != null) ? GameManager.Instance.persistentPlayers.GetPlayerData(steamid) : null;
                    if (persistentPlayerData2 == null)
                    {
                        SdtdConsole.Instance.Output($"Cannot find data for {_params[0]}. Aborting.");
                        return;
                    }

                    ///persistentPlayerData.AddPlayerToACL(persistentPlayerData2.PlayerId);

                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAllyResponse>().Setup(steamid, _cInfo.CrossplatformId, AllyStore.AllyStatus.Allies, AllyStore.AllyEvent.IncomingAccepted, AllyStore.AllyEvent.OutgoingAccepted), false, -1, -1, -1, null, 192, false);
                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAllyResponse>().Setup(_cInfo.CrossplatformId, steamid, AllyStore.AllyStatus.Allies, AllyStore.AllyEvent.IncomingAccepted, AllyStore.AllyEvent.OutgoingAccepted), false, -1, -1, -1, null, 192, false);
                    GameManager.Instance.persistentPlayers.Allies.SetStatus(_cInfo.CrossplatformId, steamid, AllyStore.AllyStatus.Allies);

                    SdtdConsole.Instance.Output($"You have added {_params[0]} to your friends list and vice versa.");
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: This command can only be run ingame.");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in AddFriend.Run: {0}.", e));
            }
        }
    }
}
