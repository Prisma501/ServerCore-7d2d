using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ListEntityCustom : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "List entity with custom parameter for better filtering.";
        }

        public override string getHelp()
        {
            return "List entity with custom parameter for better filtering:\n" +
            "  1. lce <x> <z> <radius> <type>\n" +
            "  2. lce <xMin> <xMax> <zMin> <zMax> <type>\n" +
            "  3. lce <type>\n" +
            "Valid types: Zombie, SupplyCrate, Backpack, Item, Animal, Minibike, Trader, Player, Jeep, Bicycle, Motorcycle, Gyrocopter\n" +
            "Use type as * to not filter the list by type";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-lce", "lce", "listcustomentity" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 4 && _params.Count != 5)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1, 4 or 5, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }
                string type = "*";

                int xMin = int.MinValue;
                int xMax = int.MinValue;
                int zMin = int.MinValue;
                int zMax = int.MinValue;


                if (_params.Count == 1)
                {
                    type = _params[0];
                }
                else if (_params.Count == 4)
                {
                    type = _params[3];
                    int radius = int.MinValue;
                    if (!int.TryParse(_params[0], out xMin) || xMin == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                    if (!int.TryParse(_params[1], out zMin) || zMin == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                    if (!int.TryParse(_params[2], out radius) || radius < 0)
                    {
                        SdtdConsole.Instance.Output("ERR: The radius must be greater than 0.");
                        return;
                    }
                    xMax = xMin + radius;
                    zMax = zMin + radius;
                    xMin = xMin - radius;
                    zMin = zMin - radius;
                }
                else if (_params.Count == 5)
                {
                    type = _params[4];
                    if (!int.TryParse(_params[0], out xMin) || xMin == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                    if (!int.TryParse(_params[1], out xMax) || xMax == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                    if (!int.TryParse(_params[2], out zMin) || zMin == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                    if (!int.TryParse(_params[3], out zMax) || zMax == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }
                }
                type = type.ToLower();
                if (type != "*" && type != "zombie" && type != "hornet" && type != "backpack" && type != "supplycrate" && type != "item" && type != "animal" && type != "minibike" && type != "trader" && type != "player" && type != "jeep" && type != "bicycle" && type != "motorcycle" && type != "gyrocopter")
                {
                    SdtdConsole.Instance.Output("ERR: invalid type defined");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                int counter = 0;
                List<Entity> list = GameManager.Instance.World.Entities.list;
                foreach (Entity entity in list)
                {
                    int x = entity.GetBlockPosition().x;
                    int z = entity.GetBlockPosition().z;
                    int y = entity.GetBlockPosition().y;
                    //Check if is in wanted area
                    if (xMin == int.MinValue || (x >= xMin && x <= xMax && z >= zMin && z <= zMax))
                    {


                        //Check if is wanted type
                        if (type == "*" || (entity.GetType().ToString().ToLower().IndexOf(type) != -1))
                        {
                            string name = "unknown";
                            if (entity.GetType().ToString() == "EntityZombie")
                            {
                                EntityZombie e = (EntityZombie)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityZombieCop")
                            {
                                EntityZombieCop e = (EntityZombieCop)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityZombieDog")
                            {
                                EntityZombieDog e = (EntityZombieDog)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntitySupplyCrate")
                            {
                                EntitySupplyCrate e = (EntitySupplyCrate)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityBackpack")
                            {
                                name = "Backpack";
                            }
                            else if (entity.GetType().ToString() == "EntityItem")
                            {
                                EntityItem e = (EntityItem)entity;
                                ItemClass ib = ItemClass.list[e.itemStack.itemValue.type];
                                name = ib.Name;
                            }
                            else if (entity.GetType().ToString() == "EntityAnimalRabbit")
                            {
                                EntityAnimalRabbit e = (EntityAnimalRabbit)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityAnimalStag")
                            {
                                EntityAnimalStag e = (EntityAnimalStag)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityMinibike")
                            {
                                EntityMinibike e = (EntityMinibike)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityTrader")
                            {
                                EntityTrader e = (EntityTrader)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityPlayer")
                            {
                                EntityPlayer e = (EntityPlayer)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityVJeep")
                            {
                                EntityVJeep e = (EntityVJeep)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityVGyroCopter")
                            {
                                EntityVGyroCopter e = (EntityVGyroCopter)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityMotorcycle")
                            {
                                EntityMotorcycle e = (EntityMotorcycle)entity;
                                name = e.LocalizedEntityName;
                            }
                            else if (entity.GetType().ToString() == "EntityBicycle")
                            {
                                EntityBicycle e = (EntityBicycle)entity;
                                name = e.LocalizedEntityName;
                            }
                            counter++;
                            SdtdConsole.Instance.Output("ListEntityCustom: " + counter + ". id=" + entity.entityId + ", type=" + entity.GetType().ToString() + ", name=" + name + ", pos=(" + x + "," + y + "," + z + ")");
                        }
                    }
                }


            }
            catch (Exception e)
            {
                Log.Out("Error in ListEntityCustom.Run: " + e);
            }
        }
    }
}
