using System;
using System.Collections.Generic;
using System.IO;

namespace ServerCore.CustomCommands
{
    public class ResetDeathCount : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set a player's deathcount.";
        }
        public override string getHelp()
        {
            return "Usage: sdc <name/entityId/platformID> <count>\n" +
                   "    Use operators + and - to add or subtract to/from deathcount (ex. +5 or -1)\n" +
                   "    Without operator the new deathcount will be <count>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-sdc", "sdc", "setdeathcount" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                    return;
                }

                bool add = false;
                bool subtract = false;
                int value = int.MinValue;

                if (_params[1].StartsWith("+"))
                {
                    add = true;
                    string tmp = _params[1].Replace("+", "");
                    value = Convert.ToInt32(tmp);
                }
                else if (_params[1].StartsWith("-"))
                {
                    subtract = true;
                    string tmp = _params[1].Replace("-", "");
                    value = Convert.ToInt32(tmp);
                }

                int dc;

                if (value == int.MinValue)
                {
                    if (!int.TryParse(_params[1], out dc))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Parameter for deathcount is not a valid integer: {0}", _params[1]));
                        return;
                    }
                }
                else
                {
                    dc = value;
                }

                ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[0]);

                if (_cInfo != null)
                {
                    //player online
                    EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];

                    if (add || subtract)
                    {
                        if (add)
                        {
                            //add
                            int old = player.Died;

                            player.Died = old + dc;

                            player.bPlayerStatsChanged = true;
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));

                            SdtdConsole.Instance.Output($"You have added {dc} to the deathcount of {_cInfo.playerName}. New deathcount: {old + dc}");
                        }
                        else
                        {
                            //subtract
                            int old = player.Died;
                            if (old < dc)
                            {
                                //goes below zere -> set zero
                                player.Died = 0;
                            }
                            else
                            {
                                player.Died = old - dc;
                            }

                            player.bPlayerStatsChanged = true;
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));

                            if (old < dc)
                            {
                                SdtdConsole.Instance.Output($"You have subtracted {dc} from the deathcount of {_cInfo.playerName}. New deathcount: 0");
                            }
                            else
                            {
                                SdtdConsole.Instance.Output($"You have subtracted {dc} from the deathcount of {_cInfo.playerName}. New deathcount: {old - dc}");
                            }
                        }
                    }
                    else
                    {
                        //raw count
                        if (dc < 0)
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: Parameter for deathcount is below zero: {0}", dc));
                            return;
                        }

                        if (player.Died != dc)
                        {
                            player.Died = dc;
                            player.bPlayerStatsChanged = true;

                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));

                            SdtdConsole.Instance.Output($"You have set the deathcount for {_cInfo.playerName} to {dc}.");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output($"The deathcount for {_cInfo.playerName} is allready {dc}.");
                        }
                    }
                }
                else
                {
                    //player offline
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

                    if (File.Exists($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp"))
                    {
                        PlayerDataFile playerDataFile = new PlayerDataFile();
                        playerDataFile.Load(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);

                        if (add || subtract)
                        {
                            if (add)
                            {
                                //add
                                int old = playerDataFile.deaths;

                                playerDataFile.deaths = old + dc;
                                playerDataFile.Save(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);

                                SdtdConsole.Instance.Output($"You have added {dc} to the deathcount of {_params[0].Trim()}. New deathcount: {old + dc}");
                            }
                            else
                            {
                                //subtract
                                int old = playerDataFile.deaths;
                                if (old < dc)
                                {
                                    //goes below zere -> set zero
                                    playerDataFile.deaths = 0;
                                }
                                else
                                {
                                    playerDataFile.deaths = old - dc;
                                }

                                playerDataFile.Save(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);

                                if (old < dc)
                                {
                                    SdtdConsole.Instance.Output($"You have subtracted {dc} from the deathcount of {_params[0].Trim()}. New deathcount: 0");
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output($"You have subtracted {dc} from the deathcount of {_params[0].Trim()}. New deathcount: {old - dc}");
                                }
                            }
                        }
                        else
                        {
                            //raw count
                            if (dc < 0)
                            {
                                SdtdConsole.Instance.Output(string.Format("ERR: Parameter for deathcount is below zero: {0}", dc));
                                return;
                            }

                            if (playerDataFile.deaths != dc)
                            {
                                playerDataFile.deaths = dc;
                                playerDataFile.Save(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);

                                SdtdConsole.Instance.Output($"You have set the deathcount for {_params[0].Trim()} to {dc}.");
                            }
                            else
                            {
                                SdtdConsole.Instance.Output($"The deathcount for {_params[0].Trim()} is allready {dc}.");
                            }
                        }

                        playerDataFile = null;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"ERR: Playerfile for platformID {_params[0].Trim()} cannot be found!");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in SetDeathCount.Run: {0}.", e));
            }
        }
    }
}
