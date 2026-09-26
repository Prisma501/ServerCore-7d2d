namespace PrismaCore
{
    class Bag
    {
        public static bool Exec(ClientInfo _cInfo)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_bag)
                {
                    if (API.dicDied.ContainsKey(_cInfo.PlatformId.ToString()))
                    {
                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();

                        destPos.x = API.dicDied[_cInfo.PlatformId.ToString()].x;
                        destPos.y = API.dicDied[_cInfo.PlatformId.ToString()].y + 1;
                        destPos.z = API.dicDied[_cInfo.PlatformId.ToString()].z;

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        _cInfo.SendPackage(pkg);

                        //fix duping exploit
                        DamageHandler.StartThreadParameterizedCL(_cInfo);

                        API.dicDied.Remove(_cInfo.PlatformId.ToString());
                    }
                    else
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Bag_PositionNotFound), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
