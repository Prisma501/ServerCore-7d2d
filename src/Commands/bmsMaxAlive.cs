using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class bmsMaxAlive : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Add to the number of maximum alive zombies on server setting.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bmsAddMaxAlive <count>\n" +
                   " bmsAddMaxAlive";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bmsAddMaxAlive", "bmsAddMaxAlive" };
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
                    SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_AddMaxAliveServerDuringBloodmoon: {PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_AddMaxAliveServerDuringBloodmoon}");
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
                        PrismaCoreSettings.Instance.BloodmoonSpawner_Overridden_AddMaxAliveServerDuringBloodmoon = nr;
                        //PrismaCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output($"BloodmoonSpawner_Overridden_AddMaxAliveServerDuringBloodmoon has been set to {nr} for current server session.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in bmsAddMaxAlive.Exec: {0}.", e));
            }
        }
    }
}
