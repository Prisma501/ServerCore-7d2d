namespace ServerCore
{
    class Flytowp
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);
                if (AdminLvL <= ServerCoreSettings.Instance.ChatCommandPermissions_ftw)
                {
                    DbWaypoint wp = Database.Instance.GetDbWaypoint(parameter);
                    if (wp == null)
                    {
                        return false;
                    }

                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                    {
                        x = wp.X,
                        y = wp.Y,
                        z = wp.Z
                    };
                    UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                    EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    originPos = me.GetPosition();

                    NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                    _cInfo.SendPackage(pkg);

                    //fix duping exploit
                    DamageHandler.StartThreadParameterizedCL(_cInfo);

                    if (Flyto.adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                    {
                        Flyto.adminReturns.Remove(_cInfo.PlatformId.ToString());
                        Flyto.adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                    }
                    else
                    {
                        Flyto.adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
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
