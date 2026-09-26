using System;
using System.Collections.Generic;

namespace PrismaCore
{
    class Return
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            ClientInfo cInfoMovePlayer;

            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);
                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_tb)
                {
                    //drone dupe
                    //if (PrismaCoreSettings.Instance.DroneDupePrevention_Enabled)
                    //{
                    //    if (DamageHandler.DroneDupeKill(_cInfo))
                    //    {
                    //        return true;
                    //    }
                    //}

                    //flyto player
                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();
                    foreach (DbPlayer p in players)
                    {
                        string myParameter = parameter;

                        //ambigious name handling by doublequoting name
                        if (myParameter.StartsWith("\"") && myParameter.EndsWith("\""))
                        {
                            myParameter = myParameter.Replace("\"", string.Empty);

                            if (p.Name.ToUpper().Equals(myParameter.ToUpper()))
                            {
                                if (p.Online)
                                {
                                    //ClientInfo clientinfo = ConsoleHelper.ParseParamIdOrName(p.Name);
                                    cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);

                                    if (Move.dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                                    {
                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        destPos = Move.dic[cInfoMovePlayer.PlatformId.ToString()];
                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        cInfoMovePlayer.SendPackage(pkg);

                                        //fix duping exploit
                                        DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " has been returned." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        Move.dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " has no recorded origin to return to." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                    }

                                    return true;
                                }
                                else _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " is offline now." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            }
                        }
                        else
                        {
                            if (p.Name.IndexOf(parameter, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (p.Online)
                                {
                                    cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);

                                    if (Move.dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                                    {
                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        destPos = Move.dic[cInfoMovePlayer.PlatformId.ToString()];
                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        cInfoMovePlayer.SendPackage(pkg);

                                        //fix duping exploit
                                        DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " has been returned." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        Move.dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                                    }
                                    else
                                    {
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " has no recorded origin to return to." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                    }

                                    return true;
                                }
                                else _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, "[F7FE2E]" + p.Name + " is offline now." + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            }
                        }
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
        private static bool IsDigitsOnly(string str)
        {
            Int64 no = 0;
            if (Int64.TryParse(str, out no))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
