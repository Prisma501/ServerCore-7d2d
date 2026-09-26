using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class ResetUnclaimedRegionsNow : ConsoleCmdAbstract
    {
        private static Thread thKillServer;
        public override string getDescription()
        {
            return "Reset ALL regions except the ones that have LCB/Normal Adv. Claim on.";
        }

        public override string getHelp()
        {
            return "Usage: resetunclaimedregions [kicklockreboot]";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-resetunclaimedregions", "resetunclaimedregions" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
        }

        public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                //fillup claimed regions list for deleting
                Dictionary<Vector3i, PersistentPlayerData> allBlocks = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
                RegionReset.lstRegionsClaimed.Clear();
                World w = GameManager.Instance.World;

                int LCBsize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
                decimal d = (LCBsize - 1) / 2;
                int halfLCB = Convert.ToInt32(Math.Floor(d));

                if (allBlocks != null)
                {
                    foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allBlocks)
                    {
                        //exclude based on center LCB
                        Vector3i pos = kvp.Key;
                        string regionClaimed = GetRegion(pos);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed);
                        }

                        //exclude based on LCB corners
                        int W = pos.x - halfLCB;
                        int E = pos.x + halfLCB;
                        int N = pos.z + halfLCB;
                        int S = pos.z - halfLCB;

                        //get region files for SW, NW, SE, NE and check
                        Vector3i SW = new Vector3i(W, -1, S);
                        Vector3i NW = new Vector3i(W, -1, N);
                        Vector3i SE = new Vector3i(E, -1, S);
                        Vector3i NE = new Vector3i(E, -1, N);

                        regionClaimed = string.Empty;
                        regionClaimed = GetRegion(SW);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed);
                        }

                        regionClaimed = string.Empty;
                        regionClaimed = GetRegion(NW);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed);
                        }

                        regionClaimed = string.Empty;
                        regionClaimed = GetRegion(SE);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed);
                        }

                        regionClaimed = string.Empty;
                        regionClaimed = GetRegion(NE);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed);
                        }
                    }
                }

                //enumerate normal claims and exclude too
                List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

                foreach (DbClaim activeClaim in lstClaims)
                {
                    if (activeClaim == null) continue;
                    if (activeClaim.Type == "")
                    {
                        Vector3i advClaimPos1 = new Vector3i(activeClaim.W_bound, -1, activeClaim.S_bound);
                        string regionClaimed1 = GetRegion(advClaimPos1);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed1))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed1);
                        }

                        Vector3i advClaimPos2 = new Vector3i(activeClaim.W_bound, -1, activeClaim.N_bound);
                        string regionClaimed2 = GetRegion(advClaimPos2);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed2))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed2);
                        }

                        Vector3i advClaimPos3 = new Vector3i(activeClaim.E_bound, -1, activeClaim.S_bound);
                        string regionClaimed3 = GetRegion(advClaimPos3);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed3))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed3);
                        }

                        Vector3i advClaimPos4 = new Vector3i(activeClaim.E_bound, -1, activeClaim.N_bound);
                        string regionClaimed4 = GetRegion(advClaimPos4);
                        if (!RegionReset.lstRegionsClaimed.Contains(regionClaimed4))
                        {
                            RegionReset.lstRegionsClaimed.Add(regionClaimed4);
                        }
                    }
                }

                SdtdConsole.Instance.Output("[PrismaCore] Unclaimed regions are going to be reset now!!!");

                if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("kicklockreboot"))
                {
                    KickAll();

                    Reset.resetActive = true;

                    RegionReset.RRunclaimed();

                    thKillServer = new Thread(new ThreadStart(KillServer));
                    thKillServer.IsBackground = true;
                    thKillServer.Start();
                    yield break;
                }
                
                RegionReset.RRunclaimed();

                yield break;

            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetUnclaimedRegionsNow.Run: {0}.", e));
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
        private void KickAll()
        {
            ReadOnlyCollection<ClientInfo> list = ConnectionManager.Instance.Clients.List;
            for (int i = 0; i < list.Count; i++)
            {
                ClientInfo clientInfo = list[i];
                GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), PrismaCoreStrings.Instance.ResetPrefabs_KickMessage));
            }

            int fs = 0;
            while (ConnectionManager.Instance.ClientCount() > 0)
            {
                Thread.Sleep(1000);
                fs += 1;
                if (fs == 5)
                {
                    break;
                }
            }
        }
        private static void KillServer()
        {
            Application.Quit();

            Thread.Sleep(30000);

            Process proc = Process.GetCurrentProcess();
            if (proc != null)
            {
                proc.Kill();
            }
        }
    }
}