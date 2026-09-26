namespace PrismaCore
{
    class Bed
    {
        public static bool Exec(ClientInfo _cInfo)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_bed)
                {
                    //drone dupe
                    //if(PrismaCoreSettings.Instance.DroneDupePrevention_Enabled)
                    //{
                    //    if (DamageHandler.DroneDupeKill(_cInfo))
                    //    {
                    //        return true;
                    //    }
                    //}

                    EntityPlayer ep1 = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3();

                    EntityBedrollPositionList bed = ep1.SpawnPoints;
                    if (bed.Count != 0)
                    {
                        Vector3i pos = bed[0];
                        destPos.x = pos.x;
                        destPos.y = pos.y + 2;
                        destPos.z = pos.z;

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        _cInfo.SendPackage(pkg);

                        //fix duping exploit
                        DamageHandler.StartThreadParameterizedCL(_cInfo);
                    }
                    else
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Bed_NoBed), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
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
