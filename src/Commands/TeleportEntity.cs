using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class TeleportEntity : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Teleport an entity";
        }

        public override string getHelp()
        {
            return "Teleport an entity.\n" +
                "Usage:\n" +
                "   1. etele entityID <x> <y> <z> [rot]\n" +
                "   2. etele entityID <player_id> [rot]\n" +
                "1. Teleport an entity to a location\n" +
                "2. Teleport an entity to player location\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-etele", "etele" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 4 && _params.Count != 5 && _params.Count != 3 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2, 3, 4 or 5 found " + _params.Count + ".");
                    return;
                }
                UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                int rotation = 0;
                int entityID = int.MinValue;

                int.TryParse(_params[0], out entityID);
                if (entityID == int.MinValue)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid entity ID");
                    return;
                }

                if (_params.Count == 4 || _params.Count == 5)
                {

                    int x = int.MinValue;
                    int y = int.MinValue;
                    int z = int.MinValue;

                    int.TryParse(_params[1], out x);
                    int.TryParse(_params[2], out y);
                    int.TryParse(_params[3], out z);

                    if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }

                    destPos.x = x;
                    destPos.y = y;
                    destPos.z = z;
                }
                else if (_params.Count == 2 || _params.Count == 3)
                {
                    ClientInfo ci2 = ConsoleHelper.ParseParamIdOrName(_params[1]);
                    if (ci2 == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Target playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep2 = GameManager.Instance.World.Players.dict[ci2.entityId];

                    destPos = ep2.GetPosition();
                    destPos.y += 1;
                    destPos.z += 1;
                }


                if (_params.Count == 5 || _params.Count == 3)
                {
                    if (_params.Count == 5)
                    {
                        int.TryParse(_params[4], out rotation);
                    }
                    else
                    {
                        int.TryParse(_params[2], out rotation);
                    }

                    if (rotation < 0 || rotation > 3)
                    {
                        SdtdConsole.Instance.Output("ERR: invalid rotation. It need to be greater or equal to 0 and less or equal to 3");
                        return;
                    }
                }


                bool found = false;

                List<Entity> entityes = GameManager.Instance.World.Entities.list;
                foreach (Entity entity in entityes)
                {
                    if (entity.entityId == entityID)
                    {
                        if (_params.Count == 5 || _params.Count == 3)
                        {
                            Vector3 vec = entity.rotation;
                            vec.y = rotation * 90;
                            entity.SetRotation(vec);
                        }
                        entity.SetPosition(destPos);
                        found = true;
                        SdtdConsole.Instance.Output("Entity " + entityID + " teleported.");
                        break;
                    }
                }

                if (!found)
                {
                    SdtdConsole.Instance.Output("ERR: unable to find the entity " + entityID + ".");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in TeleportEntity.Run: " + e);
                Log.Out(e.StackTrace);
            }
        }
    }
}
