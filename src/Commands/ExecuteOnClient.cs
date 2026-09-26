using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ExecuteOnClient : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-eoc", "eoc", "executeonclient" };
        }

        public override string getDescription()
        {
            return "Let a local player fire a local only console command.";
        }

        public override string getHelp()
        {
            return "Let a local player fire a local only console command\n" +
                "Usage:\n" +
                "   eoc <Name/EntityId/SteamId> \"command param1 param2\"\n" +
                "use single quotes for parameters that contain spaces in remote command.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2, found " + _params.Count + ".");
                return;
            }

            if (_params[0].ToLower().Trim() == "all")
            {
                World w = GameManager.Instance.World;
                foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
                {
                    ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                    if (_cInfo == null)
                    {
                        continue;
                    }

                    string command = _params[1].Replace("'", "\"");
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(command, true));
                }

                Log.Out($"[PrismaCore] Command has been sent to all connected clients for execution.");
            }
            else
            {
                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);

                if (ci != null)
                {
                    string command = _params[1].Replace("'", "\"");
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(command, true));
                    Log.Out($"[PrismaCore] Command: {command} sent to client for execution.");
                }
                else
                {
                    Log.Out("[PrismaCore] Player not found.");
                }
            }
        }
    }
}
