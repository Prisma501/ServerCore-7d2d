using System;
using System.Collections.Generic;
using System.Threading;

namespace PrismaCore
{
    class LocationTracker
    {
        public static string StatisticsPath = string.Format("{0}/Statistics", GameIO.GetSaveGameDir());
        public static volatile bool IsLocationRunning = false;
        public static Thread threadWhoLocation;

        public static void Start()
        {
            if (PrismaCoreSettings.Instance.LocationTracker_Enabled)
            {
                if (!IsLocationRunning)
                {
                    IsLocationRunning = true;
                    ThreadStart tsLoc = new ThreadStart(RunWhoLocationThread);
                    threadWhoLocation = new Thread(tsLoc);
                    threadWhoLocation.IsBackground = true;
                    threadWhoLocation.Start();
                    Log.Out("[PrismaCore] Started Location monitoring.");
                }
            }
        }

        public static void Unload()
        {
            try
            {
                if (IsLocationRunning)
                {
                    threadWhoLocation.Abort();
                    IsLocationRunning = false;
                    Log.Out("[PrismaCore] Stopped Location monitoring.");
                }
            }
            catch { }
        }

        private static void RunWhoLocationThread()
        {
            ClientInfo ci;

            while (IsLocationRunning)
            {
                if (!PrismaCoreSettings.Instance.LocationTracker_Enabled)
                {
                    IsLocationRunning = false;
                    Log.Out("[PrismaCore] Stopped Location monitoring.");
                    break;
                }

                if (ConnectionManager.Instance.ClientCount() > 0)
                {
                    try
                    {
                        foreach (KeyValuePair<int, EntityPlayer> player in GameManager.Instance.World.Players.dict)
                        {
                            try
                            {
                                ci = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                                if (ci == null) continue;

                                Database.Instance.SavePlayerPosition(ci);


                            }
                            catch (Exception e)
                            {
                                Log.Out("Error in RunWhoLocationThread loop: " + e.ToString());
                                continue;
                            }
                        }
                    }
                    catch { }
                }

                Thread.Sleep(PrismaCoreSettings.Instance.LocationTracker_RecordingIntervalSeconds * 1000);
            }
        }
    }
}
