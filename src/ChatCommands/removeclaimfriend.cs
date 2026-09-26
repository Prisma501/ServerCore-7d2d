using System;
using System.Collections.Generic;

namespace ServerCore
{
    class Removeclaimfriend
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
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Rcf_NoAdvancedClaim), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    bool notWhitelisted = false;
                    string playerRemoved = "";

                    foreach (string ownedClaim in ownedClaims)
                    {
                        DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaim);

                        //loop through persistent players to match parameter
                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();
                        foreach (DbPlayer player in players)
                        {
                            string myParameter = parameter;

                            if (myParameter.StartsWith("\"") && myParameter.EndsWith("\""))
                            {
                                myParameter = myParameter.Replace("\"", string.Empty);

                                if (player.Name.ToUpper().Equals(myParameter.ToUpper()))
                                {
                                    if (!activeClaim.Whitelist.Contains(player.Id))
                                    {
                                        notWhitelisted = true;
                                        break;
                                    }
                                    else
                                    {
                                        string currentWhitelist = activeClaim.Whitelist;
                                        currentWhitelist = currentWhitelist.Replace(player.Name + "(" + player.Id + ")", "");
                                        Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                        playerRemoved = player.Name;
                                        break;

                                    }
                                }
                            }
                            else
                            {
                                if (player.Name.IndexOf(parameter, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    if (!activeClaim.Whitelist.Contains(player.Id))
                                    {
                                        notWhitelisted = true;
                                        break;
                                    }
                                    else
                                    {
                                        string currentWhitelist = activeClaim.Whitelist;
                                        currentWhitelist = currentWhitelist.Replace(player.Name + "(" + player.Id + ")", "");
                                        Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                        playerRemoved = player.Name;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //player not found
                    if (!notWhitelisted && playerRemoved == "")
                    {
                        string msg5 = ServerCoreStrings.Instance.Rcf_PlayerNotFound;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg5), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    if (notWhitelisted)
                    {
                        string msg = ServerCoreStrings.Instance.Rcf_NotInWhitelist;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    if (playerRemoved != "")
                    {

                        string msg2 = ServerCoreStrings.Instance.Rcf_Success.Replace("{playerName}", playerRemoved);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg2), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    //nothing did happen and not all info is gathered
                    return false;
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
