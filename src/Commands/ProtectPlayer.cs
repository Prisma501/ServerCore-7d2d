using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ProtectPlayer : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set protective bubble on player.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " protect <Name/steamId>\n" +
                   " protect list";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-protectplayer", "protectplayer", "protect" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1, found {0}", _params.Count));
                    return;
                }

                if (_params[0].ToLower().Equals("list"))
                {
                    //show all active bubbles
                    ExecuteList();
                    return;
                }

                if (_params[0].Length == 23)
                {
                    if (ClaimProtector.adminBubble.Contains(_params[0]))
                    {
                        ClaimProtector.adminBubble.Remove(_params[0]);
                        SdtdConsole.Instance.Output("Protective bubble disabled for " + _params[0]);
                        return;
                    }
                }

                ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                if (ci == null)
                {
                    string errMsg = "ERR: Can not find the player you told me to protect.";
                    SdtdConsole.Instance.Output(errMsg);
                    return;
                }
                if (ClaimProtector.adminBubble.Contains(ci.PlatformId.ToString()))
                {
                    //allready on => switch off
                    ClaimProtector.adminBubble.Remove(ci.PlatformId.ToString());
                    string off = "[F7FE2E]Protective bubble disabled![-]";
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, off), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    SdtdConsole.Instance.Output("Protective bubble disabled for " + ci.playerName);
                }
                else
                {
                    ClaimProtector.adminBubble.Add(ci.PlatformId.ToString());
                    string on = "[F7FE2E]Protective bubble enabled![-]";
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, on), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    SdtdConsole.Instance.Output("Protective bubble enabled for " + ci.playerName);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ProtectPlayer.Run: {0}.", e));
            }
        }

        private void ExecuteList()
        {
            SdtdConsole.Instance.Output("Active protective bubbles:");
            SdtdConsole.Instance.Output("  SteamID");

            foreach (string steamid in ClaimProtector.adminBubble)
            {
                SdtdConsole.Instance.Output(string.Format("  {0}", steamid));
            }
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
