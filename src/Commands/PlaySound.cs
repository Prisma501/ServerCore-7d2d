using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class PlaySound : ConsoleCmdAbstract
    {
        public static List<string> loopedSounds = new List<string>();

        public override string getDescription()
        {
            return "Play an ingame sound on any server/player position.";
        }
        public override string getHelp()
        {
            return "Play an ingame sound on any server/ player position.\n" +
                   "Usage:\n" +
                   "  1. playsound <steamId/playerName/entityId> <soundName>\n" +
                   "  2. playsound <x> <y> <z> <soundName>\n" +
                   "  3. playsound listloops\n" +
                   "  4. playsound stop <x> <y> <z> <soundName>\n" +
                   "  5. playsound <searchString>\n" +
                   "  6. playsound\n" +
                   "1. Play a sound on the current postion of a player\n" +
                   "2. Play a sound on position x,y,z\n" +
                   "3. List all sounds that are playing in a loop\n" +
                   "4. Stop a sound that is played in loop (check listloops)\n" +
                   "5. Search for a soundname by (partial) string\n" +
                   "6. List all available sounds\n";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-playsound", "playsound" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                string sound = string.Empty;
                int x = 0, y = 0, z = 0;

                if (_params.Count == 0)
                {
                    foreach (string key in Audio.Manager.audioData.Keys)
                    {
                        SdtdConsole.Instance.Output(key);
                    }
                    return;

                }
                else if (_params.Count == 1)
                {
                    if (_params[0] == "listloops")
                    {
                        SdtdConsole.Instance.Output("Looped sounds currently playing:");
                        foreach (string s in loopedSounds)
                        {
                            SdtdConsole.Instance.Output(s);
                        }
                        return;
                    }
                    else
                    {
                        sound = _params[0];

                        foreach (string key in Audio.Manager.audioData.Keys)
                        {
                            if (key.Contains(sound)) SdtdConsole.Instance.Output(key);
                        }
                        return;
                    }
                }
                else if (_params.Count == 2)
                {
                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ERR: The player can not be found!");
                        return;
                    }

                    EntityPlayer player = GameManager.Instance.World.Players.dict[ci.entityId];
                    Vector3i vec = player.GetBlockPosition();
                    x = vec.x;
                    y = vec.y;
                    z = vec.z;
                    sound = _params[1];
                }
                else if (_params.Count == 4)
                {
                    int.TryParse(_params[0], out x);
                    int.TryParse(_params[1], out y);
                    int.TryParse(_params[2], out z);
                    sound = _params[3];
                }
                else if (_params.Count == 5)
                {
                    if (_params[0] == "stop")
                    {
                        int.TryParse(_params[1], out int xs);
                        int.TryParse(_params[2], out int ys);
                        int.TryParse(_params[3], out int zs);
                        string snd = _params[4];
                        Audio.Manager.BroadcastStop(new Vector3(xs, ys, zs), snd);
                        loopedSounds.Remove(xs + " " + ys + " " + zs + " " + snd);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: The stop parameter is expected but not found!");
                    }
                    return;
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: The number of parameters is incorrect. Expected 0, 1, 2, 4 or 5.");
                    return;
                }

                if (Audio.Manager.audioData.TryGetValue(sound, out Audio.XmlData xmlData))
                {
                    foreach (var prop in xmlData.audioClipMap)
                    {
                        if (prop.forceLoop)
                        {
                            //if looped save vector3 pos and soundname for stopping sound
                            string loop = x + " " + y + " " + z + " " + sound;
                            SdtdConsole.Instance.Output("Looped sound started: " + loop);
                            if (!loopedSounds.Contains(loop)) loopedSounds.Add(loop);
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: The sound can not be found!");
                    return;
                }

                Audio.Manager.BroadcastPlay(new Vector3(x, y, z), sound);
            }
            catch (Exception e)
            {
                SdtdConsole.Instance.Output(string.Format(string.Format("[PrismaCore] Error in Playsound.Execute: {0}.", e)));
            }
        }
    }
}
