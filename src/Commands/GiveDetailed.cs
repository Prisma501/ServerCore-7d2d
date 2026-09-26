using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class GiveDetailed : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "give an item to a player(s) (entity id or name)";
        }

        public override string getHelp()
        {
            return "Give item(s) to a (all) player(s) by putting in backpack of player(s)\n" +
                "Usage:\n" +
                "   giveplus <name/entityId/steamId> <item name> <amount> [<quality> <usedTimes>]\n" +
                "   giveplus all <item name> <amount> [<quality> <usedTimes>]\n" +
                "Either pass the full name of a player or his entity id (given by e.g. \"lpi\").\n" +
                "Item name has to be the exact name of an item as listed by \"listitems\".\n" +
                "Quality is the quality of the dropped items for items that have a quality.\n" +
                "usedTimes is the % that the item was used.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-giveplus", "giveplus" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3 && _params.Count != 5)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 3 or 5, found " + _params.Count + ".");
                    return;
                }

                if (_params[0] == "all")
                {
                    World w = GameManager.Instance.World;
                    foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
                    {
                        ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                        if (_cInfo == null)
                        {
                            continue;
                        }

                        ItemValue iva = ItemClass.GetItem(_params[1], true);
                        if (iva.type == ItemValue.None.type)
                        {
                            SdtdConsole.Instance.Output("ERR: Item not found.");
                            return;
                        }
                        iva = new ItemValue(iva.type, true);

                        if (iva == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Item not found.");
                            return;
                        }

                        int na = int.MinValue;
                        if (!int.TryParse(_params[2], out na) || na <= 0)
                        {
                            SdtdConsole.Instance.Output("ERR: Amount is not an integer or not greater than zero.");
                            return;
                        }


                        if (!iva.HasQuality && _params.Count == 5)
                        {
                            SdtdConsole.Instance.Output("ERR: Item " + _params[1] + " does not support quality.");
                            return;
                        }
                        else
                        {
                            if (iva.HasQuality && _params.Count == 5)
                            {
                                int qualitya = int.MinValue;
                                if (!int.TryParse(_params[3], out qualitya))
                                {
                                    SdtdConsole.Instance.Output("ERR: Quality is not an integer or not greater than zero.");
                                    return;
                                }

                                if (qualitya < 1 || qualitya > 6)
                                {
                                    SdtdConsole.Instance.Output("WARNING: Specified quality exceeds vanilla values (between 1 and 6). This only should be the case if you have mods installed that have altered the vanilla quality system. Using this value on a vanilla game can lead to unexpected behaviour!!!");
                                }

                                iva = new ItemValue(iva.type, qualitya, qualitya, false, null, 1f);

                                int useTimesa = int.MinValue;
                                if (!int.TryParse(_params[4], out useTimesa) || useTimesa < 0)
                                {
                                    SdtdConsole.Instance.Output(
                                        "ERR: UseTimes is not an integer or not equal and greater than zero.");
                                    return;
                                }

                                if (useTimesa > 100)
                                {
                                    SdtdConsole.Instance.Output("ERR: UseTimes is bigger than 100%");
                                    return;
                                }

                                if (iva.MaxUseTimes != 0)
                                {
                                    iva.UseTimes = iva.UseTimes = ((iva.MaxUseTimes * useTimesa) / 100);
                                }


                            }
                        }

                        var entityItema = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(iva, na),
                            pos = w.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        w.SpawnEntityInWorld(entityItema);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItema.entityId, _cInfo.entityId));
                        w.RemoveEntity(entityItema.entityId, EnumRemoveEntityReason.Killed);
                    }
                    SdtdConsole.Instance.Output("Item(s) given to all online players!");
                    return;
                }


                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);

                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                    return;
                }

                World ww = GameManager.Instance.World;

                ItemValue iv = ItemClass.GetItem(_params[1], true);
                if (iv.type == ItemValue.None.type)
                {
                    SdtdConsole.Instance.Output("ERR: Item not found.");
                    return;
                }
                iv = new ItemValue(iv.type, true);

                if (iv == null)
                {
                    SdtdConsole.Instance.Output("ERR: Item not found.");
                    return;
                }

                int n = int.MinValue;
                if (!int.TryParse(_params[2], out n) || n <= 0)
                {
                    SdtdConsole.Instance.Output("ERR: Amount is not an integer or not greater than zero.");
                    return;
                }

                if (!iv.HasQuality && _params.Count == 5)
                {
                    SdtdConsole.Instance.Output("ERR: Item " + _params[1] + " does not support quality.");
                    return;
                }
                else
                {
                    if (iv.HasQuality && _params.Count == 5)
                    {
                        int quality = int.MinValue;
                        if (!int.TryParse(_params[3], out quality))
                        {
                            SdtdConsole.Instance.Output("ERR: Quality is not an integer or not greater than zero.");
                            return;
                        }

                        if (quality < 1 || quality > 6)
                        {
                            SdtdConsole.Instance.Output("WARNING: Specified quality exceeds vanilla values (between 1 and 6). This only should be the case if you have mods installed that have altered the vanilla quality system. Using this value on a vanilla game can lead to unexpected behaviour!!!");
                        }

                        iv = new ItemValue(iv.type, quality, quality, false, null, 1f);

                        int useTimes = int.MinValue;
                        if (!int.TryParse(_params[4], out useTimes) || useTimes < 0)
                        {
                            SdtdConsole.Instance.Output(
                                "ERR: UseTimes is not an integer or not equal and greater than zero.");
                            return;
                        }

                        if (useTimes > 100)
                        {
                            SdtdConsole.Instance.Output("ERR: UseTimes is bigger than 100%");
                            return;
                        }

                        if (iv.MaxUseTimes != 0)
                        {
                            iv.UseTimes = iv.UseTimes = ((iv.MaxUseTimes * useTimes) / 100);
                        }

                    }

                }

                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(iv, n),
                    pos = ww.Players.dict[ci.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = ci.entityId
                });
                ww.SpawnEntityInWorld(entityItem);
                ci.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, ci.entityId));
                ww.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Killed);

                SdtdConsole.Instance.Output("Item(s) given");
            }
            catch (Exception e)
            {
                Log.Out("Error in GiveDetailed.Run: " + e);
            }
        }
    }
}
