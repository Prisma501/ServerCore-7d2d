using Epic.OnlineServices.Presence;
using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class SayAdmin : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Send a PM to all players meeting the minimum receipients permisson level";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
            "   sayadmin <chatName> <levelReceipients> <message>\n" +
            "Send a PM to all players meeting the minimum receipients permisson level";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-sayadmin", "sayadmin" };
        }

        private void RunInternal(List<string> _params)
        {
            World w = GameManager.Instance.World;

            if (_params.Count < 3)
            {
                SdtdConsole.Instance.Output("ERR: Usage: sayadmin <ChatName> <level> <message>");
                return;
            }

            string message = _params[2].Trim();

            int.TryParse(_params[1], out int level);

            foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
            {
                ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                if (ci == null) continue;

                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(ci);

                if (AdminLvL <= level)
                {
                    if (!string.IsNullOrEmpty(_params[0]) && ci != null)
                    {
                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(_params[0].Trim(), message), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    else
                    {
                        SdtdConsole.Instance.Output($"ERR: Could not send permission based message for player {ci.playerName}");

                    }
                }
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
                Log.Out("Error in SayAdmin.Run: " + e);
            }
        }
    }
}
