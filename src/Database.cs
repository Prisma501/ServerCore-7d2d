using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PrismaCore
{
    class Database
    {
        private static Database _instance;
        public static Database Instance => _instance ?? (_instance = new Database());
        public const string DbPlayersFileName = "PrismaCorePlayers.db";
        public const string DbWaypointsFileName = "PrismaCoreWaypoints.db";
        public const string DbGroupColorsFileName = "PrismaCoreGroupColors.db";

        public const string DbClaimsFileName = "PrismaCoreClaims.db";
        public const string DbTelespawnsFileName = "PrismaCoreTelespawns.db";
        public const string DbVehicleOwners = "PrismaCoreVehicleOwners.db";
        public const string DbDroneOwners = "PrismaCoreDroneOwners.db";

        public string GetVehicleOwner(int id)
        {
            lock (DbVehicleOwners)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbVehicleOwners}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbVehicleOwner>("vehicles");
                        var vehicle = col.FindById(id);
                        return vehicle.Owner;
                    }
                    catch { return null; }
                }
            }
        }

        public string GetDroneOwner(int id)
        {
            lock (DbDroneOwners)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbDroneOwners}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbDroneOwner>("drones");
                        var drone = col.FindById(id);
                        return drone.Owner;
                    }
                    catch { return null; }
                }
            }
        }

        public bool SetVehicleOwner(int id, string owner)
        {
            lock (DbVehicleOwners)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbVehicleOwners}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbVehicleOwner>("vehicles");
                        var vehicle = col.FindById(id);
                        if (vehicle != null)
                        {
                            vehicle.Owner = owner;
                            return col.Update(vehicle);
                        }
                        else
                        {
                            var dbvehicle = new DbVehicleOwner
                            {
                                Id = id,
                                Owner = owner
                            };

                            var docID = col.Insert(dbvehicle);
                            if (string.IsNullOrEmpty(docID.AsString))
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetDroneOwner(int id, string owner)
        {
            lock (DbDroneOwners)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbDroneOwners}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbDroneOwner>("drones");
                        var drone = col.FindById(id);
                        if (drone != null)
                        {
                            drone.Owner = owner;
                            return col.Update(drone);
                        }
                        else
                        {
                            var dbdrone = new DbDroneOwner
                            {
                                Id = id,
                                Owner = owner
                            };

                            var docID = col.Insert(dbdrone);
                            if (string.IsNullOrEmpty(docID.AsString))
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public string GetTeleSpawn(string steamid)
        {
            lock (DbTelespawnsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbTelespawnsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbTeleOnSpawn>("telespawns");
                        var telespawn = col.FindById(steamid);
                        return telespawn.Coord;
                    }
                    catch { return null; }
                }
            }
        }

        public bool SetTeleSpawn(string steamid, string coord)
        {
            lock (DbTelespawnsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbTelespawnsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbTeleOnSpawn>("telespawns");
                        var telespawn = new DbTeleOnSpawn
                        {
                            Id = steamid,
                            Coord = coord
                        };
                        col.Insert(telespawn);
                        return true;
                    }
                    catch { return false; }
                }
            }
        }

        public bool RemoveTeleSpawn(string steamid)
        {
            lock (DbTelespawnsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbTelespawnsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbTeleOnSpawn>("telespawns");
                        return col.Delete(steamid);
                    }
                    catch { return false; }
                }
            }
        }

        public List<DbPlayer> GetAllDbPlayers()
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var results = col.FindAll();
                        return results.ToList();

                    }
                    catch { return null; }
                }
            }
        }

        public List<DbClaim> GetAllDbClaims()
        {
            lock (DbClaimsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbClaimsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbClaim>("claims");
                        var results = col.FindAll();
                        return results.ToList();
                    }
                    catch { return null; }
                }
            }
        }

        public DbPlayer GetDbPlayer(string steamId)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            return player;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Out($"error in getting dbPlayer({steamId}). Error: {e}");
                        return null;
                    }
                }
            }
        }

        public long TotalPlayTime(string steamId)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            if (player.Online)
                            {
                                EntityPlayer ep = GameManager.Instance.World.Players.dict[player.EntityId];
                                long sessionPlayTime = (long)(Time.timeSinceLevelLoad - ep.CreationTimeSinceLevelLoad);
                                return player.TotalPlaytime += sessionPlayTime;
                            }
                            else
                            {
                                return player.TotalPlaytime;
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    catch { return 0; }
                }
            }
        }

        public bool SetGroupColor(string group, string color)
        {
            lock (DbGroupColorsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbGroupColorsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbGroupColor>("groupcolors");
                        var groupcolor = col.FindById(group);

                        if (groupcolor != null)
                        {
                            groupcolor.Color = color;
                            return col.Update(groupcolor);
                        }
                        else
                        {
                            var dbgroupcolor = new DbGroupColor
                            {
                                Id = group,
                                Color = color
                            };

                            var docID = col.Insert(dbgroupcolor);
                            if (string.IsNullOrEmpty(docID.AsString))
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public string GetGroupColor(string group)
        {
            lock (DbGroupColorsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbGroupColorsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbGroupColor>("groupcolors");
                        var groupcolor = col.FindById(group);
                        return groupcolor.Color;
                    }
                    catch { return null; }
                }
            }
        }

        public bool DeleteGroupColor(string group)
        {
            lock (DbGroupColorsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbGroupColorsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbGroupColor>("groupcolors");
                        return col.Delete(group);
                    }
                    catch { return false; }
                }
            }
        }

        public bool RemoveGroupFromPlayers(string group)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var players = col.FindAll();

                        foreach (DbPlayer pl in players)
                        {
                            if (pl.MemberOfGroup.ToLower() == group.ToLower())
                            {
                                pl.MemberOfGroup = string.Empty;
                                col.Update(pl);
                            }
                        }

                        return true;
                    }
                    catch { return false; }
                }
            }
        }

        public string GetSteamIdByEOS(string EOSId)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var result = col.FindOne(x => x.EOS_Id.Equals(EOSId));

                        if (result == null)
                        {
                            return null;
                        }
                        else
                        {
                            return result.Id;
                        }
                    }
                    catch { return null; }
                }
            }
        }

        public string GetEOSIdBySteamId(string steamId)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var result = col.FindOne(x => x.Id.Equals(steamId));

                        if (result == null)
                        {
                            return null;
                        }
                        else
                        {
                            return result.EOS_Id;
                        }
                    }
                    catch { return null; }
                }
            }
        }

        public List<DbGroupColor> ListGroupColors()
        {
            lock (DbGroupColorsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbGroupColorsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbGroupColor>("groupcolors");
                        var results = col.FindAll();
                        return results.ToList();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public bool SetChatNameOverride(string steamId, string chatNameOverride)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.ChatNameOverride = chatNameOverride;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetMemberOfGroup(string steamId, string memberOfGroup)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.MemberOfGroup = memberOfGroup;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetChatMuted(string steamId, bool chatMuted)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.ChatMuted = chatMuted;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetOnline(string steamId, int entityId, string name, string ip, DateTime lastOnline, string Eos_Id, LastKnownLocation lkl = null)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.Online = true;
                            player.EntityId = entityId;
                            player.EOS_Id = Eos_Id;
                            player.Name = name;
                            if (lkl != null)
                            {
                                player.LastLocation = lkl;
                            }
                            player.IP = ip;
                            player.LastOnline = lastOnline;
                            return col.Update(player);
                        }
                        else
                        {
                            var dbplayer = new DbPlayer
                            {
                                Id = steamId,
                                EOS_Id = Eos_Id,
                                Name = name,
                                ChatNameOverride = string.Empty,
                                MemberOfGroup = string.Empty,
                                ChatMuted = false,
                                ChatColor = string.Empty,
                                ChatName = false,
                                ReleaseTime = DateTime.Now,
                                AutoRelease = false,
                                Online = true,
                                EntityId = entityId,
                                LastLocation = lkl,
                                IP = ip,
                                LastOnline = lastOnline,
                                TotalPlaytime = 0
                            };

                            var docID = col.Insert(dbplayer);
                            if (string.IsNullOrEmpty(docID.AsString))
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetOffline(string steamId, DateTime lastOnline, long sessionTime = 0, LastKnownLocation lkl = null)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.Online = false;
                            if (lkl != null)
                            {
                                player.LastLocation = lkl;
                            }
                            player.LastOnline = lastOnline;
                            player.TotalPlaytime += sessionTime;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetChatColor(string steamId, string chatColor)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.ChatColor = chatColor;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetDbClaimWhitelist(string steamId, string whitelist)
        {
            lock (DbClaimsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbClaimsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbClaim>("claims");
                        var claim = col.FindById(steamId);

                        if (claim != null)
                        {
                            claim.Whitelist = whitelist;
                            return col.Update(claim);
                        }

                        return false;
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetChatName(string steamId, bool chatName)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.ChatName = chatName;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetReleaseTime(string steamId, DateTime releaseTime)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.ReleaseTime = releaseTime;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public bool SetAutoRelease(string steamId, bool autoRelease)
        {
            lock (DbPlayersFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbPlayersFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbPlayer>("players");
                        var player = col.FindById(steamId);

                        if (player != null)
                        {
                            player.AutoRelease = autoRelease;
                            return col.Update(player);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch { return false; }
                }
            }
        }

        public void SavePlayerPosition(ClientInfo ci)
        {
            EntityPlayer pl = GameManager.Instance.World.Players.dict[ci.entityId];
            string steamID = ci.PlatformId.ToString();
            string PName = ci.playerName;
            int entID = pl.entityId;
            string lastRegisteredName = "";

            if (pl.IsSpawned())
            {
                int xp = (int)Math.Floor(pl.GetPosition().x);
                int yp = (int)Math.Floor(pl.GetPosition().y);
                int zp = (int)Math.Floor(pl.GetPosition().z);

                using (var db = new LiteDatabase(string.Format("{0}/{1}.db", LocationTracker.StatisticsPath, steamID)))
                {
                    try
                    {
                        var col = db.GetCollection<PlayerLocation>("playerlocation");

                        var playerlocation = new PlayerLocation
                        {
                            Dt = DateTime.Now,
                            X = xp,
                            Y = yp,
                            Z = zp
                        };

                        col.Insert(playerlocation);

                        col.EnsureIndex(x => x.Dt);
                        col.EnsureIndex(x => x.X);
                        col.EnsureIndex(x => x.Y);
                        col.EnsureIndex(x => x.Z);

                        //fill table identifiers for name resolving (keep location db smaller)
                        var colID = db.GetCollection<Identifiers>("identifiers");

                        var check = colID.Find(Query.All(Query.Descending), 0, 1);

                        foreach (Identifiers idf in check)
                        {
                            lastRegisteredName = idf.Name;
                        }

                        if (!lastRegisteredName.Equals(PName))
                        {
                            var identifiers = new Identifiers
                            {
                                Dt = DateTime.Now,
                                Name = PName,
                                EntityID = entID
                            };

                            colID.Insert(identifiers);
                            colID.EnsureIndex(x => x.Dt);
                            colID.EnsureIndex(x => x.Name);
                        }
                    }
                    catch { }
                }
            }
        }

        public bool SaveDbWaypoint(string name, int x, int y, int z)
        {
            lock (DbWaypointsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbWaypointsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbWaypoint>("waypoints");

                        var dbwaypoint = new DbWaypoint
                        {
                            Id = name,
                            X = x,
                            Y = y,
                            Z = z
                        };

                        var docID = col.Insert(dbwaypoint);
                        if (string.IsNullOrEmpty(docID.AsString))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public bool SaveDbClaim(DbClaim claim)
        {
            lock (DbClaimsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbClaimsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbClaim>("claims");

                        var docID = col.Insert(claim);
                        if (string.IsNullOrEmpty(docID.AsString))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public bool DeleteDbWaypoint(string name)
        {
            lock (DbWaypointsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbWaypointsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbWaypoint>("waypoints");

                        return col.Delete(name);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public bool DeleteDbClaim(string name)
        {
            lock (DbClaimsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbClaimsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbClaim>("claims");

                        return col.Delete(name);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public DbWaypoint GetDbWaypoint(string name)
        {
            lock (DbWaypointsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbWaypointsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbWaypoint>("waypoints");

                        return col.FindById(name);

                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public DbClaim GetDbClaim(string name)
        {
            lock (DbClaimsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbClaimsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbClaim>("claims");

                        return col.FindById(name);

                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public List<DbWaypoint> ListDbWaypoints()
        {
            lock (DbWaypointsFileName)
            {
                using (var db = new LiteDatabase($"{GameIO.GetSaveGameDir()}/{DbWaypointsFileName}"))
                {
                    try
                    {
                        var col = db.GetCollection<DbWaypoint>("waypoints");
                        var results = col.FindAll();
                        return results.ToList();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public void CleanDataBases()
        {
            if (PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours == 0) return;

            DirectoryInfo d = new DirectoryInfo(LocationTracker.StatisticsPath);

            foreach (var file in d.GetFiles("*.db"))
            {
                using (var db = new LiteDatabase($"{file.FullName}"))
                {
                    try
                    {
                        if (PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours > 0)
                        {
                            var col = db.GetCollection<PlayerLocation>("playerlocation");
                            var res = col.Delete(x => (DateTime.Now - x.Dt).TotalHours > PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours);
                        }
                    }
                    catch { Log.Out($"[PrismaCore]: Error in CleanDataBases. Corruption/open filehandle in {file.FullName}"); }
                }
            }
        }

        public string GetSteamID(string _nameOrId)
        {
            if (_nameOrId == null || _nameOrId.Length == 0)
            {
                return null;
            }

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_nameOrId);

            if (ci == null)
            {
                long tempLong;
                if (_nameOrId.Length == 17 && long.TryParse(_nameOrId, out tempLong))
                {
                    return _nameOrId;
                }
            }
            else
            {
                return ci.PlatformId.ToString();
            }

            return null;
        }
    }

    public class PlayerLocation
    {
        public int Id { get; set; }
        public DateTime Dt { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class LastKnownLocation
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class Identifiers
    {
        public int Id { get; set; }
        public DateTime Dt { get; set; }
        public string Name { get; set; }
        public int EntityID { get; set; }
    }

    public class DbWaypoint
    {
        public string Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public class DbPlayer
    {
        public string Id { get; set; }
        public string EOS_Id { get; set; }
        public string Name { get; set; }
        public string ChatNameOverride { get; set; } = string.Empty;
        public string MemberOfGroup { get; set; } = string.Empty;
        public bool ChatMuted { get; set; }
        public string ChatColor { get; set; }
        public bool ChatName { get; set; }
        public DateTime ReleaseTime { get; set; }
        public bool AutoRelease { get; set; }
        public bool Online { get; set; }
        public int EntityId { get; set; }
        public LastKnownLocation LastLocation { get; set; }
        public string IP { get; set; }
        public DateTime LastOnline { get; set; }
        public long TotalPlaytime { get; set; }
    }

    public class DbGroupColor
    {
        public string Id { get; set; }
        public string Color { get; set; }
    }

    public class DbTradingChest
    {
        public string Id { get; set; }
        public string TradingTarget { get; set; }
        public string Items { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public bool Approved { get; set; }
    }

    public class DbClaim
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int AccessLevel { get; set; }
        public string Whitelist { get; set; }
        public int W_bound { get; set; }
        public int E_bound { get; set; }
        public int N_bound { get; set; }
        public int S_bound { get; set; }

    }

    public class DbTeleOnSpawn
    {
        public string Id { get; set; }
        public string Coord { get; set; }
    }

    public class Inventory
    {
        public List<InventoryItem> bag;
        public List<InventoryItem> belt;
        public InventoryItem[] equipment;
    }

    public class InventoryItem
    {
        public string itemName;
        public int count;
        public int quality;
        public InventoryItem[] parts;
        public int maxUseTimes;
        public float useTimes;
    }

    public class DbVehicleOwner
    {
        public int Id { get; set; }
        public string Owner { get; set; }
    }

    public class DbDroneOwner
    {
        public int Id { get; set; }
        public string Owner { get; set; }
    }
}
