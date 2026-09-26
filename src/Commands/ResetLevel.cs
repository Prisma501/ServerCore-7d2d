using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ResetLevel : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Reset a player's level to a given value.";
        }
        public override string getHelp()
        {
            return "Usage: rl <platformId / Name / entityid> <level>\n" +
                   "  Reset a player's level to <level>";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-rl", "rl", "resetlevel" };
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
                    SdtdConsole.Instance.Output(string.Format("ERR: Invalid level parameter {0}. Must be a valid integer", _params[1]));
                    return;
                }

                if (lvl < 1)
                {
                    SdtdConsole.Instance.Output("ERR: Level must be 1 or greater.");
                    return;
                }

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);

                if (ci == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player is offline. Player MUST be online for level reset.");
                    return;
                }
                else
                {
                    EntityAlive player = GameManager.Instance.World.Players.dict[ci.entityId];

                    player.Progression.Level = lvl;

                    player.Progression.bProgressionStatsChanged = true;

                    ci.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(player));

                }

                SdtdConsole.Instance.Output($"You have reset the level of this player to level {lvl}.");
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetLevel.Run: {0}.", e));
            }
        }
    }
}
