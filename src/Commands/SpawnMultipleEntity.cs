using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class SpawnMultipleEntity : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "spawn multiple entities around some coordinate or player";
        }

        public override string getHelp()
        {
            return "Spawn multiple entities around some coordinate. Type \"mes\" to see all entity types\n" +
            "Usage:\n" +
            "   mes <x> <y> <z> <spawn radius> @ [<list of entities>]\n" +
            "or\n" +
            "   mes <x> <z> <spawn radius> @ [<list of entities>]>\n" +
            "or\n" +
            "   mes <steam id/player name/entity id> <spawn radius> @ [<list of entities>]\n" +
            "Example\n" +
            "   mes -1520 860 15 @ 1 1 18 18 21 21 21\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-multipleentityspawn", "multipleentityspawn", "mes" };
        }

        private void PrintEntities()
        {
            SdtdConsole.Instance.Output("The entities need to be in the list bellow:");
            int interationVal = 1;
            foreach (KeyValuePair<int, EntityClass> kvp in EntityClass.list.Dict)
            {
                SdtdConsole.Instance.Output(" " + interationVal + " - " + kvp.Value.entityClassName);
                ;
                interationVal++;
            }
        }


        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 2)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 3 or more, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    PrintEntities();
                    return;
                }
                int paramType = 0;
                for (int n = 0; n < _params.Count; n++)
                {
                    if (_params[n] == "@")
                    {
                        if (n == 2 || n == 3 || n == 4)
                        {
                            paramType = n;
                            break;
                        }
                    }
                }
                if (paramType == 0)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong pattern of arguments.");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    PrintEntities();
                    return;
                }

                int x = int.MinValue;
                int y = int.MinValue;
                int z = int.MinValue;
                int radius = int.MinValue;

                if (paramType == 2)
                {
                    ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci1 == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                    Vector3i pos = ep1.GetBlockPosition();
                    x = pos.x;
                    y = pos.y;
                    z = pos.z;
                }
                else if (paramType == 3 || paramType == 4)
                {
                    int.TryParse(_params[0], out x);
                    if (paramType == 3)
                    {
                        y = 150;
                        int.TryParse(_params[1], out z);
                    }
                    else if (paramType == 4)
                    {
                        int.TryParse(_params[1], out y);
                        int.TryParse(_params[2], out z);
                    }
                }
                if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                {
                    SdtdConsole.Instance.Output("x:" + x);
                    SdtdConsole.Instance.Output("y:" + y);
                    SdtdConsole.Instance.Output("z:" + z);
                    SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                    return;
                }

                int.TryParse(_params[(paramType - 1)], out radius);
                if (radius < 0)
                {
                    SdtdConsole.Instance.Output("ERR: radius is not a valid integer. It must be an integer greater or equal than 0.");
                    return;
                }

                for (int n = (paramType + 1); n < _params.Count; n++)
                {
                    int type = 0;
                    int.TryParse(_params[n], out type);

                    int interationNum = 1;
                    bool result = false;

                    foreach (KeyValuePair<int, EntityClass> kvp in EntityClass.list.Dict)
                    {
                        if (interationNum == type)
                        {
                            int realX, realY, realZ;
                            bool posFound = false;
                            if (radius == 0)
                            {
                                posFound = true;
                                realX = x;
                                realY = y;
                                realZ = z;
                            }
                            else
                            {
                                posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out realX, out realY, out realZ, new Vector3((float)radius, (float)radius, (float)radius), true);
                                if (!posFound)
                                {
                                    posFound = GameManager.Instance.World.FindRandomSpawnPointNearPosition(new Vector3((float)x, (float)y, (float)z), 15, out realX, out realY, out realZ, new Vector3((float)radius, (float)150, (float)radius), true);
                                }
                            }
                            if (posFound)
                            {
                                Entity entity = EntityFactory.CreateEntity(kvp.Key, new Vector3((float)realX, (float)realY, (float)realZ));
                                GameManager.Instance.World.SpawnEntityInWorld(entity);
                                result = true;
                                SdtdConsole.Instance.Output("Spawned " + kvp.Value.entityClassName + " at " + realX + " " + realY + " " + realZ);
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: No spawn point found near coordinate " + x + " " + y + " " + z);
                                result = true;
                            }
                            break;
                        }
                        interationNum++;
                    }
                    if (!result)
                    {
                        SdtdConsole.Instance.Output("ERR: Ignoring the invalid entity [" + type + "]");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in SpawnMultipleEntity: " + e);
            }
        }
    }
}
