namespace ServerCore
{
    class Protect
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);
                if (AdminLvL <= ServerCoreSettings.Instance.ChatCommandPermissions_bubble)
                {
                    if (parameter.Equals(string.Empty))
                    {
                        if (ClaimProtector.adminBubble.Contains(_cInfo.PlatformId.ToString()))
                        {
                            //allready on => switch off
                            ClaimProtector.adminBubble.Remove(_cInfo.PlatformId.ToString());
                            string off = "[F7FE2E]Protective bubble disabled![-]";
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, off), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            Log.Out(_cInfo.playerName + " has disabled protective bubble.");
                        }
                        else
                        {
                            ClaimProtector.adminBubble.Add(_cInfo.PlatformId.ToString());
                            string on = "[F7FE2E]Protective bubble enabled![-]";
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, on), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            Log.Out(_cInfo.playerName + " has enabled protective bubble.");
                        }
                    }
                    else
                    {
                        //set bubble on player
                        ClientInfo ci = ConsoleHelper.ParseParamIdOrName(parameter);
                        if (ci == null)
                        {
                            string errMsg = "I can not find the player you told me to protect.";
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, "[FFA07A]" + errMsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return true;
                        }
                        if (ClaimProtector.adminBubble.Contains(ci.PlatformId.ToString()))
                        {
                            //allready on => switch off
                            ClaimProtector.adminBubble.Remove(ci.PlatformId.ToString());
                            string off = "[F7FE2E]Protective bubble disabled![-]";
                            string off2 = "[F7FE2E]Protective bubble disabled for " + ci.playerName + "![-]";
                            ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, off), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, off2), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            Log.Out(_cInfo.playerName + " has disabled protective bubble for " + ci.playerName);
                        }
                        else
                        {
                            ClaimProtector.adminBubble.Add(ci.PlatformId.ToString());
                            string on = "[F7FE2E]Protective bubble enabled![-]";
                            string on2 = "[F7FE2E]Protective bubble enabled for " + ci.playerName + "![-]";
                            ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, on), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, on2), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            Log.Out(_cInfo.playerName + " has enabled protective bubble for " + ci.playerName);
                        }

                    }
                }
                else
                {
                    string errMsg = ServerCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch { return false; }
            return true;
        }

    }
}
