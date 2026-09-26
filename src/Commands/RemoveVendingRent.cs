using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PrismaCore.CustomCommands
{
    public class RemoveVendingRent : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Remove a player's vendingmachine rental status";
        }
        public override string getHelp()
        {
            return "Usage: rvr <platformId>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-rvr", "rvr", "removevendingrental" };
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

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);

                if (ci == null)
                {
                    DbPlayer dbplayer = null;

                    if (_params[0].Trim().ToLower().StartsWith("steam_"))
                    {
                        dbplayer = Database.Instance.GetDbPlayer(_params[0].Trim().Replace("steam_", "Steam_"));
                    }
                    else if (_params[0].Trim().ToLower().StartsWith("xbl_"))
                    {
                        dbplayer = Database.Instance.GetDbPlayer(_params[0].Trim().Replace("xbl_", "XBL_"));
                    }
                    else if (_params[0].Trim().ToLower().StartsWith("psn_"))
                    {
                        dbplayer = Database.Instance.GetDbPlayer(_params[0].Trim().Replace("psn_", "PSN_"));
                    }

                    if (dbplayer == null)
                    {
                        SdtdConsole.Instance.Output($"ERR: Player with steamId/XblId/PsnId {_params[0]} can not be found.");
                        return;
                    }

                    if (File.Exists($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp"))
                    {

                        if (File.Exists($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp.bak.prismacore"))
                        {
                            File.Delete($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp.bak.prismacore");
                        }

                        PlayerDataFile playerDataFile = new PlayerDataFile();
                        playerDataFile.Load(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);

                        playerDataFile.rentedVMPosition = Vector3i.zero;
                        playerDataFile.rentalEndTime = 0UL;
                        playerDataFile.rentalEndTime = 0;

                        if (File.Exists($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp.bak"))
                        {
                            File.Move($"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp.bak", $"{GameIO.GetPlayerDataDir()}/{dbplayer.EOS_Id}.ttp.bak.prismacore");
                        }

                        playerDataFile.Save(GameIO.GetPlayerDataDir(), dbplayer.EOS_Id);
                        playerDataFile = null;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"ERR: Playerfile for steamID {_params[0].Trim()} cannot be found!");
                        return;
                    }
                }
                else
                {
                    StartThreadParameterized(ci);
                }

                SdtdConsole.Instance.Output($"You have removed vendingmachine rental status for steamID {_params[0].Trim()}.");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in RemoveVendingRental.Run: {0}.", e));
            }
        }

        private Thread StartThreadParameterized(ClientInfo clientInfo)
        {
            var t = new Thread(() => resetVendingRentHandler(clientInfo))
            {
                IsBackground = true
            };
            t.Start();
            return t;
        }

        private static void resetVendingRentHandler(ClientInfo clientInfo)
        {
            string steamID = clientInfo.CrossplatformId.ToString();
            string msg = "Removing your vendingmachine rental status.";

            GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), msg));

            Thread.Sleep(7000);

            if (File.Exists($"{GameIO.GetPlayerDataDir()}/{steamID}.ttp"))
            {

                if (File.Exists($"{GameIO.GetPlayerDataDir()}/{steamID}.ttp.bak.prismacore"))
                {
                    File.Delete($"{GameIO.GetPlayerDataDir()}/{steamID}.ttp.bak.prismacore");
                }

                PlayerDataFile playerDataFile = new PlayerDataFile();
                playerDataFile.Load(GameIO.GetPlayerDataDir(), steamID);

                playerDataFile.rentedVMPosition = Vector3i.zero;
                playerDataFile.rentalEndTime = 0UL;
                playerDataFile.rentalEndTime = 0;

                if (File.Exists($"{GameIO.GetPlayerDataDir()}/{steamID}.ttp.bak"))
                {
                    File.Move($"{GameIO.GetPlayerDataDir()}/{steamID}.ttp.bak", $"{GameIO.GetPlayerDataDir()}/{steamID}.ttp.bak.prismacore");
                }

                playerDataFile.Save(GameIO.GetPlayerDataDir(), steamID);
            }
        }
    }
}
