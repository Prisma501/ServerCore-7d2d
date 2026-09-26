using Platform.EOS;
using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class Mv : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Move player command (to coordinates and to other player). Optionally restrict to friends only.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " mv <playerName/steamId> <playerName/steamId> [fo] [os]\n" +
                   " mv <playerName/steamId> <xxE/W> <xxN/S>\n" +
                   " Use parameter fo to allow mv to ingame friends only (add parameter os to allow mv to offline ingame friends).";

        }
        public override string[] getCommands()
        {
            return new[] { "pc-mv", "mv" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 2 && _params.Count != 3 && _params.Count != 4)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2,3 or 4, found {0}", _params.Count));
                    return;
                }

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

                DbPlayer offlinePlayerMoving = null;
                DbPlayer offlinePlayerTarget = null;

                bool friendsOnly = false;
                bool offlineSupport = false;

                if (_params.Count == 4 && _params[3].ToLower() == "os")
                {
                    offlineSupport = true;
                    _params.RemoveAt(3);
                }

                if (_params.Count == 3 && _params[2].ToLower() == "fo")
                {
                    friendsOnly = true;
                    _params.RemoveAt(2);
                }

                if (_params.Count == 2)
                {
                    movingPlayer = _params[0].Trim();
                    toPlayer = _params[1].Trim();

                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(movingPlayer);
                    ClientInfo ciTo = ConsoleHelper.ParseParamIdOrName(toPlayer);

                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                    foreach (DbPlayer p in players)
                    {
                        if (p.Id.Equals(movingPlayer) || p.Name.EqualsCaseInsensitive(movingPlayer))
                        {
                            if (ci == null)
                            {
                                //moving player offline
                                isOfflineMoving = true;
                                offlinePlayerMoving = p;
                            }
                            else
                            {
                                //moving player online
                                movePlayer = GameManager.Instance.World.Players.dict[ci.entityId];
                                nameMoving = p.Name;
                            }
                        }

                        if (p.Id.Equals(toPlayer) || p.Name.EqualsCaseInsensitive(toPlayer))
                        {
                            if (ciTo == null)
                            {
                                //target player offline
                                isOfflineTarget = true;
                                offlinePlayerTarget = p;
                            }
                            else
                            {
                                //target player online
                                targetPlayer = GameManager.Instance.World.Players.dict[ciTo.entityId];
                                nameTarget = p.Name;
                            }
                        }
                    }

                    //check for friendsonly and abort if nescesary
                    if (!isOfflineMoving)
                    {
                        if (friendsOnly)
                        {
                            if (isOfflineTarget)
                            {
                                if (offlineSupport)
                                {
                                    PlatformUserIdentifierAbs userId = new UserIdentifierEos(offlinePlayerTarget.EOS_Id.Replace("EOS_", string.Empty));

                                    if (!IsFriend(ci.CrossplatformId, userId))
                                    {
                                        SdtdConsole.Instance.Output("ERR: Targetplayer is no in-game friend. Only TP to friends allowed.");
                                        return;
                                    }
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output("ERR: Targetplayer is offline. Only TP to online friends are allowed.");
                                    return;
                                }
                            }
                            else
                            {
                                if (!IsFriend(ci.CrossplatformId, ciTo.CrossplatformId))
                                {
                                    SdtdConsole.Instance.Output("ERR: Targetplayer is no in-game friend. Only TP to friends allowed.");
                                    return;
                                }
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
                            Database.Instance.SetTeleSpawn(offlinePlayerMoving.Id, string.Format("{0},{1},{2}", xo, yo, zo));
                        }
                        else
                            Database.Instance.SetTeleSpawn(offlinePlayerMoving.Id, string.Format("{0},{1},{2}", xo, yo, zo));


                        SdtdConsole.Instance.Output("Player is offline and will be teleported on next spawn.");
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
                            ci.SendPackage(pkg);

                            //fix duping exploit
                            DamageHandler.StartThreadParameterizedCL(ci);

                            string PMmsg = "Succesfully moved " + nameMoving + " to " + offlinePlayerTarget.Name + " (offline).";
                            SdtdConsole.Instance.Output(PMmsg);
                            Log.Out($"Succesfully moved {nameMoving} to {offlinePlayerTarget.Name} (offline).");
                        }
                        else
                        {
                            destPos = targetPlayer.GetPosition();
                            originPos = movePlayer.GetPosition();

                            NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                            ci.SendPackage(pkg);

                            //fix duping exploit
                            DamageHandler.StartThreadParameterizedCL(ci);

                            string PMmsg = "Succesfully moved  " + nameMoving + " to " + nameTarget + ".";
                            SdtdConsole.Instance.Output(PMmsg);
                        }
                    }
                    return;
                }
                else if (_params.Count == 3)
                {
                    movingPlayer = _params[0].Trim();
                    x = _params[1].Trim();
                    z = _params[2].Trim();

                    if (x.ToUpper().EndsWith("W"))
                    {
                        x = x.ToUpper().Replace("W", string.Empty);
                        x = "-" + x;
                    }
                    else if (x.ToUpper().EndsWith("E"))
                    {
                        x = x.ToUpper().Replace("E", string.Empty);
                    }
                    else return;

                    if (z.ToUpper().EndsWith("S"))
                    {
                        z = z.ToUpper().Replace("S", string.Empty);
                        z = "-" + z;
                    }
                    else if (z.ToUpper().EndsWith("N"))
                    {
                        z = z.ToUpper().Replace("N", string.Empty);
                    }
                    else return;

                    if (!IsDigitsOnly(x) || !IsDigitsOnly(z)) return;

                    UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                    {
                        x = float.Parse(x),
                        y = -1,
                        z = float.Parse(z)
                    };

                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(movingPlayer);

                    List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                    foreach (DbPlayer p in players)
                    {
                        if (p.Id.Equals(movingPlayer) || p.Name.EqualsCaseInsensitive(movingPlayer))
                        {
                            if (ci == null)
                            {
                                //player offline
                                //move offline to coords
                                if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(p.Id)))
                                {
                                    Database.Instance.RemoveTeleSpawn(p.Id);
                                    Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},-1,{1}", x, z));
                                }
                                else
                                    Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},-1,{1}", x, z));

                                SdtdConsole.Instance.Output("Player is offline and will be teleported on next spawn.");
                                return;
                            }
                            else
                            {
                                //player online

                                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                                ci.SendPackage(pkg);

                                //fix duping exploit
                                DamageHandler.StartThreadParameterizedCL(ci);

                                //report succesfull move to chatter
                                if (x.StartsWith("-")) x = x.Replace("-", string.Empty) + "W";
                                else x = x + "E";
                                if (z.StartsWith("-")) z = z.Replace("-", string.Empty) + "S";
                                else z = z + "N";

                                string PMmsg = "Succesfully moved  " + p.Name + " to ( " + x + " , " + z + " ).";
                                SdtdConsole.Instance.Output(PMmsg);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in Mv.Exec: {0}.", e));
            }
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

        private static bool IsFriend(PlatformUserIdentifierAbs steamId1, PlatformUserIdentifierAbs steamId2)
        {
            return GameManager.Instance.persistentPlayers.Allies.IsAlly(steamId1, steamId2);


        }
    }
}
