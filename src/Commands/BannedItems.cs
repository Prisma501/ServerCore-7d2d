using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class BannedItems : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Manage the banned items list.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " bi add <itemName> <permissionLevel> \n" +
                   " bi remove <itemName>\n" +
                   " bi list";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-bi", "bi", "banneditems" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                //check for 2 parameters and add/remove and allow remote execution
                if (_params[0].EqualsCaseInsensitive("add"))
                {
                    if (_params.Count == 3)
                    {
                        if(!RegionReset.lstBannedItems.ContainsKey(_params[1]))
                        {
                            RegionReset.lstBannedItems.Add(_params[1], Convert.ToInt32(_params[2]));
                            RegionReset.SaveBannedItemsList();
                            SdtdConsole.Instance.Output($"Added item {_params[1]} with permissionlevel {_params[2]} to banned items.");
                            return;
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid number of parameters given. Add expects 2 parameters. Found: " + _params.Count);
                        return;
                    }
                }
                else if (_params[0].EqualsCaseInsensitive("remove"))
                {
                    if (_params.Count == 2)
                    {
                        if (RegionReset.lstBannedItems.ContainsKey(_params[1]))
                        {
                            RegionReset.lstBannedItems.Remove(_params[1]);
                            RegionReset.SaveBannedItemsList();
                            SdtdConsole.Instance.Output($"Removed item {_params[1]} from banned items.");
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output($"ERR: Item {_params[1]} cannot be found in banned items.");
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid number of parameters given. Remove expects 1 parameter. Found: " + _params.Count);
                        return;
                    }
                }
                else if (_params[0].EqualsCaseInsensitive("list"))
                {
                    if (_params.Count == 1)
                    {
                        SdtdConsole.Instance.Output("Banned Items:");
                        SdtdConsole.Instance.Output("ItemName:PermissionLevel");
                        foreach (KeyValuePair<string,int> kvp in RegionReset.lstBannedItems)
                        {
                            SdtdConsole.Instance.Output($"{kvp.Key}:{kvp.Value}");
                        }
                        return;
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid number of parameters given. List expects no parameters. Found: " + _params.Count);
                        return;
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output("ERR: Invalid or no subcommand given. Expected: add, remove or list");
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in BannedItems.Run: {0}.", e));
            }
        }
    }
}
