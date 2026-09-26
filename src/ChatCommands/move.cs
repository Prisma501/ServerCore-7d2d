using System;
using System.Collections.Generic;

namespace ServerCore
{
    public class Move
    {
        public static Dictionary<string, UnityEngine.Vector3> dic = new Dictionary<string, UnityEngine.Vector3>();

        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= ServerCoreSettings.Instance.ChatCommandPermissions_mv)
                {
                    string[] arrParameter = parameter.Split(' ');
                    string movingPlayer = "";
                    string toPlayer = "";
                    string x = "";
                    string z = "";
                    string nameMoving = "";
                    string nameTarget = "";
                    bool isOfflineMoving = false;
                    bool isOfflineTarget = false;

                    EntityPlayer movePlayer = null;
                    EntityPlayer targetPlayer = null;
                    ClientInfo cInfoMovePlayer = null;
                    DbPlayer offlinePlayerMoving = null;
                    DbPlayer offlinePlayerTarget = null;

                    if (arrParameter.Length == 2)
                    {
                        movingPlayer = arrParameter[0].Trim();
                        toPlayer = arrParameter[1].Trim();

                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                        foreach (DbPlayer p in players)
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
                                    isOfflineMoving = true;
                                    offlinePlayerMoving = p;
                                }

                            }
                            if (p.Name.IndexOf(toPlayer, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (p.Online)
                                {
                                    targetPlayer = GameManager.Instance.World.Players.dict[p.EntityId];
                                    nameTarget = p.Name;
                                }
                                else
                                {
                                    isOfflineTarget = true;
                                    offlinePlayerTarget = p;
                                }
                            }
                        }

                        if (isOfflineMoving)
                        {

                            int xo;
                            int yo;
                            int zo;

                            if (isOfflineTarget)
                            {

                                xo = offlinePlayerTarget.LastLocation.X;
                                yo = offlinePlayerTarget.LastLocation.Y;
                                zo = offlinePlayerTarget.LastLocation.Z;
                            }
                            else
                            {
                                //get online player info
                                xo = targetPlayer.GetBlockPosition().x;
                                yo = targetPlayer.GetBlockPosition().y;
                                zo = targetPlayer.GetBlockPosition().z;
                            }

                            if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(offlinePlayerMoving.Id)))
                            {
                                Database.Instance.RemoveTeleSpawn(offlinePlayerMoving.Id);
                                Database.Instance.SetTeleSpawn(offlinePlayerMoving.Id, string.Format("{0},-1,{1}", xo, zo));
                            }
                            else
                                Database.Instance.SetTeleSpawn(offlinePlayerMoving.Id, string.Format("{0},-1,{1}", xo, zo));

                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Mv_PlayerOffline), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        }
                        else
                        {
                            UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                            UnityEngine.Vector3 originPos = new UnityEngine.Vector3();
                            //check if target player is on,- or offline
                            if (isOfflineTarget)
                            {
                                destPos.x = offlinePlayerTarget.LastLocation.X;
                                destPos.y = offlinePlayerTarget.LastLocation.Y;
                                destPos.z = offlinePlayerTarget.LastLocation.Z;

                                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                cInfoMovePlayer.SendPackage(pkg);

                                //fix duping exploit
                                DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                string PMmsg = ServerCoreStrings.Instance.Mv_TargetOffline.Replace("{movingPlayer}", nameMoving);
                                PMmsg = PMmsg.Replace("{targetPlayer}", offlinePlayerTarget.Name);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            }
                            else
                            {
                                destPos = targetPlayer.GetPosition();
                                originPos = movePlayer.GetPosition();

                                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                cInfoMovePlayer.SendPackage(pkg);

                                //fix duping exploit
                                DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                if (dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                                {
                                    dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                                    dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                }
                                else
                                {
                                    dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                }
                                string PMmsg = ServerCoreStrings.Instance.Mv_SuccesPlayer.Replace("{movingPlayer}", nameMoving);
                                PMmsg = PMmsg.Replace("{targetPlayer}", nameTarget);
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            }
                        }
                        return true;
                    }
                    else if (arrParameter.Length == 3)
                    {
                        movingPlayer = arrParameter[0].Trim();
                        x = arrParameter[1].Trim();
                        z = arrParameter[2].Trim();

                        if (x.ToUpper().EndsWith("W"))
                        {
                            x = x.ToUpper().Replace("W", string.Empty);
                            x = "-" + x;
                        }
                        else if (x.ToUpper().EndsWith("E"))
                        {
                            x = x.ToUpper().Replace("E", string.Empty);
                        }
                        else return false;

                        if (z.ToUpper().EndsWith("S"))
                        {
                            z = z.ToUpper().Replace("S", string.Empty);
                            z = "-" + z;
                        }
                        else if (z.ToUpper().EndsWith("N"))
                        {
                            z = z.ToUpper().Replace("N", string.Empty);
                        }
                        else return false;

                        if (!IsDigitsOnly(x) || !IsDigitsOnly(z)) return false;

                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                        {
                            x = float.Parse(x),
                            y = -1,
                            z = float.Parse(z)
                        };

                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                        foreach (DbPlayer p in players)
                        {
                            if (p.Name.IndexOf(movingPlayer, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (p.Online)
                                {
                                    cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);
                                    NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                    cInfoMovePlayer.SendPackage(pkg);

                                    //fix duping exploit
                                    DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                    //report succesfull move to chatter
                                    if (x.StartsWith("-")) x = x.Replace("-", string.Empty) + "W";
                                    else x = x + "E";
                                    if (z.StartsWith("-")) z = z.Replace("-", string.Empty) + "S";
                                    else z = z + "N";

                                    string PMmsg = ServerCoreStrings.Instance.Mv_SuccesCoords.Replace("{movingPlayer}", p.Name);
                                    PMmsg = PMmsg.Replace("{coords}", "(" + x + ", " + z + ")");
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                                    return true;
                                }
                                else
                                {
                                    //move offline to coords
                                    if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(p.Id)))
                                    {
                                        Database.Instance.RemoveTeleSpawn(p.Id);
                                        Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},-1,{1}", x, z));
                                    }
                                    else
                                        Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},-1,{1}", x, z));

                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Mv_PlayerOffline), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                }
                            }
                        }
                    }
                    else return false;
                }
                else
                {
                    string errMsg = ServerCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in moving player: " + e.ToString());
                return false;
            }
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
