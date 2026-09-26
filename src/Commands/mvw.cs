using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class Mvw : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Move player to waypoint.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " mvw <playerName/steamId> <waypointName>\n" +
                   " mvw listwaypoints";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-mvw", "mvw" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                    return;
                }

                string movingPlayer = "";
                string toWaypoint = "";

                EntityPlayer movePlayer = null;

                if (_params.Count == 1)
                {
                    string PMmsg = "Available waypoints:";
                    SdtdConsole.Instance.Output(PMmsg);

                    var result = Database.Instance.ListDbWaypoints();

                    foreach (DbWaypoint wp in result)
                    {
                        string wpcoord = string.Format("X={0} Y={1} Z={2}", wp.X, wp.Y, wp.Z);
                        SdtdConsole.Instance.Output(string.Format("{0} :  {1}", wp.Id, wpcoord));
                    }

                    return;
                }

                if (_params.Count == 2)
                {
                    movingPlayer = _params[0].Trim();
                    toWaypoint = _params[1].Trim();

                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(movingPlayer);

                    if (ci == null)
                    {
                        //player offline
                        List<DbPlayer> players = Database.Instance.GetAllDbPlayers();

                        foreach (DbPlayer p in players)
                        {
                            if (p.Id.Equals(movingPlayer) || p.Name.EqualsCaseInsensitive(movingPlayer))
                            {
                                //move offline
                                DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                                if (wp == null)
                                {
                                    SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                                    return;
                                }

                                if (!string.IsNullOrEmpty(Database.Instance.GetTeleSpawn(p.Id)))
                                {
                                    Database.Instance.RemoveTeleSpawn(p.Id);
                                    Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));
                                }
                                else
                                    Database.Instance.SetTeleSpawn(p.Id, string.Format("{0},{1},{2}", wp.X, wp.Y, wp.Z));


                                SdtdConsole.Instance.Output("Player is offline and will be teleported on next spawn.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        //player online
                        movePlayer = GameManager.Instance.World.Players.dict[ci.entityId];
                        DbWaypoint wp = Database.Instance.GetDbWaypoint(toWaypoint);
                        if (wp == null)
                        {
                            SdtdConsole.Instance.Output($"ERR: Waypoint with name {toWaypoint} does not exist!");
                            return;
                        }
                        UnityEngine.Vector3 originPos = new UnityEngine.Vector3();
                        originPos = movePlayer.GetPosition();

                        UnityEngine.Vector3 destPos = new UnityEngine.Vector3
                        {
                            x = wp.X,
                            y = wp.Y,
                            z = wp.Z
                        };

                        //drone dupe
                        //if (PrismaCoreSettings.Instance.DroneDupePrevention_Enabled)
                        //{
                        //    if (DamageHandler.DroneDupeKill(ci))
                        //    {
                        //        return;
                        //    }
                        //}

                        NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                        ci.SendPackage(pkg);

                        //fix duping exploit
                        DamageHandler.StartThreadParameterizedCL(ci);

                        if (Move.dic.ContainsKey(ci.PlatformId.ToString()))
                        {
                            Move.dic.Remove(ci.PlatformId.ToString());
                            Move.dic.Add(ci.PlatformId.ToString(), originPos);
                        }
                        else
                        {
                            Move.dic.Add(ci.PlatformId.ToString(), originPos);
                        }

                        string PMmsg = "Succesfully moved  " + ci.playerName + " to waypoint " + wp.Id + ".";
                        SdtdConsole.Instance.Output(PMmsg);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in Mvw.Run: {0}.", e));
            }
        }
    }
}
