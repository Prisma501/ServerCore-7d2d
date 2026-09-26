using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class Arrest : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-arrest", "arrest" };
        }

        public override string getDescription()
        {
            return "Put a player in jail (reversed claim jail required!)";
        }

        public override string getHelp()
        {
            return "Put a player in jail (reversed claim jail required!)\n" +
                "Usage:\n" +
                "   arrest <Name/EntityId/SteamId>\n" +
                "   arrest <Name/EntityId/SteamId> <jailTime(Minutes)>";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count != 1 && _params.Count != 2)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1 or 2, found " + _params.Count + ".");
                return;
            }

            DbClaim activeClaim = Database.Instance.GetDbClaim("jail");

            if (activeClaim == null)
            {
                SdtdConsole.Instance.Output("ERR: Reversed claim \"jail\" not found! Make it with ccc command.");
                return;

            }

            string steamID;
            string playerName;

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
            if (ci == null)
            {
                //check if steamid for offline use
                DbPlayer player = Database.Instance.GetDbPlayer(_params[0]);
                if (player == null)
                {
                    SdtdConsole.Instance.Output("ERR: Player not found!");
                    return;
                }
                else
                {
                    steamID = player.Id;
                    playerName = player.Name;
                }
            }
            else
            {
                steamID = ci.PlatformId.ToString();
                playerName = ci.playerName;
            }

            if (activeClaim.Whitelist.Contains(steamID))
            {
                SdtdConsole.Instance.Output(playerName + " is already in jail.");
                return;
            }
            else
            {
                string currentWhitelist = activeClaim.Whitelist;
                currentWhitelist += playerName + "(" + steamID + ")";
                Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);

                //check for jailtime and apply here
                if (_params.Count == 2)
                {
                    if (!int.TryParse(_params[1], out int minutes))
                    {
                        SdtdConsole.Instance.Output("ERR: Value for releaseTime is not a valid integer.");
                    }

                    DateTime newTime = DateTime.Now.AddMinutes(minutes);
                    Database.Instance.SetAutoRelease(steamID, true);
                    Database.Instance.SetReleaseTime(steamID, newTime);

                    if (ci != null)
                    {
                        string notiMsg = ServerCoreStrings.Instance.Arrest_Notification_Timed;
                        notiMsg = notiMsg.Replace("{minutes}", minutes.ToString());
                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    SdtdConsole.Instance.Output(playerName + " has been put in jail. And will automatically released in " + minutes + " minutes.");
                }
                else
                {
                    Database.Instance.SetAutoRelease(steamID, false);
                    if (ci != null)
                    {
                        string notiMsg = ServerCoreStrings.Instance.Arrest_Notification;
                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    SdtdConsole.Instance.Output(playerName + " has been put in jail. Use \"release\" command to set free.");
                }

                return;

            }
        }
    }
}
