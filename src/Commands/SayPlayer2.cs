using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class SayPlayer2 : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-say2", "say2" };
        }

        public override string getDescription()
        {
            return "Sends a message to all connected clients with specific sender";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "   say2 <senderName> <message>\n";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output(
                    "ERR: Wrong number of arguments, expected 2, found " + _params.Count + ".");
            }
            else
            {
                var _sender = _params[0];
                var _msg = _params[1];
                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, _sender + ": " + _msg, null, EMessageSender.None);
            }
        }
    }
}
