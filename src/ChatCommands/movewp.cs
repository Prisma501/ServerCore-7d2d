using System;
using System.Collections.Generic;

namespace PrismaCore
{
    class Movewp
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_mvw)
                {
                    //drone dupe
                    //if (PrismaCoreSettings.Instance.DroneDupePrevention_Enabled)
                    //{
                    //    if (DamageHandler.DroneDupeKill(_cInfo))
                    //    {
                    //        return true;
                    //    }
                    //}

                    string[] arrParameter = parameter.Split(' ');
                    string movingPlayer = "";
                    string toWaypoint = "";

                    ClientInfo cInfoMovePlayer = null;
                    EntityPlayer movePlayer = null;

                    if (arrParameter.Length == 2)
                    {
                        movingPlayer = arrParameter[0].Trim();
                        toWaypoint = arrParameter[1].Trim();

                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                        foreach (DbPlayer p in players)
                        {
                            //ambigious name handling by doublequoting name
                            if (movingPlayer.StartsWith("\"") && movingPlayer.EndsWith("\""))
                            {
                                movingPlayer = movingPlayer.Replace("\"", string.Empty);

                                if (p.Name.ToUpper().Equals(movingPlayer.ToUpper()))
                                {
                                    if (p.Online)
                                    {
                                        cInfoMovePlayer = ConsoleHelper.ParseParamIdOrName(p.Name);
                                        movePlayer = GameManager.Instance.World.Players.dict[p.EntityId];
                                        DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                                        if (wp == null)
                                        {
                                            //SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                                            return false;
                                        }

                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();
                                        originPos = movePlayer.GetPosition();

                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                                        {
                                            x = wp.X,
                                            y = wp.Y,
                                            z = wp.Z
                                        };

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        cInfoMovePlayer.SendPackage(pkg);

                                        //fix duping exploit
                                        DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                        if (Move.dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                                        {
                                            Move.dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                        }

                                        string PMmsg = PrismaCoreStrings.Instance.Mvw_Success.Replace("{movingPlayer}", p.Name);
                                        PMmsg = PMmsg.Replace("{waypointName}", wp.Id);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        return true;

                                    }
                                    else
                                    {
                                        //move offline
                                        DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                                        if (wp == null)
                                        {
                                            //SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                                            return false;
                                        }

                                        if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(p.Id)))
                                        {
                                            Database.Instance.RemoveTeleSpawn(p.Id);
                                            Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));
                                        }
                                        else
                                            Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));


                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Mvw_PlayerOffline), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
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
                                        DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                                        if (wp == null)
                                        {
                                            //SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                                            return false;
                                        }

                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();
                                        originPos = movePlayer.GetPosition();

                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                                        {
                                            x = wp.X,
                                            y = wp.Y,
                                            z = wp.Z
                                        };

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        cInfoMovePlayer.SendPackage(pkg);

                                        //fix duping exploit
                                        DamageHandler.StartThreadParameterizedCL(cInfoMovePlayer);

                                        if (Move.dic.ContainsKey(cInfoMovePlayer.PlatformId.ToString()))
                                        {
                                            Move.dic.Remove(cInfoMovePlayer.PlatformId.ToString());
                                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            Move.dic.Add(cInfoMovePlayer.PlatformId.ToString(), originPos);
                                        }

                                        string PMmsg = PrismaCoreStrings.Instance.Mvw_Success.Replace("{movingPlayer}", p.Name);
                                        PMmsg = PMmsg.Replace("{waypointName}", wp.Id);
                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                        return true;

                                    }
                                    else
                                    {
                                        //move offline
                                        DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                                        if (wp == null)
                                        {
                                            //SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                                            return false;
                                        }

                                        if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(p.Id)))
                                        {
                                            Database.Instance.RemoveTeleSpawn(p.Id);
                                            Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));
                                        }
                                        else
                                            Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));


                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PrismaCoreStrings.Instance.Mvw_PlayerOffline), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                    }
                                }
                            }
                        }
                    }
                    else return false;
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
