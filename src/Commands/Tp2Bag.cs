using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class Tp2Bag : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Teleport player to his/her backpack after death.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " tp2bag <Name/SteamID/EntityID>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-tp2bag", "tp2bag", "teleport2bag" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (ci == null)
                {
                    string errMsg = "ERR: Can not find the player for tp to bag.";
                    SdtdConsole.Instance.Output(errMsg);
                    return;
                }

                if (API.dicDied.ContainsKey(ci.PlatformId.ToString()))
                {
                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3();

                    destPos.x = API.dicDied[ci.PlatformId.ToString()].x;
                    destPos.y = API.dicDied[ci.PlatformId.ToString()].y + 1;
                    destPos.z = API.dicDied[ci.PlatformId.ToString()].z;

                    NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                    ci.SendPackage(pkg);

                    //fix duping exploit
                    DamageHandler.StartThreadParameterizedCL(ci);

                    API.dicDied.Remove(ci.PlatformId.ToString());
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: Players last dropped backpack position cannot be found.");
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in Tp2Bag.Run: {0}.", e));
            }
        }
    }
}
