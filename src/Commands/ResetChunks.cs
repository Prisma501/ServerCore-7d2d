using System.Collections;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ResetChunks : ConsoleCmdAbstract
    {

        private static Dictionary<int, Vector3i> location = new Dictionary<int, Vector3i>();

        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. resetchunks p1\n" +
                "  2. resetchunks p2\n" +
                "  3. resetchunks radius <radius> <steamId/entityId/Name>\n" +
                "1. Store your position to be used on method 2.\n" +
                "2. Reset chunks from position stored on method 1 until your current location(p2).\n" +
                "3. Reset chunks within boundaries on <radius> distance from <steamId/entityId/Name> position.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-resetchunks", "resetchunks" };
        }

        public override string getDescription()
        {
            return "Reset chunks to RWG default.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
        }

        public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            //try {
            if (_params.Count != 1 && _params.Count != 3)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1 or 3, found " + _params.Count + ".");
                SdtdConsole.Instance.Output(GetHelp());
                yield break;
            }
            else
            {
                int x1 = int.MinValue;
                int y1 = int.MinValue;
                int z1 = int.MinValue;

                int x2 = int.MinValue;
                int y2 = int.MinValue;
                int z2 = int.MinValue;

                if (_params.Count == 1)
                {
                    if (_params[0].ToLower().Equals("p1"))
                    {
                        ClientInfo ci = _senderInfo.RemoteClientInfo;
                        if (ci == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Unable to get your position");
                            yield break;

                        }
                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                        if (ep == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Unable to get your position");
                            yield break;

                        }
                        if (location.ContainsKey(ci.entityId))
                        {
                            location.Remove(ci.entityId);
                        }
                        location.Add(ci.entityId, new Vector3i(ep.GetBlockPosition().x, ep.GetBlockPosition().y, ep.GetBlockPosition().z));
                        SdtdConsole.Instance.Output("Stored position: " + ep.GetBlockPosition().x + " " + ep.GetBlockPosition().y + " " + ep.GetBlockPosition().z);
                        yield break;
                    }

                    if (_params[0].ToLower().Equals("p2"))
                    {
                        ClientInfo ci2 = _senderInfo.RemoteClientInfo;
                        if (ci2 == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Unable to get your position");
                            yield break;

                        }
                        EntityPlayer ep2 = GameManager.Instance.World.Players.dict[ci2.entityId];
                        if (ep2 == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Unable to get your position");
                            yield break;

                        }
                        if (!location.ContainsKey(ci2.entityId))
                        {
                            SdtdConsole.Instance.Output("ERR: There isnt any stored location. Use method 1 to store a position.");
                            SdtdConsole.Instance.Output(GetHelp());
                            yield break;
                        }
                        Vector3i storedPos;
                        if (!location.TryGetValue(ci2.entityId, out storedPos))
                        {
                            SdtdConsole.Instance.Output("ERR: No stored position found. Do resetchunks p1 first.");
                            yield break;
                        }

                        x1 = storedPos.x;
                        y1 = storedPos.y;
                        z1 = storedPos.z;

                        x2 = ep2.GetBlockPosition().x;
                        y2 = ep2.GetBlockPosition().y;
                        z2 = ep2.GetBlockPosition().z;

                        location.Remove(ci2.entityId);
                    }
                }
                else if (_params.Count == 3)
                {
                    if (!_params[0].ToLower().EqualsCaseInsensitive("radius"))
                    {
                        SdtdConsole.Instance.Output($"ERR: Invalid subcommand given. Expected: radius. Parameter: {_params[0]}");
                        yield break;
                    }

                    if (!int.TryParse(_params[1], out int r))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The value for radius is not a valid integer. Parameter: {0}", _params[1]));
                        yield break;
                    }

                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[2]);
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Unable to get player position.");
                        yield break;
                    }

                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    if (ep == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Unable to get player position");
                        yield break;

                    }

                    Vector3i pos = ep.GetBlockPosition();

                    x1 = pos.x - r;
                    x2 = pos.x + r;
                    z2 = pos.z + r;
                    z1 = pos.z - r;
                }

                if (x1 == int.MinValue || z1 == int.MinValue || x2 == int.MinValue || z2 == int.MinValue)
                {
                    SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                    SdtdConsole.Instance.Output(GetHelp());
                    yield break;
                }

                Vector2i vector2i = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
                Vector2i vector2i2 = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);
                if (vector2i2.x - vector2i.x > 16384 || vector2i2.y - vector2i.y > 16384)
                {
                    SdtdConsole.Instance.Output("Area too big to reset.");
                    yield break;
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
                    //yield return GameManager.Instance.ResetWindowsAndLocksByChunks(hashSetLong);
                    ;
                    LockManager.Instance.ForceUnlockByChunk(hashSetLong);
                    chunkProviderGenerateWorld.RemoveChunks(hashSetLong);
                    foreach (long key in hashSetLong)
                    {
                        try
                        {
                            if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, key, true))
                            {
                                SdtdConsole.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
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

                    //GameManager.Instance.SaveWorld();
                    chunkProviderGenerateWorld.SaveAll();
                    //chunkCache.Clear();
                    //chunkProviderGenerateWorld.ClearCaches();

                    SdtdConsole.Instance.Output(string.Format("Chunks from {0}/{1} to {2}/{3} have been reset (chunk coordinates {4} to {5}).", new object[]
                    {
                            x1,
                            z1,
                            x2,
                            z2,
                            vector2i,
                            vector2i2
                    }));

                    yield break;
                }
                else
                {
                    SdtdConsole.Instance.Output("Chunks could not be reset! Chunkprovider for generated world not available.");
                }
            }
            //} catch (Exception e) {
            //	Log.Out ("Error in ResetChunks.Run: " + e);
            //}
        }
    }
}
