using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerCore.CustomCommands
{
    public class ResetPlayerData : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Reset a player";
        }
        public override string getHelp()
        {
            return "Usage: resetplayerdata <platformID>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-rpd", "rpd", "resetplayerdata" };
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

                    string fileMAP = $"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}.map";
                    string fileTTP = $"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}.ttp";
                    string fileBAK = $"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}.ttp.bak";
                    string fileMETA = $"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}.ttp.meta";
                    if (SdFile.Exists(fileMAP))
                    {
                        SdFile.Delete(fileMAP);
                    }
                    if (SdFile.Exists(fileTTP))
                    {
                        SdFile.Delete(fileTTP);
                    }
                    if (SdFile.Exists(fileBAK))
                    {
                        SdFile.Delete(fileBAK);
                    }
                    if (SdFile.Exists(fileMETA))
                    {
                        SdFile.Delete(fileMETA);
                    }
                    if (SdDirectory.Exists($"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}"))
                    {
                        SdDirectory.Delete($"{GameIO.GetSaveGameDir()}/Player/{dbplayer.EOS_Id}", true);
                    }
                }
                else
                {
                    StartThreadParameterized(ci);
                }

                string logMsg;
                logMsg = "You have reset the profile for the player.";
                SdtdConsole.Instance.Output(string.Format("{0}", logMsg));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetPlayer.Run: {0}.", e));
            }
        }

        private Thread StartThreadParameterized(ClientInfo clientInfo)
        {
            var t = new Thread(() => resetPlayerHandler(clientInfo))
            {
                IsBackground = true
            };
            t.Start();
            return t;
        }

        private static void resetPlayerHandler(ClientInfo clientInfo)
        {
            string steamID = clientInfo.CrossplatformId.ToString();
            string msg = "Resetting your player data. You can rejoin right away.";

            GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), msg));

            Thread.Sleep(7000);

            string fileMAP = $"{GameIO.GetSaveGameDir()}/Player/{steamID}.map";
            string fileTTP = $"{GameIO.GetSaveGameDir()}/Player/{steamID}.ttp";
            string fileBAK = $"{GameIO.GetSaveGameDir()}/Player/{steamID}.ttp.bak";
            if (SdFile.Exists(fileMAP))
            {
                SdFile.Delete(fileMAP);
            }
            if (SdFile.Exists(fileTTP))
            {
                SdFile.Delete(fileTTP);
            }
            if (SdFile.Exists(fileBAK))
            {
                SdFile.Delete(fileBAK);
            }
        }
    }
}
