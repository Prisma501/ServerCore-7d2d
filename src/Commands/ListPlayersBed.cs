using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ListPlayersBed : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "list bed locations of all players or a specific player";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
            "  1. lpb <steam id / player name / entity id>\n" +
            "or\n " +
            "  2. lpb  *this will list all players online and their bed";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-listbedplayer", "listbedplayer", "lbp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 2)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of parameters. Expectec 0,1 or 2. Found: " + _params.Count);
                }
                else
                {
                    if (_params.Count == 1)
                    {
                        ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                        if (ci1 == null)
                        {
                            SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                            return;
                        }
                        EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                        EntityBedrollPositionList bed = ep1.SpawnPoints;

                        if (bed.Count == 0)
                        {
                            SdtdConsole.Instance.Output("ERR: The player does not have a bed");
                            return;
                        }
                        for (int x = 0; x < bed.Count; x++)
                        {
                            Vector3i pos = bed[x];
                            SdtdConsole.Instance.Output("PlayerBed: " + ep1.LocalizedEntityName + " at " + pos.x + ", " + pos.y + ", " + pos.z);
                        }
                    }
                    else if (_params.Count == 0)
                    {
                        Dictionary<int, EntityPlayer>.Enumerator enumerator = GameManager.Instance.World.Players.dict.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<int, EntityPlayer> pair = enumerator.Current;
                            EntityPlayer ep1 = pair.Value;
                            EntityBedrollPositionList bed = ep1.SpawnPoints;
                            if (bed.Count == 0)
                            {
                                continue;
                            }
                            else
                            {
                                for (int x = 0; x < bed.Count; x++)
                                {
                                    Vector3i pos = bed[x];
                                    SdtdConsole.Instance.Output(ep1.LocalizedEntityName + ": " + pos.x + ", " + pos.y + ", " + pos.z);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in ListPlayersBed.Run: " + e);
            }
        }
    }
}
