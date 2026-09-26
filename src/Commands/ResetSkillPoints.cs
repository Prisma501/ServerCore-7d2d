using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ResetSkillpoints : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Reset a player's skillpoints to a given value";
        }
        public override string getHelp()
        {
            return "Usage: rs <platformId / Name / entityid> <count>\n" +
                   "  Reset a player's skillpoints to <count>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-rs", "rs", "resetskillpoints" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                    return;
                }

                if (!int.TryParse(_params[1], out int lvl))
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Invalid count parameter {0}. Must be a valid integer", _params[1]));
                    return;
                }

                if (lvl < 0)
                {
                    SdtdConsole.Instance.Output("ERR: Count must be 0 or greater.");
                    return;
                }

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);

                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player is offline. Player MUST be online for skillpoints reset.");
                    return;
                }
                else
                {
                    EntityAlive player = GameManager.Instance.World.Players.dict[ci.entityId];

                    player.Progression.SkillPoints = lvl;

                    //player.bPlayerStatsChanged = true;
                    player.Progression.bProgressionStatsChanged = true;

                    ci.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));
                }

                SdtdConsole.Instance.Output($"You have reset the skillpoints for this player to {lvl}.");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetSkillPoints.Run: {0}.", e));
            }
        }
    }
}
