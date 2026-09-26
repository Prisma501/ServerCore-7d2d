using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class GrabLCB : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Put all landclaims within <radius> of and owned by player in his/her backpack.";
        }

        public override string getHelp()
        {
            return "Usage:" +
                   "  1. grablcb <steamid / entityid / name> <radius>\n" +
                  "1. Put all landclaims within <radius> and owned by player in his/her backpack.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-grablcb", "grablcb" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output("ERR: Incorrect number of parameters. Expected 2.");
                    return;
                }

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player cannot be found!");
                    return;
                }

                if (!int.TryParse(_params[1], out int closeToDistance))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter for radius is not a valid integer!");
                    return;
                }

                EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                Vector3i closeTo = new Vector3i(ep.GetPosition());

                Dictionary<Vector3i, PersistentPlayerData> allLandClaims = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;
                List<Vector3i> claimsToRemove = new List<Vector3i>();

                foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allLandClaims)
                {
                    if (Math.Abs(kvp.Key.x - closeTo.x) <= closeToDistance && Math.Abs(kvp.Key.z - closeTo.z) <= closeToDistance)
                    {
                        if (kvp.Value.PlayerData.PrimaryId.ToString().Equals(ci.CrossplatformId.ToString()))
                        {
                            claimsToRemove.Add(kvp.Key);
                        }
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
                        GiveItem(ci, "keystoneBlock", 1);

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
                    SdtdConsole.Instance.Output("ERR: Error removing claims");
                    Log.Out("Error in GrabLCB.Run: " + e);
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in GrabLCB.Run: " + e);
            }
        }

        private static void GiveItem(ClientInfo ci, string item, int amount)
        {
            if (ci == null)
            {
                return;
            }

            World ww = GameManager.Instance.World;

            ItemValue iv = ItemClass.GetItem(item, true);
            if (iv.type == ItemValue.None.type)
            {
                return;
            }
            iv = new ItemValue(iv.type, true);

            if (iv == null)
            {
                return;
            }

            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
            {
                entityClass = EntityClass.FromString("item"),
                id = EntityFactory.nextEntityID++,
                itemStack = new ItemStack(iv, amount),
                pos = ww.Players.dict[ci.entityId].position,
                rot = new Vector3(20f, 0f, 20f),
                lifetime = 60f,
                belongsPlayerId = ci.entityId
            });
            ww.SpawnEntityInWorld(entityItem);
            ci.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, ci.entityId));
            ww.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);
        }
    }
}
