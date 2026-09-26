using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class bmsAdjustForOnlinePlayers : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Enable or disable the automatic adjusting of BloodmoonEnemycount to nr. of online players";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bmsAdjustForOnlinePlayers <true/false>\n" +
                   " bmsAdjustForOnlinePlayers";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bmsAdjustForOnlinePlayers", "bmsAdjustForOnlinePlayers" };
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
                    SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_AdjustBMEnemyCountPerPlayerToNrOnlinePlayers: {PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_AdjustBMEnemyCountPerPlayerToNrOnlinePlayers}");
                    return;
                }
                else
                {
                    if (!bool.TryParse(_params[0], out bool blocked))
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid parameter. Must be true or false");
                        return;
                    }
                    else
                    {
                        PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_AdjustBMEnemyCountPerPlayerToNrOnlinePlayers = blocked;
                        //PrismaCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_AdjustBMEnemyCountPerPlayerToNrOnlinePlayers has been set to {blocked} for current server session.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in bmsAdjustForOnlinePlayers.Exec: {0}.", e));
            }
        }
    }
}
