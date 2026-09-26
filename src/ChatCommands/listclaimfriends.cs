using System.Collections.Generic;

namespace ServerCore
{
    class Listclaimfriends
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                bool allowed = true;
                if (allowed)
                {
                    List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();
                    List<string> ownedClaims = new List<string>();

                    foreach (var claim in lstClaims)
                    {
                        if (claim.Id.Contains(_cInfo.PlatformId.ToString()))
                        {
                            ownedClaims.Add(claim.Id);
                        }
                    }

                    if (ownedClaims.Count == 0)
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Lcf_NoAdvancedClaim), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaims[0]);
                    //owned claim found
                    string whitelist = activeClaim.Whitelist;
                    if (string.IsNullOrEmpty(whitelist))
                    {
                        string PMmsg = ServerCoreStrings.Instance.Lcf_EmptyWhitelist;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    else
                    {
                        string PMmsg = "[F7FE2E]Claim whitelist:[-]";
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, "[F7FE2E]" + whitelist + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                    return true;
                }
                else
                {
                    string errMsg = ServerCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    return true;
                }
            }
            catch { return false; }
        }
    }
}
