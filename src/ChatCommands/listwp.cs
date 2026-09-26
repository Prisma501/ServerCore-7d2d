namespace PrismaCore
{
    class Listwp
    {
        public static bool Exec(ClientInfo _cInfo)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_listwp)
                {
                    var result = Database.Instance.ListDbWaypoints();

                    string PMmsg = "";
                    if (result.Count == 0)
                    {
                        PMmsg = PrismaCoreStrings.Instance.Listwp_NoWaypoints;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "" + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }
                    else PMmsg = PrismaCoreStrings.Instance.Listwp_ListTitle;

                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "" + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                    foreach (DbWaypoint wp in result)
                    {
                        string wpcoord = string.Format("X={0} Y={1} Z={2}", wp.X, wp.Y, wp.Z);
                        PMmsg = string.Format("{0} :  {1}", wp.Id, wpcoord);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "* [F7FE2E]" + PMmsg + "[-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch { return false; }
            return true;
        }
    }
}
