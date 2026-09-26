using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class ShutdownBA : ConsoleCmdAbstract
    {
        private static Thread thCountdown;
        private static int minutes;
        private static bool countdRunning = false;
        private static int delayedOnDay = 0;

        public override string getDescription()
        {
            return "Timed shutdown with bloodmoon awareness.";
        }

        public override string getHelp()
        {
            return "Usage: shutdownba <minutes> [resetvehicles] [resetdrones]\n" +
                   "shutdownba stop\n" +
                   "shutdownba delayfrom <inGameHour>\n" +
                   "shutdownba delayuntil <inGameHour>\n" +
                   "shutdownba counttext <text>\n" +
                   "shutdownba delaytext <text>";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-shutdownba", "shutdownba" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 0)
                {
                    SdtdConsole.Instance.Output(string.Format("Active shutdownba settings:"));
                    SdtdConsole.Instance.Output(string.Format("Shutdown count msg: {0}", PrismaCoreStrings.Instance.ShutdownBA_CountdownMessage));
                    SdtdConsole.Instance.Output(string.Format("Shutdown delay msg: {0}", PrismaCoreStrings.Instance.ShutdownBA_RestartDelayedMessage));
                    SdtdConsole.Instance.Output(string.Format("Delay shutdown bloodday after: {0}", PrismaCoreSettings.Instance.ShutdownBA_DelayRestartBloodDayAfter));
                    SdtdConsole.Instance.Output(string.Format("Delay after bloodmoon until: {0}", PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil));
                    return;
                }
                else if (_params.Count == 1)
                {
                    if (_params[0] == "stop")
                    {
                        if (!countdRunning)
                        {
                            SdtdConsole.Instance.Output("ERR: shutdownba is not active.");
                        }
                        else
                        {
                            thCountdown.Abort();
                            countdRunning = false;
                            //if(File.Exists(string.Format("{0}/reset", API.RegionPath)))
                            //{
                            //    File.Delete(string.Format("{0}/reset", API.RegionPath));
                            //}
                            //RegionReset.resetRegions = false;
                            //RegionReset.resetUnclaimed = false;
                            RegionReset.resetVehicles = false;
                            RegionReset.resetDrones = false;
                            SdtdConsole.Instance.Output("[PrismaCore] shutdownba was manually interrupted.");
                        }
                    }
                    else
                    {
                        if (countdRunning)
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                        }
                        else
                        {
                            if (!int.TryParse(_params[0], out minutes))
                            {
                                SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                            }
                            else
                            {
                                Start();
                            }
                        }

                    }
                }
                else if (_params.Count > 1 && _params.Count < 4)
                {
                    if (_params[0].Trim().ToLower() == "delayfrom")
                    {
                        if (!int.TryParse(_params[1], out int delayHour))
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for delayfrom is not a valid integer."));
                            return;
                        }

                        PrismaCoreSettings.Instance.ShutdownBA_DelayRestartBloodDayAfter = delayHour;
                        PrismaCoreSettings.Instance.Save();

                        SdtdConsole.Instance.Output(string.Format("delayfrom has been set to {0}", delayHour));
                        return;
                    }
                    else if (_params[0].Trim().ToLower() == "delayuntil")
                    {
                        if (!int.TryParse(_params[1], out int delayUntil))
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for delayuntil is not a valid integer."));
                            return;
                        }

                        PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil = delayUntil;
                        PrismaCoreSettings.Instance.Save();

                        SdtdConsole.Instance.Output(string.Format("delayuntil has been set to {0}.", delayUntil));
                        return;
                    }
                    else if (_params[0].Trim().ToLower() == "counttext")
                    {
                        string counttext = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for counttext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            counttext = _params[1].Trim();
                            PrismaCoreStrings.Instance.ShutdownBA_CountdownMessage = counttext;
                            PrismaCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("counttext has been set to \"{0}\"", counttext));
                        }
                        return;
                    }
                    else if (_params[0].Trim().ToLower() == "delaytext")
                    {
                        string delaytext = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for delaytext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            delaytext = _params[1].Trim();
                            PrismaCoreStrings.Instance.ShutdownBA_RestartDelayedMessage = delaytext;
                            PrismaCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("delaytext has been set to \"{0}\"", delaytext));
                        }
                        return;
                    }

                    //if (_params.ContainsCaseInsensitive("reset"))
                    //{
                    //    if (countdRunning)
                    //    {
                    //        SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        if (!int.TryParse(_params[0], out minutes))
                    //        {
                    //            SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            //using (File.Create(string.Format("{0}/reset", API.RegionPath))) { }
                    //            //RegionReset.resetRegions = true;
                    //            SdtdConsole.Instance.Output("[PrismaCore] Regions marked for reset are going to be reset at shutdown!!!");
                    //            //Start();
                    //        }
                    //    }
                    //}
                    //else if (_params[1].EqualsCaseInsensitive("resetprefabs"))
                    //{
                    //    if (countdRunning)
                    //    {
                    //        SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                    //    }
                    //    else
                    //    {
                    //        if (!int.TryParse(_params[0], out minutes))
                    //        {
                    //            SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                    //        }
                    //        else
                    //        {
                    //            using (File.Create(string.Format("{0}/resetprefabs", API.RegionPath))) { }
                    //            SdtdConsole.Instance.Output("[PrismaCore] RWG Prefabs are going to reset at shutdown!!!");
                    //            Start();
                    //        }
                    //    }
                    //}

                    //if (_params.ContainsCaseInsensitive("resetunclaimed"))
                    //{
                    //    if (countdRunning)
                    //    {
                    //        SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        if (!int.TryParse(_params[0], out minutes))
                    //        {
                    //            SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            Dictionary<Vector3i, PersistentPlayerData> allBlocks = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
                    //            RegionReset.lstRegionsClaimed.Clear();

                    //            int LCBsize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
                    //            decimal d = (LCBsize - 1) / 2;
                    //            int halfLCB = Convert.ToInt32(Math.Floor(d));

                    //            if (allBlocks != null)
                    //            {
                    //                foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allBlocks)
                    //                {
                    //                    //exclude based on center LCB
                    //                    Vector3i pos = kvp.Key;
                    //                    string regionClaimed = GetRegion(pos);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed);
                    //                    }

                    //                    //exclude based on LCB corners
                    //                    int W = pos.x - halfLCB;
                    //                    int E = pos.x + halfLCB;
                    //                    int N = pos.z + halfLCB;
                    //                    int S = pos.z - halfLCB;

                    //                    //get region files for SW, NW, SE, NE and check
                    //                    Vector3i SW = new Vector3i(W, -1, S);
                    //                    Vector3i NW = new Vector3i(W, -1, N);
                    //                    Vector3i SE = new Vector3i(E, -1, S);
                    //                    Vector3i NE = new Vector3i(E, -1, N);

                    //                    regionClaimed = string.Empty;
                    //                    regionClaimed = GetRegion(SW);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed);
                    //                    }

                    //                    regionClaimed = string.Empty;
                    //                    regionClaimed = GetRegion(NW);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed);
                    //                    }

                    //                    regionClaimed = string.Empty;
                    //                    regionClaimed = GetRegion(SE);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed);
                    //                    }

                    //                    regionClaimed = string.Empty;
                    //                    regionClaimed = GetRegion(NE);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed);
                    //                    }
                    //                }
                    //            }

                    //            //enumerate normal claims and exclude too
                    //            List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();
                    //            foreach (var activeClaim in lstClaims)
                    //            {
                    //                if (activeClaim == null) continue;

                    //                if (activeClaim.Type == "")
                    //                {
                    //                    Vector3i advClaimPos1 = new Vector3i(activeClaim.W_bound, -1, activeClaim.S_bound);
                    //                    string regionClaimed1 = GetRegion(advClaimPos1);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed1))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed1);
                    //                    }

                    //                    Vector3i advClaimPos2 = new Vector3i(activeClaim.W_bound, -1, activeClaim.N_bound);
                    //                    string regionClaimed2 = GetRegion(advClaimPos2);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed2))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed2);
                    //                    }

                    //                    Vector3i advClaimPos3 = new Vector3i(activeClaim.E_bound, -1, activeClaim.S_bound);
                    //                    string regionClaimed3 = GetRegion(advClaimPos3);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed3))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed3);
                    //                    }

                    //                    Vector3i advClaimPos4 = new Vector3i(activeClaim.E_bound, -1, activeClaim.N_bound);
                    //                    string regionClaimed4 = GetRegion(advClaimPos4);
                    //                    if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed4))
                    //                    {
                    //                        RegionReset.lstRegionsClaimed.Add(regionClaimed4);
                    //                    }
                    //                }
                    //            }

                    //            //using (File.Create(string.Format("{0}/resetunclaimed", API.RegionPath))) { }
                    //            //RegionReset.resetUnclaimed = true;
                    //            SdtdConsole.Instance.Output("[PrismaCore] Regions that have NO claimblocks/Normal Adv. Claims on are going to be reset at shutdown!!!");
                    //            //Start();
                    //        }
                    //    }
                    //}

                    if (_params.ContainsCaseInsensitive("resetvehicles"))
                    {
                        if (countdRunning)
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                            return;
                        }
                        else
                        {
                            if (!int.TryParse(_params[0], out minutes))
                            {
                                SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                                return;
                            }
                            else
                            {
                                RegionReset.resetVehicles = true;
                                SdtdConsole.Instance.Output("[PrismaCore] ALL vehicles on map will be deleted on shutdown!");
                                //Start();
                            }
                        }
                    }

                    if (_params.ContainsCaseInsensitive("resetdrones"))
                    {
                        if (countdRunning)
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: Server is already stopping in {0} minutes", minutes));
                            return;
                        }
                        else
                        {
                            if (!int.TryParse(_params[0], out minutes))
                            {
                                SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of minutes specified: {0}", minutes));
                                return;
                            }
                            else
                            {
                                RegionReset.resetDrones = true;
                                SdtdConsole.Instance.Output("[PrismaCore] ALL drones on map will be deleted on shutdown!");
                                //Start();
                            }
                        }
                    }

                    Start();
                }
                else
                {
                    //wrong number of params
                    SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of parameters specified: {0}", _params.Count));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in shutdownba.Run: {0}.", e));
            }
        }

        private string GetRegion(Vector3i coords)
        {
            int rgLat = Math.DivRem(coords.x, 512, out int remainderLat);
            int rgLng = Math.DivRem(coords.z, 512, out int remainderLng);

            if (coords.x < 0) rgLat -= 1;
            if (coords.z < 0) rgLng -= 1;

            string regionFile = string.Format("r.{0}.{1}.7rg", rgLat.ToString(), rgLng.ToString());
            return regionFile;
        }

        private static void Start()
        {
            thCountdown = new Thread(new ThreadStart(Countdown));
            thCountdown.IsBackground = true;
            //thCountdown.Priority= System.Threading.ThreadPriority.BelowNormal;
            thCountdown.Start();
        }

        private static void Countdown()
        {
            countdRunning = true;
            World w = GameManager.Instance.World;

            if (minutes > 0)
            {
                //check for bm tresholds
                int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));

                if (!IsEnoughUptime())
                {
                    int minUptime = PrismaCoreSettings.Instance.ShutdownBA_MinimumUptimeRequired;
                    int uptime = Convert.ToInt32(Time.timeSinceLevelLoad / 60f);

                    Log.Out($"[PrismaCore] The server has not reached required uptime for a shutdownba yet. Uptime required: {minUptime} minutes. Server uptime: {uptime} minutes. Waiting for {minUptime - uptime} minutes.");
                    while (!IsEnoughUptime())
                    {
                        Thread.Sleep(60000);
                    }
                }

                if (BMcycle > 0)
                {
                    delayedOnDay = 0;
                    if (MustDelay())
                    {
                        string m = PrismaCoreStrings.Instance.ShutdownBA_RestartDelayedMessage.Replace("{DelayUntil}", PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil.ToString());
                        SdtdConsole.Instance.Output(string.Format("Shutdown has been delayed until after bloodmoon at {0}", PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil.ToString()));
                        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, string.Format("{0}", m)), null, EMessageSender.None);
                        while (MustDelay())
                        {
                            Thread.Sleep(10000);
                        }
                    }
                }

                string t = PrismaCoreStrings.Instance.ShutdownBA_CountdownMessage;

                while (minutes != 1)
                {
                    t = t.Replace("{Minutes}", minutes.ToString());
                    GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, string.Format("{0}", t)), null, EMessageSender.None);
                    int ov = minutes;
                    minutes = minutes - 1;
                    t = t.Replace(ov.ToString(), minutes.ToString());
                    Thread.Sleep(60000);
                }

                t = t.Replace("{Minutes}", minutes.ToString());
                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, string.Format("{0}", t)), null, EMessageSender.None);
                Thread.Sleep(60000);
            }
            //CmdClaimCommandResult iConsole = new CmdClaimCommandResult();

            //SdtdConsole.Instance.ExecuteSync("saveworld", null);

            //SdtdConsole.Instance.ExecuteSync($"kickall \"{PrismaCoreStrings.Instance.ShutdownBA_KickMessage}\"", null);

            //int fs = 0;
            //while (ConnectionManager.Instance.ClientCount() > 0)
            //{
            //    Thread.Sleep(1000);
            //    fs += 1;
            //    if(fs == 5)
            //    {
            //        break;
            //    }
            //}

            SdtdConsole.Instance.ExecuteSync("shutdown", null);

            //if shutdown fails for any reason -> kill main process after 30 secs
            Thread.Sleep(30000);

            Process proc = Process.GetCurrentProcess();
            if (proc != null)
            {
                proc.Kill();
            }

        }

        private static bool IsEnoughUptime()
        {
            int minUptime = PrismaCoreSettings.Instance.ShutdownBA_MinimumUptimeRequired;

            if (minUptime > 0)
            {
                int uptime = Convert.ToInt32(Time.timeSinceLevelLoad / 60f);
                if (uptime > minUptime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MustDelay()
        {
            int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int hours = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);

            int remainder;

            int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));
            int BMrange = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonRange"));


            if (BMrange > 0)
            {
                //random bm handling
                //= days equal to Bloodmoonday? -> bloodmoonday :P

                int num = (int)SkyManager.dayCount;
                int bmDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);

                bool bmActive = (num == bmDay && SkyManager.TimeOfDay() >= 22f) || (num > 1 && num == bmDay + 1 && SkyManager.TimeOfDay() <= 4f);

                if (days == bmDay && hours >= PrismaCoreSettings.Instance.ShutdownBA_DelayRestartBloodDayAfter)
                {
                    delayedOnDay = days;
                    return true;
                }

                if (bmDay == days - 1 && bmActive)
                {
                    delayedOnDay = days - 1;
                    return true;
                }

                if (bmDay > days && !bmActive && delayedOnDay == days - 1 && hours < PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil)
                {
                    return true;
                }
            }
            else
            {
                int q = Math.DivRem(days, BMcycle, out remainder);

                if (days == 1)
                {
                    return false;
                }

                if (remainder == 0)
                {
                    //bloodday!! check for time that should delay shutdown
                    if (hours >= PrismaCoreSettings.Instance.ShutdownBA_DelayRestartBloodDayAfter && BMrange == 0)
                    {
                        //its past delay time -> return true
                        return true;
                    }
                }
                else if (remainder == 1 && hours < 4 && BMrange == 0)
                {
                    //bloodmoon is ongoing -> do delay
                    return true;
                }
                else if (remainder == 1 && hours < PrismaCoreSettings.Instance.ShutdownBA_DelayRestartAfterBloodmoonUntil && BMrange == 0)
                {
                    //bloodmoon over -> delay until time is reached
                    return true;
                }
            }

            return false;
        }
    }
}