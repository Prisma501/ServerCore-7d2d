using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class SayToPlayer2 : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "send a message to a single player with a specific sender name";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
            "   pm2 <sender playerName> <receiver player name / steam id / entity id> <message>\n" +
            "Send a PM from a chosen name to another player.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-sayplayer2", "sayplayer2", "pm2" };
        }

        private void RunInternal(List<string> _params)
        {
            if (_params.Count < 3)
            {
                SdtdConsole.Instance.Output("Usage: sayplayer2 <sendername> <playername|entityid> <message>");
                return;
            }

            string message = _params[2];
            string sender = _params[0];
            ClientInfo receiver = ConsoleHelper.ParseParamIdOrName(_params[1]);

            if (!string.IsNullOrEmpty(sender) && receiver != null)
            {
                //GameManager.Instance.ChatMessageServer(receiver, EChatType.Whisper, -1, message, sender, null);
                receiver.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, sender + ": " + message, null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
            }
            else
            {
                SdtdConsole.Instance.Output("ERR: Playername / SteamID / entity id of either sender or receiver not found.");
            }
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                RunInternal(_params);

            }
            catch (Exception e)
            {
                Log.Out("Error in SayToPlayer2.Run: " + e);
            }
        }
    }
}
