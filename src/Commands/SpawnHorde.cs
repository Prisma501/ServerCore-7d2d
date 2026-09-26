using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class SpawnHorde : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Spawn targeted horde near a (or all) player or coordinate";
        }

        public override string getHelp()
        {
            return "Spawn targeted horde near a (or all) player." +
            "Usage:\n" +
            "   th <steam id/player name/entity id> <qntd>\n" +
            "or" +
            "   th all <qntd>\n" +
            "or" +
            "   th <x> <y> <z> <qntd>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-targetedhorde", "targetedhorde", "th" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 4)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2 or 4, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" ");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                Vector3 pos = new Vector3();
                int x = int.MinValue;
                int y = int.MinValue;
                int z = int.MinValue;


                int qtd = int.MinValue;

                if (_params.Count == 2)
                {
                    if (_params[0].ToLower() == "all")
                    {
                        World w = GameManager.Instance.World;
                        foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                            if (_cInfo == null)
                            {
                                continue;
                            }

                            pos = player.Value.GetPosition();
                            int.TryParse(_params[1], out qtd);

                            AIDirectorChunkEventComponent chunkComponentAll = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>();
                            if (chunkComponentAll == null)
                            {
                                SdtdConsole.Instance.Output("ERR: No AIDirectorChunkEventComponent Component found");
                                return;
                            }

                            AIScoutHordeSpawner.IHorde hordeAll = chunkComponentAll.CreateHorde(pos);
                            hordeAll.SpawnMore(qtd);
                        }
                        SdtdConsole.Instance.Output("Targeted horde spawned to all online players!");
                        return;
                    }
                    else
                    {
                        ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (ci1 == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Playername or entity/steam id not found.");
                            return;
                        }
                        EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                        pos = ep1.GetPosition();

                        int.TryParse(_params[1], out qtd);
                    }
                }
                else if (_params.Count == 4)
                {
                    int.TryParse(_params[0], out x);
                    int.TryParse(_params[1], out y);
                    int.TryParse(_params[2], out z);
                    int.TryParse(_params[3], out qtd);

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

                AIScoutHordeSpawner.IHorde horde = chunkComponent.CreateHorde(pos);
                horde.SpawnMore(qtd);

                SdtdConsole.Instance.Output("Horde spawning at " + pos.x + ", " + pos.y + ", " + pos.z);

            }
            catch (Exception e)
            {
                Log.Out("Error in SpawnHorde: " + e);
            }
        }
    }
}
