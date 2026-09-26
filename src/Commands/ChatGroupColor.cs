using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class ChatGroupColor : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Manage chat color by groupmembership.";
        }

        public override string getHelp()
        {
            return "Manage chat color by groupmembership.\n" +
                   "Usage:\n" +
                   "   cgc listgroups\n" +
                   "   cgc addgroup <groupName> <groupColor>\n" +
                   "   cgc deletegroup <groupName>\n" +
                   "   cgc listmembers <groupName>\n" +
                   "   cgc adduser <steam id/player name/entity id> <groupName>\n" +
                   "   cgc cleargroup <steam id/player name/entity id>\n" +
                   "The <groupColor> must be 6 hex characters. Example: FF00FF\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-chatgroupcolor", "chatgroupcolor", "cgc" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 1)
                {
                    if (_params[0].ToLower() == "listgroups")
                    {
                        List<DbGroupColor> groups = Database.Instance.ListGroupColors();

                        SdtdConsole.Instance.Output("List of groupcolors:");
                        foreach (DbGroupColor groupcolor in groups)
                        {
                            SdtdConsole.Instance.Output(string.Format("   {0} : {1}", groupcolor.Id, groupcolor.Color));
                        }
                        return;
                    }
                    else SdtdConsole.Instance.Output("ERR: Argument not recognized as subcommand.");
                }
                else if (_params.Count == 2)
                {
                    if (_params[0].ToLower() == "deletegroup")
                    {
                        if (Database.Instance.DeleteGroupColor(_params[1]))
                        {
                            Database.Instance.RemoveGroupFromPlayers(_params[1]);

                            SdtdConsole.Instance.Output("Group " + _params[1] + " has been removed!");
                        }
                        else SdtdConsole.Instance.Output("ERR: Group " + _params[1] + " does not exist!");

                        return;
                    }
                    else if (_params[0].ToLower() == "listmembers")
                    {
                        string groupName = _params[1];
                        List<DbPlayer> allPlayers = Database.Instance.GetAllDbPlayers();

                        if (!string.IsNullOrEmpty(Database.Instance.GetGroupColor(groupName)))
                        {
                            SdtdConsole.Instance.Output("Members of group " + groupName + ":");
                            foreach (DbPlayer pl in allPlayers)
                            {
                                if (pl.MemberOfGroup.ToLower() == groupName.ToLower())
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(pl.Name))
                                        {
                                            SdtdConsole.Instance.Output($"   {pl.Name}");
                                        }
                                        else
                                        {
                                            SdtdConsole.Instance.Output($"   {pl.Id}");
                                        }
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                            }
                            return;
                        }
                        else SdtdConsole.Instance.Output("ERR: Group " + groupName + " does not exist!");
                    }
                    else if (_params[0].ToLower() == "cleargroup")
                    {
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[1].Trim());
                        if (ci == null)
                        {
                            Database.Instance.SetMemberOfGroup(_params[1].Replace("steam_", "Steam_"), string.Empty);

                            SdtdConsole.Instance.Output(_params[1].Trim() + " groupmembership has been cleared!");
                            return;
                        }
                        else
                        {
                            Database.Instance.SetMemberOfGroup(ci.PlatformId.ToString(), string.Empty);

                            SdtdConsole.Instance.Output(ci.playerName + " groupmembership has been cleared!");
                            return;
                        }
                    }
                    else SdtdConsole.Instance.Output("ERR: Argument not recognized as subcommand.");
                }
                else if (_params.Count == 3)
                {
                    if (_params[0].ToLower() == "adduser")
                    {
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[1].Trim());
                        if (ci == null)
                        {
                            if (!string.IsNullOrEmpty(Database.Instance.GetGroupColor(_params[2].Trim())))
                            {
                                //player is offline, make sure its a steamId!!!
                                if (_params[1].Trim().Length == 23)
                                {
                                    Database.Instance.SetMemberOfGroup(_params[1].Trim().Replace("steam_", "Steam_"), _params[2].Trim());
                                    SdtdConsole.Instance.Output("Player " + _params[1].Trim() + " is now member of group " + _params[2].Trim());
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output($"ERR: Player is offline. Groupcolor can ONLY be added by using steamId!");
                                    return;
                                }
                            }
                            else SdtdConsole.Instance.Output("ERR: Group " + _params[2].Trim() + " does not exist!");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(Database.Instance.GetGroupColor(_params[2].Trim())))
                            {
                                Database.Instance.SetMemberOfGroup(ci.PlatformId.ToString(), _params[2].Trim());
                                SdtdConsole.Instance.Output("Player " + ci.playerName + " is now member of group " + _params[2].Trim());
                                return;
                            }
                            else SdtdConsole.Instance.Output("ERR: Group " + _params[2].Trim() + " does not exist!");
                        }
                    }
                    else if (_params[0].ToLower() == "addgroup")
                    {
                        string groupName = _params[1];
                        string groupColor = _params[2].ToUpper();

                        if (groupColor.Length != 6 || !OnlyHexInString(groupColor))
                        {
                            SdtdConsole.Instance.Output("ERR: Groupcolor has not the correct format!");
                            return;
                        }

                        if (Database.Instance.SetGroupColor(groupName, groupColor))
                        {
                            SdtdConsole.Instance.Output("Group " + groupName + " has succesfully been added with color " + groupColor);
                            return;
                        }
                        else SdtdConsole.Instance.Output("ERR: Adding groupcolor failed!");

                    }
                    else SdtdConsole.Instance.Output("ERR: Argument not recognized as subcommand.");
                }
                else SdtdConsole.Instance.Output(GetHelp());
            }
            catch (Exception e)
            {
                Log.Out("ChatGroupColor.Run: " + e);
            }
        }

        private static bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private static bool IsDigitsOnly(string str)
        {
            Int64 no = 0;
            if (Int64.TryParse(str, out no))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
