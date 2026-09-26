using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class SpawnScouts : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Spawn scouts near a player or coordinate";
        }

        public override string getHelp()
        {
            return "Call scouts near a player." +
            "Usage:\n" +
            "   cs <steam id/player name/entity id>\n" +
            "or" +
            "   cs <x> <y> <z>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-cs", "cs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 3)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1 or 3, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                Vector3 pos = new Vector3();
                int x = int.MinValue;
                int y = int.MinValue;
                int z = int.MinValue;

                if (_params.Count == 1)
                {
                    ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci1 == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                    pos = ep1.GetPosition();
                }
                else if (_params.Count == 3)
                {
                    int.TryParse(_params[0], out x);
                    int.TryParse(_params[1], out y);
                    int.TryParse(_params[2], out z);

                    if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                    {
                        SdtdConsole.Instance.Output("x:" + x);
                        SdtdConsole.Instance.Output("y:" + y);
                        SdtdConsole.Instance.Output("z:" + z);
                        SdtdConsole.Instance.Output("ERR: At least one of the given coordinates is not a valid integer");
                        return;
                    }

                    pos = new Vector3((float)x, (float)y, (float)z);
                }

                AIDirectorChunkEventComponent chunkComponent = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
                if (chunkComponent == null)
                {
                    SdtdConsole.Instance.Output("ERR: No AIDirectorChunkEventComponent Component found");
                    return;
                }

                chunkComponent.SpawnScouts(pos);
                SdtdConsole.Instance.Output("Scouts spawning at " + pos.x + ", " + pos.y + ", " + pos.z);

            }
            catch (Exception e)
            {
                Log.Out("Error in SpawnScouts: " + e);
            }
        }
    }
}
