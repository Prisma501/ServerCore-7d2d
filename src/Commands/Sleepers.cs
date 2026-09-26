using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class Sleepers : ConsoleCmdAbstract
    {
        private static Dictionary<int, Vector3i> location = new Dictionary<int, Vector3i>();

        public override string getDescription()
        {
            return "Remove sleepervolumes from world";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. sleepers p1\n" +
                   "  2. sleepers p2\n" +
                   "  3. sleepers p2 remove\n" +
                   "  4. sleepers p2 reset\n" +
                   "  5. sleepers x1 z1 x2 z2 reset\n" +
                   "1. Store position for use with p2\n" +
                   "2. Show sleepervolume count from stored pos p1 to p2\n" +
                   "3. Remove sleepervolumes from stored pos p1 to p2\n" +
                   "4. Despawn and reset sleepervolumes from stored pos p1 to p2\n" +
                   "5. Despawn and reset sleepervolumes from coordinates x1,z1 to x2,z2";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-sleepers", "sleepers" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                int x1 = int.MinValue;
                int y1 = int.MinValue;
                int z1 = int.MinValue;

                int x2 = int.MinValue;
                int y2 = int.MinValue;
                int z2 = int.MinValue;

                if (_params.Count == 5 && _params[4].EqualsCaseInsensitive("reset"))
                {
                    if (!Int32.TryParse(_params[0], out x1))
                    {
                        SdtdConsole.Instance.Output("x1 is not a valid integer.");
                        return;
                    }

                    if (!Int32.TryParse(_params[1], out z1))
                    {
                        SdtdConsole.Instance.Output("z1 is not a valid integer.");
                        return;
                    }

                    if (!Int32.TryParse(_params[2], out x2))
                    {
                        SdtdConsole.Instance.Output("x2 is not a valid integer.");
                        return;
                    }

                    if (!Int32.TryParse(_params[3], out z2))
                    {
                        SdtdConsole.Instance.Output("z2 is not a valid integer.");
                        return;
                    }

                    if (x2 < x1)
                    {
                        int val = x1;
                        x1 = x2;
                        x2 = val;
                    }

                    if (z2 < z1)
                    {
                        int val = z1;
                        z1 = z2;
                        z2 = val;
                    }

                    World world = GameManager.Instance.World;
                    if (world != null)
                    {
                        int sleeperVolumeCount = world.sleeperVolumes.Count;
                        for (int i = 0; i < sleeperVolumeCount; i++)
                        {
                            SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
                            if (sleeperVolume != null)
                            {
                                if (sleeperVolume.Center.x > x1 && sleeperVolume.Center.z > z1 && sleeperVolume.Center.x < x2 && sleeperVolume.Center.z < z2)
                                {
                                    sleeperVolume.DespawnAndReset(world);
                                }
                            }
                        }
                    }

                    SdtdConsole.Instance.Output($"All sleepervolumes have been despawned and reset from area(x z) {x1} {z1} to {x2} {z2}");
                    return;
                }

                ClientInfo ci = _senderInfo.RemoteClientInfo;
                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Unable to get your position");
                    return;
                }

                EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                if (ep == null)
                {
                    SdtdConsole.Instance.Output("ERR: Unable to get your position");
                    return;
                }

                if (_params.Count == 1)
                {
                    if (_params[0].ToLower().Equals("p1"))
                    {
                        if (location.ContainsKey(ci.entityId))
                        {
                            location.Remove(ci.entityId);
                        }

                        location.Add(ci.entityId, new Vector3i(ep.GetBlockPosition().x, ep.GetBlockPosition().y, ep.GetBlockPosition().z));
                        SdtdConsole.Instance.Output("Stored position: " + ep.GetBlockPosition().x + " " + ep.GetBlockPosition().y + " " + ep.GetBlockPosition().z);
                        return;
                    }
                }

                if (_params[0].EqualsCaseInsensitive("p2") && _params.Count == 1)
                {
                    if (!location.ContainsKey(ci.entityId))
                    {
                        SdtdConsole.Instance.Output("ERR: There isnt any stored location. Use method 3 to store a position.");
                        SdtdConsole.Instance.Output(GetHelp());
                        return;
                    }
                    Vector3i storedPos;
                    location.TryGetValue(ci.entityId, out storedPos);

                    x1 = storedPos.x;
                    y1 = storedPos.y;
                    z1 = storedPos.z;

                    x2 = ep.GetBlockPosition().x;
                    y2 = ep.GetBlockPosition().y;
                    z2 = ep.GetBlockPosition().z;

                    int totalSleepers = 0;

                    Vector2i vector2iMin = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
                    Vector2i vector2iMax = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);

                    Vector2i vector2iMinC = World.toChunkXZ(vector2iMin);
                    Vector2i vector2iMaxC = World.toChunkXZ(vector2iMax);

                    HashSetLong hashSetLong = new HashSetLong();

                    GameManager instance = GameManager.Instance;
                    World world = ((instance != null) ? instance.World : null);
                    if (world == null)
                    {
                        SdtdConsole.Instance.Output("World has not been loaded!");
                        return;
                    }

                    for (int j = vector2iMinC.x; j <= vector2iMaxC.x; j++)
                    {
                        for (int k = vector2iMinC.y; k <= vector2iMaxC.y; k++)
                        {
                            Chunk chunk = (Chunk)world.GetChunkSync(j, k);
                            if (chunk != null)
                            {
                                var sleepers = chunk.GetSleeperVolumes();
                                var count = sleepers.Count;

                                if (count > 0)
                                {
                                    totalSleepers += 1;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: The blocks are to far away. Chunk not loaded on that area");
                                return;
                            }
                        }
                    }

                    SdtdConsole.Instance.Output($"There are {totalSleepers} sleepervolumes present in area(x z) {vector2iMin.x} {vector2iMin.y} to {vector2iMax.x} {vector2iMax.y}");
                    return;
                }

                if (_params.Count == 2 && _params[0].EqualsCaseInsensitive("p2") && _params[1].EqualsCaseInsensitive("remove"))
                {
                    if (!location.ContainsKey(ci.entityId))
                    {
                        SdtdConsole.Instance.Output("ERR: There isnt any stored location. Use method 3 to store a position.");
                        SdtdConsole.Instance.Output(GetHelp());
                        return;
                    }
                    Vector3i storedPos;
                    location.TryGetValue(ci.entityId, out storedPos);

                    x1 = storedPos.x;
                    y1 = storedPos.y;
                    z1 = storedPos.z;

                    x2 = ep.GetBlockPosition().x;
                    y2 = ep.GetBlockPosition().y;
                    z2 = ep.GetBlockPosition().z;

                    Vector2i vector2iMin = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
                    Vector2i vector2iMax = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);

                    Vector2i vector2iMinC = World.toChunkXZ(vector2iMin);
                    Vector2i vector2iMaxC = World.toChunkXZ(vector2iMax);

                    HashSetLong hashSetLong = new HashSetLong();

                    GameManager instance = GameManager.Instance;
                    World world = ((instance != null) ? instance.World : null);
                    if (world == null)
                    {
                        SdtdConsole.Instance.Output("World has not been loaded!");
                        return;
                    }

                    for (int j = vector2iMinC.x; j <= vector2iMaxC.x; j++)
                    {
                        for (int k = vector2iMinC.y; k <= vector2iMaxC.y; k++)
                        {

                            Chunk chunk = (Chunk)world.GetChunkSync(j, k);
                            if (chunk != null)
                            {
                                var sleepers = chunk.GetSleeperVolumes();
                                var count = sleepers.Count;

                                if (count > 0)
                                {

                                    hashSetLong.Add(chunk.Key);
                                    chunk.GetSleeperVolumes().Clear();
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: The blocks are to far away. Chunk not loaded on that area");
                                return;
                            }
                        }
                    }

                    world.m_ChunkManager.ResendChunksToClients(hashSetLong);

                    //remove spawned sleepers
                    RemoveSpawnedSleepers(vector2iMin.x, vector2iMin.y, vector2iMax.x, vector2iMax.y);
                    SdtdConsole.Instance.Output($"All sleepervolumes have been remvoved from area(x z) {vector2iMin.x} {vector2iMin.y} to {vector2iMax.x} {vector2iMax.y}");
                    return;
                }

                if (_params.Count == 2 && _params[0].EqualsCaseInsensitive("p2") && _params[1].EqualsCaseInsensitive("reset"))
                {
                    if (!location.ContainsKey(ci.entityId))
                    {
                        SdtdConsole.Instance.Output("ERR: There isnt any stored location. Use method 3 to store a position.");
                        SdtdConsole.Instance.Output(GetHelp());
                        return;
                    }
                    Vector3i storedPos;
                    location.TryGetValue(ci.entityId, out storedPos);

                    x1 = storedPos.x;
                    y1 = storedPos.y;
                    z1 = storedPos.z;

                    x2 = ep.GetBlockPosition().x;
                    y2 = ep.GetBlockPosition().y;
                    z2 = ep.GetBlockPosition().z;

                    Vector2i vector2iMin = new Vector2i((x1 <= x2) ? x1 : x2, (z1 <= z2) ? z1 : z2);
                    Vector2i vector2iMax = new Vector2i((x1 <= x2) ? x2 : x1, (z1 <= z2) ? z2 : z1);

                    Vector2i vector2iMinC = World.toChunkXZ(vector2iMin);
                    Vector2i vector2iMaxC = World.toChunkXZ(vector2iMax);

                    GameManager instance = GameManager.Instance;
                    World world = ((instance != null) ? instance.World : null);
                    if (world == null)
                    {
                        SdtdConsole.Instance.Output("World has not been loaded!");
                        return;
                    }

                    for (int j = vector2iMinC.x; j <= vector2iMaxC.x; j++)
                    {
                        for (int k = vector2iMinC.y; k <= vector2iMaxC.y; k++)
                        {

                            Chunk chunk = (Chunk)world.GetChunkSync(j, k);
                            if (chunk != null)
                            {
                                var sleepersVols = chunk.GetSleeperVolumes();
                                var count = sleepersVols.Count;

                                if (count > 0)
                                {
                                    foreach (int i in sleepersVols)
                                    {
                                        SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
                                        if (sleeperVolume != null)
                                        {
                                            sleeperVolume.DespawnAndReset(world);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: The blocks are to far away. Chunk not loaded on that area");
                                return;
                            }
                        }
                    }

                    SdtdConsole.Instance.Output($"All sleepervolumes have been despawned and reset from area(x z) {vector2iMin.x} {vector2iMin.y} to {vector2iMax.x} {vector2iMax.y}");
                    return;
                }

                SdtdConsole.Instance.Output("Invalid or invalid number of parameters.");
            }
            catch (Exception e)
            {
                Log.Out("Error in Sleepers.Run: " + e);
            }
        }

        private static void RemoveSpawnedSleepers(int x1, int z1, int x2, int z2)
        {
            List<EntityAlive> hostiles = new List<EntityAlive>();

            try
            {
                hostiles.Clear();
                for (int index = 0; index < GameManager.Instance.World.Entities.list.Count; ++index)
                {
                    EntityAlive entityAlive = GameManager.Instance.World.Entities.list[index] as EntityAlive;
                    if (entityAlive != null && entityAlive.IsAlive() && EntityClass.list[entityAlive.entityClass].bIsEnemyEntity)
                    {
                        Vector3i hostilePos = new Vector3i(entityAlive.GetPosition());

                        //Fix the order of xyz1 xyz2
                        if (x2 < x1)
                        {
                            int val = x1;
                            x1 = x2;
                            x2 = val;
                        }

                        if (z2 < z1)
                        {
                            int val = z1;
                            z1 = z2;
                            z2 = val;
                        }

                        if (hostilePos.x > x1 && hostilePos.x < x2 && hostilePos.z > z1 && hostilePos.z < z2)
                        {
                            hostiles.Add(entityAlive);
                        }
                    }
                }

                if (hostiles.Count > 0)
                {
                    foreach (EntityAlive ea in hostiles)
                    {
                        GameManager.Instance.World.RemoveEntity(ea.entityId, EnumRemoveEntityReason.Killed);
                    }
                }

            }
            catch (Exception e) { Log.Error(e.ToString()); }
        }
    }
}
