using System;
using System.Collections.Generic;
using System.Threading;

namespace ServerCore
{
    public class ForceChunkReload
    {
        public static void exec(Dictionary<long, Chunk> dic)
        {
            try
            {
                Dictionary<string, List<Chunk>> players = new Dictionary<string, List<Chunk>>();
                foreach (long key in dic.Keys)
                {
                    try
                    {
                        Chunk chunk;
                        dic.TryGetValue(key, out chunk);
                        int x = chunk.GetWorldPos().x;
                        int z = chunk.GetWorldPos().z;

                        foreach (PlatformUserIdentifierAbs steamid in GameManager.Instance.persistentPlayers.Players.Keys)
                        {
                            try
                            {
                                ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(steamid.ToString());
                                if (ci1 != null)
                                {
                                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                                    if (Math.Abs(ep1.GetPosition().x - x) < 200 && Math.Abs(ep1.GetPosition().z - z) < 200)
                                    {
                                        List<Chunk> list;
                                        if (!players.ContainsKey(steamid.ToString()))
                                        {
                                            list = new List<Chunk>();
                                            players.Add(steamid.ToString(), list);
                                        }
                                        players.TryGetValue(steamid.ToString(), out list);

                                        list.Add(chunk);

                                        players.Remove(steamid.ToString());
                                        players.Add(steamid.ToString(), list);
                                    }
                                }
                            }
                            catch { continue; }
                        }
                    }
                    catch { continue; }
                }

                foreach (string steamid in players.Keys)
                {
                    try
                    {
                        ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(steamid);

                        if (ci1 != null)
                        {
                            List<Chunk> list;
                            players.TryGetValue(steamid, out list);
                            if (list != null)
                            {
                                foreach (Chunk chunk in list)
                                {
                                    try
                                    {
                                        ci1.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
                                    }
                                    catch { continue; }
                                }
                            }
                        }
                    }
                    catch { continue; }
                }

                Thread.Sleep(50);

                //MAKE TWITCE TO AVOID GLITCHES
                foreach (string steamid in players.Keys)
                {
                    try
                    {
                        ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(steamid);
                        if (ci1 != null)
                        {
                            List<Chunk> list;
                            players.TryGetValue(steamid, out list);
                            if (list != null)
                            {
                                foreach (Chunk chunk in list)
                                {
                                    try
                                    {
                                        ci1.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
                                    }
                                    catch { continue; }
                                }
                            }
                        }
                    }
                    catch { continue; }
                }
            }
            catch { }
        }

        public static void exec(Dictionary<long, Chunk> dic, ClientInfo ci1)
        {
            try
            {
                if (ci1 != null)
                {
                    foreach (long key in dic.Keys)
                    {
                        try
                        {
                            Chunk c;
                            if (dic.TryGetValue(key, out c))
                            {
                                ci1.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(c, true));
                            }
                        }
                        catch { continue; }
                    }
                }
            }
            catch { }
        }
    }
}
