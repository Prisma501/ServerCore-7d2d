using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class RemoveLandProtection2 : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "removes the association of a land protection block without players needing to be near";
        }

        public override string getHelp()
        {
            return "Usage:" +
                   "  1. rlp2 <steamid>\n" +
                   "  2. rlp2 <x> <y> <z>\n" +
                   "  3. rlp2 nearby [length]\n" +
                   "  4. rlp2 cleanup\n" +
                   "1. Remove all land claims owned by the user with the given SteamID\n" +
                   "2. Remove only the claim block on the exactly given block position\n" +
                   "3. Remove all claims in a square with edge length of 64 (or the optionally specified size) around the executing player\n" +
                   "4. Remove all expired landclaims from the world";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-removelandprotection2", "removelandprotection2", "rlp2" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_senderInfo.RemoteClientInfo != null)
                {
                    if (_params.Count >= 1 && _params[0].EqualsCaseInsensitive("nearby"))
                    {
                        _params.Add(_senderInfo.RemoteClientInfo.entityId.ToString());
                    }
                }

                if (_params.Count > 0 && _params[0].EqualsCaseInsensitive("nearby"))
                {
                    try
                    {
                        int closeToDistance = 32;
                        if (_params.Count == 3)
                        {
                            if (!int.TryParse(_params[1], out closeToDistance))
                            {
                                SdtdConsole.Instance.Output("ERR: Given length is not an integer!");
                                return;
                            }
                            closeToDistance /= 2;
                        }
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[_params.Count - 1]);
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                        Vector3i closeTo = new Vector3i(ep.GetPosition());

                        Dictionary<Vector3i, PersistentPlayerData> allLandClaims = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
                        List<Vector3i> claimsToRemove = new List<Vector3i>();

                        foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allLandClaims)
                        {
                            if (Math.Abs(kvp.Key.x - closeTo.x) <= closeToDistance && Math.Abs(kvp.Key.z - closeTo.z) <= closeToDistance)
                            {
                                claimsToRemove.Add(kvp.Key);
                            }
                        }

                        try
                        {
                            foreach (Vector3i claim in claimsToRemove)
                            {
                                List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                                BlockChangeInfo bci = new BlockChangeInfo(claim, new BlockValue(0), true, false);
                                changes.Add(bci);
                                GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(claim);

                                try
                                {
                                    GameManager.Instance.SetBlocksRPC(changes);
                                }
                                catch { GameManager.Instance.SetBlocksRPC(changes); }
                                finally { }
                            }
                        }
                        catch (Exception e)
                        {
                            SdtdConsole.Instance.Output("Error removing claims");
                            Log.Out("Error in RemoveLandProtection2.Run: " + e);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        SdtdConsole.Instance.Output("Error getting current player's position");
                        Log.Out("Error in RemoveLandProtection2.Run: " + e);
                        return;
                    }
                }
                else if (_params.Count > 0 && _params[0].EqualsCaseInsensitive("cleanup"))
                {
                    //cleanup all expired landclaims
                    RemoveAllInactiveClaims();
                }
                else if (_params.Count == 1)
                {
                    RemoveById(_params[0]);
                }
                else if (_params.Count == 3)
                {
                    int x = int.MinValue;
                    int.TryParse(_params[0], out x);
                    int y = int.MinValue;
                    int.TryParse(_params[1], out y);
                    int z = int.MinValue;
                    int.TryParse(_params[2], out z);

                    if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }

                    RemoveByPosition(x, y, z);

                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: Illegal parameters");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in RemoveLandProtection2.Run: " + e);
            }
        }

        private void RemoveAllInactiveClaims()
        {
            Dictionary<Vector3i, PersistentPlayerData> allLandClaims = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
            List<string> claimsToRemove = new List<string>();

            foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allLandClaims)
            {
                if (!GameManager.Instance.World.IsLandProtectionValidForPlayer(GameManager.Instance.GetPersistentPlayerList().GetPlayerData(kvp.Value.PlayerData.PrimaryId)))
                {
                    string steamId = Database.Instance.GetSteamIdByEOS(kvp.Value.PlayerData.PrimaryId.ToString());
                    if(!string.IsNullOrEmpty(steamId))
                        claimsToRemove.Add(steamId);
                }
            }

            foreach (string id in claimsToRemove)
            {
                RemoveById(id);
            }

            SdtdConsole.Instance.Output("All expired landclaims have been removed!");
        }


        private void RemoveById(string _id)
        {
            try
            {
                if (_id.Trim().Length != 23)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Invalid SteamId {0}", _id));
                    return;
                }

                DbPlayer dbplayer = Database.Instance.GetDbPlayer(_id.Trim().Replace("steam_", "Steam_"));

                if (dbplayer == null)
                {
                    SdtdConsole.Instance.Output($"ERR: Player with steamId {_id.Trim()} can not be found.");
                    return;
                }

                Dictionary<Vector3i, PersistentPlayerData> allLandClaims = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
                List<Vector3i> claimsToRemove = new List<Vector3i>();

                foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allLandClaims)
                {
                    if (dbplayer.EOS_Id.Equals(kvp.Value.PlayerData.PrimaryId.ToString()))
                    {
                        claimsToRemove.Add(kvp.Key);
                    }
                }

                foreach (Vector3i claim in claimsToRemove)
                {
                    if (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(claim.x), World.toChunkXZ(claim.z)) == null)
                    {
                        Vector3 chnkV = new Vector3(claim.x, 0, claim.z);

                        SdtdConsole.Instance.Output("Waiting for chunk with claimblock to load...");
                        ChunkManager.ChunkObserver cobs = null;
                        try
                        {
                            cobs = GameManager.Instance.AddChunkObserver(chnkV, false, 3, -1);
                        }
                        catch
                        {
                            SdtdConsole.Instance.Output("ERR: Failed to load chunk with claimblock");
                            continue;
                        }

                        StartThreadParameterized(claim, cobs);

                        SdtdConsole.Instance.Output("Land protection block at (" + claim.ToString() + ") removed");
                    }
                    else
                    {
                        BlockChangeInfo bci = new BlockChangeInfo(claim, new BlockValue(0), true, false);

                        List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                        changes.Add(bci);
                        GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(claim);

                        try
                        {
                            GameManager.Instance.SetBlocksRPC(changes);
                        }
                        catch { GameManager.Instance.SetBlocksRPC(changes); }
                        finally { }

                        SdtdConsole.Instance.Output("Land protection block at (" + claim.ToString() + ") removed");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private void RemoveByPosition(int x, int y, int z)
        {
            try
            {
                Vector3i v = new Vector3i(x, y, z);

                PersistentPlayerList ppl = GameManager.Instance.GetPersistentPlayerList();

                Dictionary<Vector3i, PersistentPlayerData> d = ppl.m_lpBlockMap;
                if (d == null || !d.ContainsKey(v))
                {
                    SdtdConsole.Instance.Output("ERR: No land protection block at the given position or not a valid position.");
                    return;
                }

                if (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(v.x), World.toChunkXZ(v.z)) == null)
                {
                    Vector3 chnkV = new Vector3(x, 0, z);

                    SdtdConsole.Instance.Output("Waiting for chunk with claimblock to load...");
                    ChunkManager.ChunkObserver co = null;
                    try
                    {
                        co = GameManager.Instance.AddChunkObserver(chnkV, false, 3, -1);
                    }
                    catch
                    {
                        SdtdConsole.Instance.Output("ERR: Failed to load chunk with claimblock");
                        return;
                    }

                    StartThreadParameterized(v, co);
                }
                else
                {
                    BlockChangeInfo bci = new BlockChangeInfo(v, new BlockValue(0), true, false);
                    List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                    changes.Add(bci);
                    GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(v);
                    try
                    {
                        GameManager.Instance.SetBlocksRPC(changes);
                    }
                    catch { GameManager.Instance.SetBlocksRPC(changes); }
                    finally { }
                }

                SdtdConsole.Instance.Output("Land protection block at (" + v.ToString() + ") removed");

            }
            catch { }
        }

        private Thread StartThreadParameterized(Vector3i vec, ChunkManager.ChunkObserver cobs)
        {
            var t = new Thread(() => ChunkLoadParameterized(vec, cobs))
            {
                IsBackground = true
            };
            t.Start();
            return t;
        }

        private static void ChunkLoadParameterized(Vector3i vec, ChunkManager.ChunkObserver cobs)
        {
            while (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(vec.x), World.toChunkXZ(vec.z)) == null)
            {
                Thread.Sleep(1000);
            }

            BlockChangeInfo bci = new BlockChangeInfo(vec, new BlockValue(0), true, false);

            List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
            changes.Add(bci);
            GameManager.Instance.GetPersistentPlayerList().RemoveLandProtectionBlock(vec);

            try
            {
                GameManager.Instance.SetBlocksRPC(changes);
            }
            catch { GameManager.Instance.SetBlocksRPC(changes); }
            finally { }

            try
            {
                GameManager.Instance.RemoveChunkObserver(cobs);
            }
            catch { Log.Out("error in removing chunkobserver: rlp2 !"); }
        }
    }
}
