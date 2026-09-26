using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ToggleChatCommandHide : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "specify a chat message prefix that defines chat commands that are hidden from chat";
        }

        public override string getHelp()
        {
            return "If used chat messages starting with the defined prefix (e.g. \"/\") will not be shown to other players." +
            "Usage:\n" +
            "   hccp \n" +
            "    - If used without any parameter this functionality is disabled" +
            "   hccp <string pattern> \n" +
            "    - do not use string pattern with spaces and dont use comma for prefix.\n" +
            "    - define the prefix for chat commands\n" +
            "    - define multiple prefixes for chat commands by comma seperated list\n" +
            "   hccp list\n" +
            "    - show configured prefixes\n" +
            "   Example: hccp / or hccp #,$\n" +
            "    - Chat messages like \"/help\" or \"$help\" will be hidden\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-hidechatcommand", "hidechatcommand", "hccp" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(" * Do not use string pattern with spaces.");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }

                if (_params.Count == 0)
                {
                    PrismaCoreSettings.Instance.HideChatCommandPrefixes_Enabled = false;
                    PrismaCoreSettings.Instance.Save();
                    SdtdConsole.Instance.Output("Chat command hiding disabled");
                }
                else
                {
                    if (_params[0].Trim().ToLower() == "list")
                    {
                        if (PrismaCoreSettings.Instance.HideChatCommandPrefixes_Enabled)
                        {
                            SdtdConsole.Instance.Output($"Enabled prefixes: {PrismaCoreSettings.Instance.HideChatCommandPrefixes_Prefixes}");
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("Enabled prefixes: none");
                        }

                        return;
                    }
                    PrismaCoreSettings.Instance.HideChatCommandPrefixes_Prefixes = _params[0];
                    PrismaCoreSettings.Instance.HideChatCommandPrefixes_Enabled = true;
                    PrismaCoreSettings.Instance.Save();
                    SdtdConsole.Instance.Output("Prefix \"" + _params[0] + "\" defined for chat commands");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in ToggleChatCommandHide: " + e);
            }
        }
    }
}
