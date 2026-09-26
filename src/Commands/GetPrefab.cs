using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class GetPrefab : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Get info/manage the RWG prefab you are standing in.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. getprefab [steamId/entityId/name] reset\n" +
                   "  2. getprefab exclude\n" +
                   "  3. getprefab exclude type\n" +
                   "  4. getprefab\n" +
                   "1. Reset the RWG prefab you are standing in (or [steamId/entityId/name] is standing in).\n" +
                   "2. Exclude the RWG prefab you are standing in from resets by unique name\n" +
                   "3. Exclude the RWG prefab you are standing in from resets by type\n" +
                   "4. Get info on the RWG prefab you are standing in.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-getprefab", "getprefab" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                ClientInfo ci;

                if (_params.Count == 2 && _params.ContainsCaseInsensitive("reset"))
                {
                    ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Unable to get your position");
                        return;
                    }
                }
                else
                {
                    ci = _senderInfo.RemoteClientInfo;
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Unable to get your position");
                        return;
                    }
                }

                EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                if (ep == null)
                {
                    SdtdConsole.Instance.Output("ERR: Unable to get your position");
                    return;
                }

                bool containsBed;

                PrefabInstance prefabFromWorldPosInside = ep.prefab;

                if (prefabFromWorldPosInside != null)
                {
                    containsBed = prefabFromWorldPosInside.CheckForAnyPlayerHome(GameManager.Instance.World) != GameUtils.EPlayerHomeType.None;

                    if (_params.ContainsCaseInsensitive("reset"))
                    {
                        //remove claimblocks if present
                        if (containsBed)
                        {
                            int int2 = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"));
                            int num2 = int2 / 2;
                            Vector3i BoxMin = prefabFromWorldPosInside.boundingBoxPosition;
                            Vector3i BoxMax = prefabFromWorldPosInside.boundingBoxPosition + prefabFromWorldPosInside.boundingBoxSize;

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
                                        if (!lpb.Contains(pos))
                                        {
                                            lpb.Add(pos);
                                        }
                                    }
                                }
                            }

                            if (lpb.Count > 0)
                            {
                                List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                                foreach (Vector3i vec in lpb)
                                {
                                    BlockChangeInfo bci = new BlockChangeInfo(vec, new BlockValue(0), true, false);
                                    changes.Add(bci);
                                    GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(vec);
                                }

                                try
                                {
                                    GameManager.Instance.SetBlocksRPC(changes);
                                }
                                catch { GameManager.Instance.SetBlocksRPC(changes); }
                            }

                            int deadsize = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BedrollDeadZoneSize"));
                            Vector3i expand = new Vector3i(deadsize, deadsize, deadsize);
                            Vector3i BoxMinNew = BoxMin - expand;
                            Vector3i BoxMaxNew = BoxMax + expand;
                            double bedrollexpireTime = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;

                            foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
                            {
                                if (keyValuePair.Value.HasBedrollPos)
                                {
                                    Vector3i bedrollPos = keyValuePair.Value.BedrollPos;
                                    if (bedrollPos.x >= BoxMinNew.x && bedrollPos.x < BoxMaxNew.x && bedrollPos.y >= BoxMinNew.y && bedrollPos.y < BoxMaxNew.y && bedrollPos.z >= BoxMinNew.z && bedrollPos.z < BoxMaxNew.z && keyValuePair.Value.OfflineHours < bedrollexpireTime)
                                    {
                                        keyValuePair.Value.ClearBedroll();
                                        GameManager.Instance.GetPersistentPlayerList().SpawnPointRemoved(bedrollPos);

                                        BlockChangeInfo bci = new BlockChangeInfo(bedrollPos, new BlockValue(0), true, false);

                                        List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                                        changes.Add(bci);

                                        try
                                        {
                                            GameManager.Instance.SetBlocksRPC(changes);
                                        }
                                        catch { GameManager.Instance.SetBlocksRPC(changes); }
                                        finally { }

                                        SdtdConsole.Instance.Output("Bed(roll) at (" + bedrollPos.ToString() + ") deactivated");
                                    }
                                }
                            }
                        }

                        GameManager.Instance.StartCoroutine(this.onPerformAction(prefabFromWorldPosInside));

                        return;
                    }

                    if (_params.ContainsCaseInsensitive("exclude") && _params.ContainsCaseInsensitive("type"))
                    {

                        var prefabContent = File.ReadAllLines(RegionReset.PrefabExceptionFile);
                        List<string> lstPrefabExceptions = new List<string>(prefabContent);

                        if (!lstPrefabExceptions.ContainsCaseInsensitive(prefabFromWorldPosInside.location.Name))
                        {
                            lstPrefabExceptions.Add(prefabFromWorldPosInside.location.Name);
                            using (TextWriter tw = new StreamWriter(RegionReset.PrefabExceptionFile))
                            {
                                foreach (string s in lstPrefabExceptions)
                                {
                                    if (!string.IsNullOrEmpty(s))
                                        tw.WriteLine(s);
                                }
                            }

                            SdtdConsole.Instance.Output($"RWG Prefab type {prefabFromWorldPosInside.location.Name} has been added to ResetPrefabs_Exceptions.txt");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output($"RWG Prefab type {prefabFromWorldPosInside.location.Name} is already excluded from reset.");
                        }

                        return;
                    }

                    if (_params.ContainsCaseInsensitive("exclude"))
                    {

                        var prefabContent = File.ReadAllLines(RegionReset.PrefabExceptionFile);
                        List<string> lstPrefabExceptions = new List<string>(prefabContent);

                        if (!lstPrefabExceptions.ContainsCaseInsensitive(prefabFromWorldPosInside.name))
                        {
                            lstPrefabExceptions.Add(prefabFromWorldPosInside.name);
                            using (TextWriter tw = new StreamWriter(RegionReset.PrefabExceptionFile))
                            {
                                foreach (string s in lstPrefabExceptions)
                                {
                                    if (!string.IsNullOrEmpty(s))
                                        tw.WriteLine(s);
                                }
                            }

                            SdtdConsole.Instance.Output($"RWG Prefab {prefabFromWorldPosInside.name} has been added to ResetPrefabs_Exceptions.txt");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output($"RWG Prefab {prefabFromWorldPosInside.name} is already excluded from reset.");
                        }

                        return;
                    }

                    bool questPoi = false;

                    if (prefabFromWorldPosInside.prefab.HasQuestTag() && prefabFromWorldPosInside.prefab.DifficultyTier > 0)
                    {
                        questPoi = true;
                    }


                    SdtdConsole.Instance.Output($"Prefab Name: {prefabFromWorldPosInside.name}");
                    SdtdConsole.Instance.Output($"Prefab FileName: {prefabFromWorldPosInside.location.Name}");
                    Vector2 center = prefabFromWorldPosInside.GetCenterXZ();
                    SdtdConsole.Instance.Output($"Prefab Center (x, z): {(int)Math.Floor(center.x)}, {(int)Math.Floor(center.y)}");
                    SdtdConsole.Instance.Output($"Prefab Size (x, y, z): {prefabFromWorldPosInside.prefab.size.x},  {prefabFromWorldPosInside.prefab.size.y},  {prefabFromWorldPosInside.prefab.size.z}");
                    SdtdConsole.Instance.Output($"Prefab contains a player bed/lcb: {containsBed}");
                    SdtdConsole.Instance.Output($"Quest POI: {questPoi}");
                    SdtdConsole.Instance.Output($"Number of sleepervolumes in prefabfile on disk: {prefabFromWorldPosInside.sleeperVolumes.Count}");
                    return;
                }

                SdtdConsole.Instance.Output("No RWG prefab found on your location.");
            }
            catch (Exception e)
            {
                Log.Out("Error in GetPrefab.Run: " + e);
            }
        }

        private IEnumerator onPerformAction(PrefabInstance prefabFromWorldPosInside)
        {
            World world = GameManager.Instance.World;
            List<PrefabInstance> prefabInstances = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsIntersecting(prefabFromWorldPosInside);
            SdtdConsole.Instance.Output("RWG Prefab you are standing in has been reset to RWG default and sleepers/triggervolumes have been reset.");
            yield return world.ResetPOIS(prefabInstances, QuestEventManager.manualResetTag, -1, null, null);
        }
    }
}
