using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class ListBuffs : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "List or search all available buffs";
        }

        public override string getHelp()
        {
            return "List or search all available buffs\n" +
            "Usage:\n" +
            "1.   lbuffs <searchstring>\n" +
            "2.   lbuffs";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-listbuffs", "listbuffs", "lbuffs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }

                if (_params.Count == 1)
                {
                    foreach (KeyValuePair<string, BuffClass> kvp in BuffManager.Buffs)
                    {
                        if (kvp.Value.LocalizedName.ContainsCaseInsensitive(_params[0]) || kvp.Value.Name.ContainsCaseInsensitive(_params[0]))
                        {
                            SdtdConsole.Instance.Output($"Buff Name: {kvp.Value.LocalizedName}");
                            SdtdConsole.Instance.Output($"Technical BuffName: {kvp.Value.Name}");
                            if (!string.IsNullOrEmpty(kvp.Value.Description))
                            {
                                SdtdConsole.Instance.Output($"Buff Description: {kvp.Value.Description}");
                            }

                            SdtdConsole.Instance.Output(string.Empty);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, BuffClass> kvp in BuffManager.Buffs)
                    {
                        SdtdConsole.Instance.Output($"Buff Name: {kvp.Value.LocalizedName}");
                        SdtdConsole.Instance.Output($"Technical BuffName: {kvp.Value.Name}");
                        if (!string.IsNullOrEmpty(kvp.Value.Description))
                        {
                            SdtdConsole.Instance.Output($"Buff Description: {kvp.Value.Description}");
                        }

                        SdtdConsole.Instance.Output(string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in ListBuffs.Execute: " + e);
            }
        }
    }
}
