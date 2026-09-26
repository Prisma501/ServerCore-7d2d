using System;

namespace PrismaCore
{
    class Setwp
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_setwp)
                {
                    EntityPlayer targetPlayer = GameManager.Instance.World.Players.dict[_cInfo.entityId];

                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3();

                    destPos = targetPlayer.GetPosition();

                    bool success = Database.Instance.SaveDbWaypoint(parameter, (int)Math.Floor(destPos.x), (int)Math.Floor(destPos.y) + 1, (int)Math.Floor(destPos.z));

                    if (success)
                    {
                        string PMmsg = PrismaCoreStrings.Instance.Setwp_Success.Replace("{waypointName}", parameter);
                        PMmsg = PMmsg.Replace("{coords}", string.Format("X:{0} Y:{1} Z:{2}.", (int)Math.Floor(destPos.x), (int)Math.Floor(destPos.y) + 1, (int)Math.Floor(destPos.z)));
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    else
                    {
                        return false;
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
