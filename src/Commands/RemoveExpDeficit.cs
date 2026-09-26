using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class RemoveExpDeficit : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Remove a player's ExpDeficit.";
        }
        public override string getHelp()
        {
            return "Usage: red <platformId / Name / entityid>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-red", "red", "removeexpdeficit" };
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
                    SdtdConsole.Instance.Output("ERR: Player is offline. Player MUST be online for resetting ExpDeficit.");
                    return;
                }
                else
                {
                    EntityAlive player = GameManager.Instance.World.Players.dict[ci.entityId];

                    player.Progression.ExpDeficit = 0;

                    //player.bPlayerStatsChanged = true;
                    player.Progression.bProgressionStatsChanged = true;

                    ci.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));
                }

                SdtdConsole.Instance.Output($"You have removed ExpDeficit for this player.");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in RemoveExpDeficit.Run: {0}.", e));
            }
        }
    }
}
