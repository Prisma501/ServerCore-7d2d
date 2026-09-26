using System;
using System.Collections.Generic;

namespace PrismaCore
{
    class seen
    {
        public static bool Exec(ClientInfo _cInfo, string target)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_ls)
                {
                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                    foreach (DbPlayer p in players)
                    {
                        string myTarget = target;
                        if (myTarget.StartsWith("\"") && myTarget.EndsWith("\""))
                        {
                            myTarget = myTarget.Replace("\"", string.Empty);

                            //Log.Out("Quoted name found in /seen. Unquoted name: " + myTarget);
                            string PMmsg = "";
                            if (p.Name.ToUpper().Equals(myTarget.ToUpper()))
                            {
                                if (p.Online)
                                    PMmsg = PrismaCoreStrings.Instance.Ls_OnlineNow.Replace("{playerName}", p.Name);
                                else
                                {

                                    string lo = p.LastOnline.ToString("yyyy-MM-dd HH:mm");
                                    string time = "";

                                    DateTime dtLo = DateTime.ParseExact(lo, "yyyy-MM-dd HH:mm", null);
                                    TimeSpan span = DateTime.Now - dtLo;

                                    if (span.Days != 0)
                                        time += span.Days.ToString() + " days ";
                                    if (span.Hours != 0)
                                        time += span.Hours.ToString() + " hours ";
                                    if (span.Minutes != 0)
                                        time += span.Minutes.ToString() + " minutes ";


                                    PMmsg = PrismaCoreStrings.Instance.Ls_Success.Replace("{playerName}", p.Name);
                                    PMmsg = PMmsg.Replace("{time}", time.Trim());
                                }
                                
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }
                        }
                        else
                        {
                            string PMmsg = "";
                            if (p.Name.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (p.Online)
                                    PMmsg = PrismaCoreStrings.Instance.Ls_OnlineNow.Replace("{playerName}", p.Name);
                                else
                                {

                                    string lo = p.LastOnline.ToString("yyyy-MM-dd HH:mm");
                                    string time = "";
                                    DateTime dtLo = DateTime.ParseExact(lo, "yyyy-MM-dd HH:mm", null);
                                    TimeSpan span = DateTime.Now - dtLo;

                                    if (span.Days != 0)
                                        time += span.Days.ToString() + " days ";
                                    if (span.Hours != 0)
                                        time += span.Hours.ToString() + " hours ";
                                    if (span.Minutes != 0)
                                        time += span.Minutes.ToString() + " minutes ";


                                    PMmsg = PrismaCoreStrings.Instance.Ls_Success.Replace("{playerName}", p.Name);
                                    PMmsg = PMmsg.Replace("{time}", time.Trim());
                                }

                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Out("ERR in seen.Exec: " + e.ToString());
                return false;
            }
            return true;
        }

    }
}
