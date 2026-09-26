using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class SetPlayerStatus : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Change players attributes (zombiekills, playerkills)";
        }

        public override string getHelp()
        {
            return "Change players attributes (zombiekills, playerkills)\n" +
            "Usage:\n" +
            "1.   sps <name/entityId/steamId> <status> <value>\n" +
            "2.   <Status> can be: zkills, pkills";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-setplayerstatus", "setplayerstatus", "sps" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 3)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 3, found " + _params.Count + ".");
                    return;
                }
                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                    return;
                }

                EntityPlayer p1 = GameManager.Instance.World.Players.dict[ci.entityId];
                if (p1 == null)
                {
                    SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                    return;
                }
                string status = _params[1];

                int value = int.MinValue;

                int.TryParse(_params[2], out value);

                if (value < 0)
                {
                    SdtdConsole.Instance.Output("ERR: invalid value");
                    return;
                }
                if (status != "zkills" && status != "pkills")
                {
                    SdtdConsole.Instance.Output("ERR: invalid status type.");
                    return;
                }

                if (status == "pkills")
                {
                    int val = value - p1.KilledPlayers;
                    GameManager.Instance.AddScoreServer(p1.entityId, 0, val, p1.TeamNumber, 2);
                }

                if (status == "zkills")
                {
                    int val = value - p1.KilledZombies;
                    GameManager.Instance.AddScoreServer(p1.entityId, val, 0, p1.TeamNumber, 2);
                }

                SdtdConsole.Instance.Output("Attribute Updated!");
            }
            catch (Exception e)
            {
                Log.Out("Error in SetPlayerStatus.Execute: " + e);
            }
        }
    }
}
