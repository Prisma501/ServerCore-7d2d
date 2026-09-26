using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class ResetRWGPrefabs : ConsoleCmdAbstract
    {
        private static Thread thKillServer;

        public override string getDescription()
        {
            return "Reset all RWG prefabs. Exclude claimed prefabs in PrismaCoreSettings.xml";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "   1. rrp [tradersonly] [kicklockreboot]\n" +
                   "1. Reset all RWG prefabs live on the map.\n" +
                   "   Use parameter kicklockreboot to kick online players and lock server during reset. Reboots server when done.\n" +
                   "   Exclude claimed prefabs in PrismaCoreSettings.xml (ResetPrefabs_ExcludeClaimedPrefabs).";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-resetrwgprefabs", "resetrwgprefabs", "rrp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
        }

        public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("tradersonly"))
            {
                Log.Out("[PrismaCore] Started reset of traders RWG prefabs only.");
                PrefabsReset(true);
                yield break;
            }
            else if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("kicklockreboot"))
            {
                Log.Out("[PrismaCore] Started reset of all RWG prefabs.");

                KickAll();

                Reset.resetActive = true;

                PrefabsReset();

                thKillServer = new Thread(new ThreadStart(KillServer));
                thKillServer.IsBackground = true;
                thKillServer.Start();
                yield break;

            }
            else if (_params.Count == 2)
            {
                Log.Out("[PrismaCore] kicklockreboot parameter not supported for trader only resets.");
            }
            else
            {
                Log.Out("[PrismaCore] Started reset of all RWG prefabs.");
                PrefabsReset();
                yield break;
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

        public static void PrefabsReset(bool tradersOnly = false)
        {
            try
            {
                RegionReset.LoadPrefabExceptons();
                DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();

                if (dynamicPrefabDecorator != null)
                {
                    List<PrefabInstance> allPrefabs = new List<PrefabInstance>();
                    dynamicPrefabDecorator.GetWorldPrefabs(allPrefabs);
                    HashSetLong allChunks = new HashSetLong();
                    int count = 0;
                    World world = GameManager.Instance.World;

                    var prefabChunks = new HashSetLong();//fab.GetOccupiedChunks();

                    for (int i = 0; i < allPrefabs.Count; i++)
                    {
                        PrefabInstance fab = allPrefabs[i];
                        if (fab != null)
                        {
                            if (!fab.location.Name.ContainsCaseInsensitive("trader") && tradersOnly)
                            {
                                continue;
                            }

                            var pcenter = fab.GetCenterXZ();

                            if (RegionReset.lstPrefabExceptions.ContainsCaseInsensitive(fab.name))
                            {
                                Log.Out($"[PrismaCore] Prefab {fab.name} has been skipped from reset by unique name @ (x, z) ({(int)Math.Floor(pcenter.x)}, {(int)Math.Floor(pcenter.y)})!");
                                continue;
                            }
                            if (RegionReset.lstPrefabExceptions.ContainsCaseInsensitive(fab.location.Name))
                            {
                                Log.Out($"[PrismaCore] Prefab {fab.name} has been skipped from reset by prefab type ({fab.location.Name}) @ (x, z) ({(int)Math.Floor(pcenter.x)}, {(int)Math.Floor(pcenter.y)})!");
                                continue;
                            }

                            //bool containsBed = fab.CheckForAnyPlayerHome(GameManager.Instance.World) != GameUtils.EPlayerHomeType.None;

                            bool skipPrefab = false;

                            //if (containsBed)
                            //{
                            int int2 = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
                            int num2 = int2 / 2;
                            Vector3i BoxMin = fab.boundingBoxPosition;
                            Vector3i BoxMax = fab.boundingBoxPosition + fab.boundingBoxSize;

                            Dictionary<Vector3i, PersistentPlayerData> allBlocks = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;

                            List<Vector3i> lpb = new List<Vector3i>();

                            if (allBlocks != null)
                            {
                                foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allBlocks)
                                {
                                    Vector3i pos = kvp.Key;

                                    Vector3i p = new Vector3i();
                                    p.x = pos.x - num2;
                                    p.z = pos.z - num2;

                                    if (p.x <= BoxMax.x && p.x + int2 >= BoxMin.x && p.z <= BoxMax.z && p.z + int2 >= BoxMin.z)
                                    {
                                        if (!PrismaCoreSettings.Instance.ResetPrefabs_ExcludeClaimedPrefabs)
                                        {
                                            if (!lpb.Contains(pos))
                                            {
                                                lpb.Add(pos);
                                            }
                                        }
                                        else
                                        {
                                            skipPrefab = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (skipPrefab) continue;

                            if (lpb.Count > 0)
                            {
                                List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                                CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                foreach (Vector3i vec in lpb)
                                {
                                    BlockChangeInfo bci = new BlockChangeInfo(vec, new BlockValue(0), true, false);
                                    changes.Add(bci);
                                    //GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(vec);
                                    GameManager.Instance.persistentPlayers.RemoveLandProtectionBlock(vec);
                                    //SdtdConsole.Instance.ExecuteAsync($"rlp2 {vec.x} {vec.y} {vec.z}", iConsole);
                                }

                                try
                                {
                                    GameManager.Instance.SetBlocksRPC(changes);
                                }
                                catch { GameManager.Instance.SetBlocksRPC(changes); }
                                finally { }
                            }
                            //}

                            //int posXmin = fab.boundingBoxPosition.x;
                            //int posXmax = fab.boundingBoxPosition.x + fab.boundingBoxSize.x;
                            //int posZmin = fab.boundingBoxPosition.z;
                            //int posZmax = fab.boundingBoxPosition.z + fab.boundingBoxSize.z;

                            //Vector2i vector2i = new Vector2i((posXmin <= posXmax) ? posXmin : posXmax, (posZmin <= posZmax) ? posZmin : posZmax);
                            //Vector2i vector2i2 = new Vector2i((posXmin <= posXmax) ? posXmax : posXmin, (posZmin <= posZmax) ? posZmax : posZmin);

                            //vector2i = World.toChunkXZ(vector2i);
                            //vector2i2 = World.toChunkXZ(vector2i2);

                            //for (int k = vector2i.x; k <= vector2i2.x; k++)
                            //{
                            //    for (int l = vector2i.y; l <= vector2i2.y; l++)
                            //    {
                            //        //Log.Out($"Fab:{fab.name} ChunkX:{k} ChunkZ:{l} BoundingXMin:{fab.boundingBoxPosition.x} BoundingXMax:{fab.boundingBoxPosition.x + fab.boundingBoxSize.x} BoundingZMin:{fab.boundingBoxPosition.z} BoundingZMax:{fab.boundingBoxPosition.z + fab.boundingBoxSize.z}");
                            //        long ckey = WorldChunkCache.MakeChunkKey(k, l);

                            //        prefabChunks.Add(ckey);
                            //    }
                            //}

                            prefabChunks = fab.GetOccupiedChunks();

                            foreach (long key in prefabChunks)
                            {
                                allChunks.Add(key);
                            }

                            count += 1;

                            for (int j = 0; j < fab.sleeperVolumes.Count; j++)
                            {
                                Vector3i startPos = fab.prefab.SleeperVolumeList[j].startPos;
                                Vector3i size = fab.prefab.SleeperVolumeList[j].size;
                                int num = GameManager.Instance.World.FindSleeperVolume(fab.boundingBoxPosition + startPos, fab.boundingBoxPosition + startPos + size);
                                if (num != -1)
                                {
                                    GameManager.Instance.World.GetSleeperVolume(num).DespawnAndReset(GameManager.Instance.World);
                                }
                            }
                            for (int l = 0; l < fab.prefab.TriggerVolumeList.Count; l++)
                            {
                                Vector3i startPos2 = fab.prefab.TriggerVolumeList[l].startPos;
                                Vector3i size2 = fab.prefab.TriggerVolumeList[l].size;
                                int num3 = GameManager.Instance.World.FindTriggerVolume(fab.boundingBoxPosition + startPos2, fab.boundingBoxPosition + startPos2 + size2);
                                if (num3 != -1)
                                {
                                    world.GetTriggerVolume(num3).Reset();
                                }
                            }
                        }
                    }

                    ChunkCluster chunkCache = world.ChunkCache;
                    ChunkProviderGenerateWorld chunkProviderGenerateWorld = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;

                    if (chunkProviderGenerateWorld != null)
                    {
                        //for (int k = posXmin; k <= posXmax; k++)
                        //{
                        //    for (int l = posZmin; l <= posZmax; l++)
                        //    {
                        //        if((k % 16) == 0 && (l % 16) == 0)
                        //        {
                        //            Log.Out($"GOT HIM!!!!! {fab.name} ChunkX:{l} ChunkZ:{k} BoundingXMin:{fab.boundingBoxPosition.x} BoundingXMax:{fab.boundingBoxPosition.x + fab.boundingBoxSize.x} BoundingZMin:{fab.boundingBoxPosition.z} BoundingZMax:{fab.boundingBoxPosition.z + fab.boundingBoxSize.z}");
                        //        }
                        //        prefabChunks.Add(WorldChunkCache.MakeChunkKey(k, l));
                        //    }
                        //}

                        //GameManager.Instance.ResetWindowsAndLocksByChunks(allChunks);
                        LockManager.Instance.ForceUnlockByChunk(allChunks);
                        chunkProviderGenerateWorld.RemoveChunks(allChunks);
                        foreach (long key in allChunks)
                        {
                            //if (!done.Contains(key))
                            //{
                            try
                            {
                                if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, key, true))
                                {
                                    SdtdConsole.Instance.Output(string.Format("Resetting prebabchunk failed at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
                                }
                                //done.Add(key);
                            }
                            catch { continue; }
                            //}
                        }

                        //Log.Out($"[PrismaCore] Prefab {fab.name} has been reset to RWG default state @ (x, z) ({(int)Math.Floor(pcenter.x)}, {(int)Math.Floor(pcenter.y)})");
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"RWG Prefabs could not be reset! Chunkprovider for generated world not available.");
                        return;
                    }

                    GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(allChunks);

                    if (DynamicMeshManager.Instance != null)
                    {
                        using (HashSetLong.Enumerator enumerator = allChunks.GetEnumerator())
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

                    //GameManager.Instance.SaveWorld();

                    if (tradersOnly)
                    {
                        Log.Out($"{count} Trader RWG prefabs have been reset to RWG default.");
                    }
                    else
                    {
                        Log.Out($"{count} RWG prefabs have been reset to RWG default and sleepers/triggervolumes have been reset. Server reboot highly recommended!");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in ResetRWGPrefabs.Run: " + e);
            }
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
    }
}
