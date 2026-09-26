using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class bmsOverrideVanillaSpawner : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Enable/Disable vanilla bloodmoon spawner override.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bmsOverrideVanillaSpawner <true/false>\n" +
                   " bmsOverrideVanillaSpawner";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bmsOverrideVanillaSpawner", "bmsOverrideVanillaSpawner" };
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
                    SdtdConsole.Instance.Output($"PrismaCoreSettings.Instance.BloodmoonSpawner_OverrideVanillaSpawner: {PrismaCoreSettings.Instance.BloodmoonSpawner_OverrideVanillaSpawner}");
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
                        PrismaCoreSettings.Instance.BloodmoonSpawner_OverrideVanillaSpawner = blocked;
                        //PrismaCoreSettings.Instance.Save();
                        SdtdConsole.Instance.Output($"BloodmoonSpawner_OverrideVanillaSpawner has been set to {blocked} for current server session.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in bmsOverrideVanillaSpawner.Exec: {0}.", e));
            }
        }
    }
}
