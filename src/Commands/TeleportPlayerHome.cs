using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class TeleportPlayerHome : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "teleport a player to his home (on bedroll)";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
            "  teleportplayerhome <steam id / player name / entity id>";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-teleportplayerhome", "teleportplayerhome", "teleh" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output("Usage: teleportplayerhome <entityid|playername|steamid>");
                }
                else
                {
                    ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci1 == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                        return;
                    }
                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[ci1.entityId];
                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3();

                    EntityBedrollPositionList bed = ep1.SpawnPoints;
                    if (bed.Count != 0)
                    {
                        Vector3i pos = bed[0];
                        destPos.x = pos.x;
                        destPos.y = pos.y + 2;
                        destPos.z = pos.z;

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        ci1.SendPackage(pkg);

                        //fix duping exploit
                        DamageHandler.StartThreadParameterizedCL(ci1);

                        SdtdConsole.Instance.Output("Player teleported to his home at " + ep1.position.x + " " + ep1.position.y + " " + ep1.position.z);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: The player does not have a defined HOME bed!");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in TeleportPlayerHome.Run: " + e);
            }
        }
    }
}
