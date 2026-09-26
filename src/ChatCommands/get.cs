using System;
using System.Collections.Generic;

namespace PrismaCore
{
    public class Get
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_get)
                {
                    string movingPlayer = "";
                    string nameMoving = "";

                    EntityPlayer movePlayer = null;
                    EntityPlayer targetPlayer = null;
                    ClientInfo cInfoMovePlayer = null;

                    if (!string.IsNullOrEmpty(parameter))
                    {
                        movingPlayer = parameter;
                        targetPlayer = GameManager.Instance.World.Players.dict[_cInfo.entityId];

                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                        foreach (DbPlayer p in players)
                        {
                            if (movingPlayer.StartsWith("\"") && movingPlayer.EndsWith("\""))
                            {
                                movingPlayer = movingPlayer.Replace("\"", string.Empty);
                                if (p.Name.ToUpper().Equals(movingPlayer.ToUpper()))
                                {
                                    if (p.Online)
                                    {
                                        cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);
                                        movePlayer = GameManager.Instance.World.Players.dict[p.EntityId];
                                        nameMoving = p.Name;
                                    }
                                    else
                                    {
                                        string mNO = PrismaCoreStrings.Instance.Get_PlayerOffline.Replace("{playerName}", p.Name);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, mNO), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                if (p.Name.IndexOf(movingPlayer, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    if (p.Online)
                                    {
                                        cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);
                                        movePlayer = GameManager.Instance.World.Players.dict[p.EntityId];
                                        nameMoving = p.Name;
                                    }
                                    else
                                    {
                                        string mNO = PrismaCoreStrings.Instance.Get_PlayerOffline.Replace("{playerName}", p.Name);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, mNO), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        return true;
                                    }
                                }
                            }
                        }

                        if (movePlayer == null)
                        {
                            string mPNF = PrismaCoreStrings.Instance.Get_PlayerNotFound;
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, mPNF), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return true;
                        }

                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                        destPos = targetPlayer.GetPosition();
                        originPos = movePlayer.GetPosition();

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        cInfoMovePlayer.SendPackage(pkg);
                        if (Move.dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                        {
                            Move.dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                        }
                        else
                        {
                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                        }

                        string PMmsg = PrismaCoreStrings.Instance.Get_MoveSuccess.Replace("{namePlayer}", nameMoving);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }
                    else
                    {
                        string PMmsg = PrismaCoreStrings.Instance.Get_InvalidParameters;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    return true;
                }
            }
            catch { return false; }
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
