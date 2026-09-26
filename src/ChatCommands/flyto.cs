using System;
using System.Collections.Generic;

namespace ServerCore
{
    class Flyto
    {
        public static Dictionary<string, UnityEngine.Vector3> adminReturns = new Dictionary<string, UnityEngine.Vector3>();

        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);
                if (AdminLvL <= ServerCoreSettings.Instance.ChatCommandPermissions_ft)
                {
                    if (parameter.IndexOf(" ") >= 0 && !parameter.StartsWith("\"") && !parameter.EndsWith("\""))
                    {
                        //flyto coords
                        string[] arrParameter = parameter.Split(' ');

                        string x = "";
                        string z = "";

                        x = arrParameter[0].Trim();
                        z = arrParameter[1].Trim();

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

                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                        destPos.x = float.Parse(x);
                        destPos.y = -1;
                        destPos.z = float.Parse(z);

                        EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                        originPos = me.GetPosition();

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        _cInfo.SendPackage(pkg);

                        //fix duping exploit
                        DamageHandler.StartThreadParameterizedCL(_cInfo);

                        if (adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                        {
                            adminReturns.Remove(_cInfo.PlatformId.ToString());
                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                        }
                        else
                        {
                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                        }

                        return true;
                    }
                    else
                    {
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
                                        EntityPlayer targetPlayer = null;

                                        targetPlayer = GameManager.Instance.World.Players.dict[p.EntityId];

                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                                        destPos = targetPlayer.GetPosition();
                                        EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        originPos = me.GetPosition();

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);

                                        _cInfo.SendPackage(pkg);

                                        if (adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                                        {
                                            adminReturns.Remove(_cInfo.PlatformId.ToString());
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        destPos.x = p.LastLocation.X;
                                        destPos.y = p.LastLocation.Y;
                                        destPos.z = p.LastLocation.Z;
                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                                        EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        originPos = me.GetPosition();

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        _cInfo.SendPackage(pkg);

                                        if (adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                                        {
                                            adminReturns.Remove(_cInfo.PlatformId.ToString());
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }

                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Ft_TargetOffline.Replace("{playerName}", p.Name)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                                        return true;
                                    }

                                }
                            }
                            else
                            {
                                if (p.Name.IndexOf(parameter, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    if (p.Online)
                                    {
                                        EntityPlayer targetPlayer = null;

                                        targetPlayer = GameManager.Instance.World.Players.dict[p.EntityId];

                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                                        destPos = targetPlayer.GetPosition();
                                        EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        originPos = me.GetPosition();

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);

                                        _cInfo.SendPackage(pkg);

                                        if (adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                                        {
                                            adminReturns.Remove(_cInfo.PlatformId.ToString());
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }

                                        return true;
                                    }
                                    else
                                    {
                                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3();
                                        destPos.x = p.LastLocation.X;
                                        destPos.y = p.LastLocation.Y;
                                        destPos.z = p.LastLocation.Z;
                                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();

                                        EntityPlayer me = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                        originPos = me.GetPosition();

                                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                        _cInfo.SendPackage(pkg);

                                        if (adminReturns.ContainsKey(_cInfo.PlatformId.ToString()))
                                        {
                                            adminReturns.Remove(_cInfo.PlatformId.ToString());
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }
                                        else
                                        {
                                            adminReturns.Add(_cInfo.PlatformId.ToString(), originPos);
                                        }

                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.Ft_TargetOffline.Replace("{playerName}", p.Name)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string errMsg = ServerCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
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
