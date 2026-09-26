using Platform.EOS;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class DeactivateBedroll : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Deactivate a players bed(roll).";
        }

        public override string getHelp()
        {
            return "Usage:" +
                   "  1. db <steamid/XblId>\n" +
                   "Deactivate the active bed (roll) of a player by SteamID";

        }

        public override string[] getCommands()
        {
            return new[] { "pc-db", "db", "deactivatebed" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 1)
                {
                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0].Trim());

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

                        if (dbplayer == null)
                        {
                            SdtdConsole.Instance.Output($"ERR: Offline Player with steamId/XblId {_params[0]} can not be found.");
                            return;
                        }

                        PlatformUserIdentifierAbs steamid = new UserIdentifierEos(dbplayer.EOS_Id.Replace("EOS_", string.Empty));

                        if (steamid == null)
                        {
                            SdtdConsole.Instance.Output($"ERR: Player with steamId {_params[0]} can not be found.");
                            return;
                        }

                        RemoveById(steamid);
                    }
                    else
                    {
                        RemoveById(ci.CrossplatformId);
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: Invalid parameters");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in DeactivateBedroll.Run: " + e);
            }
        }

        private void RemoveById(PlatformUserIdentifierAbs _id)
        {
            try
            {
                foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
                {
                    if (keyValuePair.Key.ToString() == _id.ToString())
                    {
                        if (keyValuePair.Value.HasBedrollPos)
                        {
                            Vector3i bedrollPos = keyValuePair.Value.BedrollPos;


                            if (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(bedrollPos.x), World.toChunkXZ(bedrollPos.z)) == null)
                            {
                                Vector3 chnkV = new Vector3(bedrollPos.x, 0, bedrollPos.z);

                                SdtdConsole.Instance.Output("Waiting for chunk with bed(roll) to load...");
                                ChunkManager.ChunkObserver cobs = null;
                                try
                                {
                                    cobs = GameManager.Instance.AddChunkObserver(chnkV, false, 3, -1);
                                }
                                catch
                                {
                                    SdtdConsole.Instance.Output("ERR: Failed to load chunk with bed(roll)");
                                    continue;
                                }

                                StartThreadParameterized(keyValuePair.Value, cobs);

                                SdtdConsole.Instance.Output("Bed(roll) at (" + bedrollPos.ToString() + ") deactivated");
                            }
                            else
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
            }
            catch (Exception e)
            {
                Log.Out("Error in DeactivateBedroll.removeById: " + e);
            }
        }

        private Thread StartThreadParameterized(PersistentPlayerData pd, ChunkManager.ChunkObserver cobs)
        {
            var t = new Thread(() => ChunkLoadParameterized(pd, cobs))
            {
                IsBackground = true
            };
            t.Start();
            return t;
        }

        private static void ChunkLoadParameterized(PersistentPlayerData pd, ChunkManager.ChunkObserver cobs)
        {
            Vector3i bedrollPos = pd.BedrollPos;

            while (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(bedrollPos.x), World.toChunkXZ(bedrollPos.z)) == null)
            {
                Thread.Sleep(1000);
            }

            pd.ClearBedroll();
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

            try
            {
                GameManager.Instance.RemoveChunkObserver(cobs);
            }
            catch { Log.Out("error in removing chunkobserver: deactivate bed !"); }
        }
    }
}
