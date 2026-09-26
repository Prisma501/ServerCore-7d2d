using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class Reset : ConsoleCmdAbstract
    {
        public static bool resetActive = false;
        private static Thread thKillServer;

        public override string getDescription()
        {
            return "Reset the area(s) covered by Adv. Claim Reset to RWG default on chunk level.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "resetadvclaim [unclaimed] [kicklockreboot]\n" +
                   "   Reset all area(s) covered by Adv. Claim Reset to RWG default on chunk level.\n" +
                   "   Use parameter kicklockreboot to kick online players and lock server during reset. Reboots server when done.\n" +
                   "   Use parameter unclaimed to reset all BUT claimed chunks (takes a long time!).";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-resetadvclaim", "rac", "resetadvclaim" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
        }


        public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("unclaimed"))
                {
                    ResetUnclaimed();
                    yield break;
                }
                else if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("kicklockreboot"))
                {
                    //kick
                    KickAll();

                    //lock
                    resetActive = true;

                    ResetClaims();

                    //reboot
                    thKillServer = new Thread(new ThreadStart(KillServer));
                    thKillServer.IsBackground = true;
                    thKillServer.Start();

                }
                else if (_params.Count == 2 && _params[0].Trim().ToLower().Equals("unclaimed") && _params[1].Trim().ToLower().Equals("kicklockreboot"))
                {
                    //kick
                    KickAll();

                    //lock
                    resetActive = true;

                    ResetUnclaimed();

                    //reboot
                    thKillServer = new Thread(new ThreadStart(KillServer));
                    thKillServer.IsBackground = true;
                    thKillServer.Start();

                }
                else
                {
                    ResetClaims();
                    yield break;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetAdvClaim.Run: {0}.", e));
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

        private void ResetClaims()
        {
            Log.Out("[PrismaCore] Started reset of all chunks in Adv. Claim(s) Reset.");

            List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

            if (lstClaims != null && lstClaims.Count != 0)
            {
                foreach (DbClaim activeClaim in lstClaims)
                {
                    if (activeClaim == null) continue;

                    if (activeClaim.Type.ToLower().Equals("reset"))
                    {
                        int x1 = activeClaim.W_bound;
                        int z1 = activeClaim.S_bound;
                        int x2 = activeClaim.E_bound;
                        int z2 = activeClaim.N_bound;

                        Vector2i vector2i = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
                        Vector2i vector2i2 = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);

                        Vector2i claimMin = vector2i;
                        Vector2i claimMax = vector2i2;

                        if (vector2i2.x - vector2i.x > 16384 || vector2i2.y - vector2i.y > 16384)
                        {
                            Log.Out($"[PrismaCore] Adv. Claim Reset area too big to reset for claim {activeClaim.Id}. Aborting reset.");
                            return;
                        }
                        vector2i = World.toChunkXZ(vector2i);
                        vector2i2 = World.toChunkXZ(vector2i2);
                        HashSetLong hashSetLong = new HashSetLong();
                        for (int i = vector2i.x; i <= vector2i2.x; i++)
                        {
                            for (int j = vector2i.y; j <= vector2i2.y; j++)
                            {
                                hashSetLong.Add(WorldChunkCache.MakeChunkKey(i, j));
                            }
                        }

                        World world = GameManager.Instance.World;
                        ChunkCluster chunkCache = world.ChunkCache;
                        ChunkProviderGenerateWorld chunkProviderGenerateWorld = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
                        if (chunkProviderGenerateWorld != null)
                        {
                            Log.Out($"[PrismaCore] Started reset of Adv. Claim Reset: {activeClaim.Id}");
                            //GameManager.Instance.ResetWindowsAndLocksByChunks(hashSetLong);
                            LockManager.Instance.ForceUnlockByChunk(hashSetLong);
                            chunkProviderGenerateWorld.RemoveChunks(hashSetLong);
                            foreach (long key in hashSetLong)
                            {
                                try
                                {
                                    if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, key, true))
                                    {
                                        Log.Out(string.Format("[PrismaCore] Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
                                    }
                                }
                                catch { continue; }
                            }

                            world.m_ChunkManager.ResendChunksToClients(hashSetLong);

                            if (DynamicMeshManager.Instance != null)
                            {
                                using (HashSetLong.Enumerator enumerator = hashSetLong.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        long key7 = enumerator.Current;
                                        DynamicMeshManager.Instance.AddChunk(key7, true, true, null);
                                    }
                                }
                            }

                            chunkProviderGenerateWorld.SaveAll();
                            //chunkCache.Clear();
                            //chunkProviderGenerateWorld.ClearCaches();

                            //reset all sleepervolumes inside the resetclaim
                            int sleeperVolumeCount = world.sleeperVolumes.Count;
                            for (int i = 0; i < sleeperVolumeCount; i++)
                            {
                                SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
                                if (sleeperVolume != null)
                                {
                                    Vector3 center = sleeperVolume.Center;
                                    //Log.Out($"sleepervolume center is at {center}");
                                    if (center.x >= claimMin.x && center.x <= claimMax.x && center.z >= claimMin.y && center.z <= claimMax.y)
                                    {
                                        sleeperVolume.DespawnAndReset(world);
                                    }
                                }
                            }

                            //GameManager.Instance.SaveWorld();

                            Log.Out($"[PrismaCore] Reset of Adv. Claim Reset: {activeClaim.Id} done.");
                            //GC.Collect();
                            //GC.WaitForPendingFinalizers();
                        }
                        else
                        {
                            Log.Out($"[PrismaCore] Chunks of Adv. Claim Reset: {activeClaim.Id} could not be reset! Chunkprovider for generated world not available.");
                        }
                    }
                }

                Log.Out($"[PrismaCore] Reset of all Adv. Claim(s) Reset done and sleepers have been reset. Server reboot highly recommended!");
            }
        }

        private void ResetUnclaimed()
        {
            Log.Out("[PrismaCore] Started reset of all unclaimed chunks. This takes a long time!");

            int claimSize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
            int mapSize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("WorldGenSize"));
            int mapMin = 0 - (mapSize / 2);
            int mapMax = mapSize / 2;

            decimal d = (claimSize - 1) / 2;
            int halfClaimSize = Convert.ToInt32(Math.Floor(d));

            Dictionary<Vector3i, PersistentPlayerData> allBlocks = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;

            HashSetLong lcbOccupiedChunks = new HashSetLong();

            if (allBlocks != null)
            {
                foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allBlocks)
                {
                    Vector3i pos = kvp.Key;

                    int w = World.toChunkXZ(pos.x - halfClaimSize);
                    int e = World.toChunkXZ(pos.x + halfClaimSize);
                    int s = World.toChunkXZ(pos.z - halfClaimSize);
                    int n = World.toChunkXZ(pos.z + halfClaimSize);
                    for (int i = w; i <= e; i++)
                    {
                        for (int j = s; j <= n; j++)
                        {
                            lcbOccupiedChunks.Add(WorldChunkCache.MakeChunkKey(i, j));
                        }
                    }
                }
            }

            //make map partitions and loop through them
            // save the map in between
            Vector2i vector2i = new Vector2i(mapMin, mapMin);
            Vector2i vector2i2 = new Vector2i(mapMax, mapMax);

            if (vector2i2.x - vector2i.x > 16384 || vector2i2.y - vector2i.y > 16384)
            {
                SdtdConsole.Instance.Output("ERR: Map too big to reset. Aborting.");
                return;
            }
            vector2i = World.toChunkXZ(vector2i);
            vector2i2 = World.toChunkXZ(vector2i2);

            HashSetLong hashSetLong = new HashSetLong();
            for (int i = vector2i.x; i <= vector2i2.x; i++)
            {
                for (int j = vector2i.y; j <= vector2i2.y; j++)
                {
                    long ck = WorldChunkCache.MakeChunkKey(i, j);

                    if (!lcbOccupiedChunks.Contains(ck))
                    {
                        hashSetLong.Add(ck);
                    }
                    else
                    {
                        //Log.Out($"Chunk at position {WorldChunkCache.extractX(ck) << 4}/{WorldChunkCache.extractZ(ck) << 4} skipped because claimed!");
                    }
                }
            }

            //diff chunkproviders for mem reduction
            HashSet<long> newHash = new HashSet<long>();

            foreach (long lng in hashSetLong)
            {
                newHash.Add(lng);
            }

            var divides = newHash.Divide(16);
            int progress = 0;

            foreach (HashSet<long> hs in divides)
            {
                World world = GameManager.Instance.World;
                ChunkCluster chunkCache = world.ChunkCache;
                ChunkProviderGenerateWorld chunkProviderGenerateWorld = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
                if (chunkProviderGenerateWorld != null)
                {
                    HashSetLong tmpHash = new HashSetLong();

                    foreach (long lng in hs)
                    {
                        tmpHash.Add(lng);
                    }
                    //GameManager.Instance.ResetWindowsAndLocksByChunks(hashSetLong);
                    LockManager.Instance.ForceUnlockByChunk(tmpHash);
                    chunkProviderGenerateWorld.RemoveChunks(tmpHash);
                    foreach (long key in tmpHash)
                    {
                        try
                        {
                            if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, key, true))
                            {
                                Log.Out(string.Format("[PrismaCore] Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
                            }
                        }
                        catch { continue; }

                    }

                    world.m_ChunkManager.ResendChunksToClients(tmpHash);

                    if (DynamicMeshManager.Instance != null)
                    {
                        using (HashSetLong.Enumerator enumerator = tmpHash.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                long key7 = enumerator.Current;
                                DynamicMeshManager.Instance.AddChunk(key7, true, true, null);
                            }
                        }
                    }

                    chunkProviderGenerateWorld.SaveAll();
                    chunkCache.Clear();
                    chunkProviderGenerateWorld.ClearCaches();
                    progress += 1;

                    //GameManager.Instance.SaveWorld();

                    Log.Out($"[PrismaCore] Chunks of map partition {progress}/16 have been reset to RWG default.");
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }
            }

            //reset all sleepervolumes on map
            Log.Out("[PrismaCore] Started resetting sleepervolumes.");
            World world2 = GameManager.Instance.World;
            int sleeperVolumeCount = world2.sleeperVolumes.Count;
            for (int i = 0; i < sleeperVolumeCount; i++)
            {
                SleeperVolume sleeperVolume = world2.GetSleeperVolume(i);
                if (sleeperVolume != null)
                {
                    sleeperVolume.DespawnAndReset(world2);
                }
            }
            Log.Out("[PrismaCore] Resetting sleepervolumes done.");

            Log.Out("[PrismaCore] All unclaimed chunks on map have been reset to RWG default and sleepers have been reset. Server reboot highly recommended!");
        }

        private void KickAll()
        {
            ReadOnlyCollection<ClientInfo> list = ConnectionManager.Instance.Clients.List;
            for (int i = 0; i < list.Count; i++)
            {
                ClientInfo clientInfo = list[i];
                GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), PrismaCoreStrings.Instance.AdvClaims_Reset_KickMessage));
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
    }

    public static class Ext
    {
        public static IList<HashSet<T>>
        Divide<T>(this HashSet<T> hashset, int divisions)
        {
            if (hashset == null)
                throw new ArgumentNullException("hashset");

            if (divisions <= 0)
                throw new ArgumentOutOfRangeException("divisions");

            HashSet<T>[] sets = new HashSet<T>[divisions];

            for (int i = 0; i < sets.Length; i++)
                sets[i] = new HashSet<T>();

            int capacity = hashset.Count / divisions;
            int remainder = hashset.Count % divisions;
            int itemCount = 0;
            int setIndex = 0;

            foreach (T item in hashset)
            {
                sets[setIndex].Add(item);
                itemCount++;

                if (itemCount >= capacity)
                {
                    if (setIndex < remainder && itemCount == capacity)
                        continue;

                    setIndex++;
                    itemCount = 0;
                }
            }
            return Array.AsReadOnly(sets);
        }
    }
}