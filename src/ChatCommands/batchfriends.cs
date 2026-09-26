using System.Collections.Generic;
using static vp_ComponentPreset;

namespace PrismaCore
{
    class batchFriends
    {
        public static bool Exec(ClientInfo _cInfo, string type)
        {
            try
            {
                bool friends = false;
                                
                foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in GameManager.Instance.persistentPlayers.Allies.EnumerateAllies(_cInfo.CrossplatformId))
                {
                        friends = true;
                        string steamId = Database.Instance.GetSteamIdByEOS(platformUserIdentifierAbs.ToString());

                        if (type.ToLower().Equals("add"))
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
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Aaf_Raf_NoAdancedClaim), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }
                            foreach (string ownedClaim in ownedClaims)
                            {
                                DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaim);

                                if (activeClaim.Whitelist.Contains(steamId))
                                {
                                    continue;
                                }
                                else
                                {
                                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                                    foreach (DbPlayer player in players)
                                    {
                                        string pl = "";

                                        if (player != null)
                                        {
                                            if (player.Id == steamId)
                                            {
                                                pl = player.Name;
                                            }

                                            if (pl != "")
                                            {
                                                string currentWhitelist = activeClaim.Whitelist;
                                                currentWhitelist += pl + "(" + steamId + ")";
                                                Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                                string msg = PrismaCoreStrings.Instance.Aaf_AddSucces.Replace("{playerName}", pl);
                                                msg = msg.Replace("{claimName}", activeClaim.Id);
                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
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
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Aaf_Raf_NoAdancedClaim), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }
                            foreach (string ownedClaim in ownedClaims)
                            {
                                DbClaim activeClaim = Database.Instance.GetDbClaim(ownedClaim);

                                if (activeClaim.Whitelist.Contains(steamId))
                                {
                                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                                    foreach (DbPlayer player in players)
                                    {
                                        string pl = "";

                                        if (player != null)
                                        {
                                            if (player.Id == steamId)
                                            {
                                                pl = player.Name;
                                            }

                                            if (pl != "")
                                            {
                                                string currentWhitelist = activeClaim.Whitelist;
                                                currentWhitelist = currentWhitelist.Replace(player.Name + "(" + player.Id + ")", "");
                                                Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);
                                                string msg = PrismaCoreStrings.Instance.Raf_RemoveSucces.Replace("{playerName}", pl);
                                                msg = msg.Replace("{claimName}", activeClaim.Id);
                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                }

                if (!friends)
                {
                    string msg = PrismaCoreStrings.Instance.Aaf_Raf_NoIngameFriends;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch { return false; }
            return true;
        }
    }
}
