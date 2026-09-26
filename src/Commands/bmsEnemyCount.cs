using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class bmsEnemyCount : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set the maximum nr. of zombies alive per player on bloodmoons";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bmsEnemyCount <count>\n" +
                   " bmsEnemyCount";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bmsEnemyCount", "bmsEnemyCount" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 0 or 1, found {0}", _params.Count));
                    return;
                }

                if (_params.Count == 0)
                {
                    //show active
                    SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_BMEnemyCountPerPlayer: {PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_BMEnemyCountPerPlayer}");
                    return;
                }
                else
                {
                    if (!int.TryParse(_params[0], out int nr))
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid parameter. Must be an integer");
                        return;
                    }
                    else
                    {
                        PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_BMEnemyCountPerPlayer = nr;
                        SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_BMEnemyCountPerPlayer has been set to {nr} for current server session.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in bmsEnemyCount.Exec: {0}.", e));
            }
        }
    }
}
