using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;

namespace PrismaCore
{
    class Who
    {
        public static bool Exec(ClientInfo _cInfo, string parameter)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_loctrack)
                {
                    string radius = "";
                    string timespan = "";
                    ClientInfo clientinfo = null;
                    int nearDistance = PrismaCoreSettings.Instance.LocationTracker_NearDistance;
                    bool locationEnabled = PrismaCoreSettings.Instance.LocationTracker_Enabled;
                    int maxDataAge = PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours;
                    string command = PrismaCoreSettings.Instance.LocationTracker_ChatCommand;
                    string color = PrismaCoreSettings.Instance.LocationTracker_ResponseColor;
                    color = string.Format("[{0}]", color);

                    EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    int x = (int)Math.Floor(player.GetPosition().x);
                    int y = (int)Math.Floor(player.GetPosition().y);
                    int z = (int)Math.Floor(player.GetPosition().z);
                    //string steamID = _cInfo.playerId;

                    if (parameter.ToLower().Equals("near"))
                    {
                        bool playerFound = false;

                        string PMmsg = "Players within " + nearDistance.ToString() + " meters of your current location:";
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                        //scan for players
                        foreach (KeyValuePair<int, EntityPlayer> pl in GameManager.Instance.World.Players.dict)
                        {
                            clientinfo = ConnectionManager.Instance.Clients.ForEntityId(pl.Key);
                            if (clientinfo == null || clientinfo.PlatformId.ToString().Equals(_cInfo.PlatformId.ToString())) continue;

                            int xn = (int)Math.Floor(pl.Value.GetPosition().x);
                            int zn = (int)Math.Floor(pl.Value.GetPosition().z);

                            if (Math.Abs(x - xn) < nearDistance && Math.Abs(z - zn) < nearDistance)
                            {
                                playerFound = true;
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + clientinfo.playerName + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, color + clientinfo.playerName + "[-]", "*", false, null));
                            }

                        }
                        if (!playerFound) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + "No players are within " + nearDistance.ToString() + " meters of your current location.[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return true;
                    }

                    //if location recording disabled, these commands are not available
                    if (locationEnabled)
                    {
                        if (parameter.IndexOf(" ") >= 0)
                        {
                            string[] arrParameter = parameter.Split(' ');

                            radius = arrParameter[0].Trim();
                            timespan = arrParameter[1].Trim();

                            int r = int.MinValue;
                            int t = int.MinValue;

                            int.TryParse(radius, out r);
                            int.TryParse(timespan, out t);

                            if (r < 0)
                            {
                                // only radius > 0 allowed
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + "Something went wrong! Radius (first parameter) can NOT be negative![-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }

                            if (t < 1 || t > maxDataAge)
                            {
                                // only timespan between 1 and "whoUtils.maxAgeDataLocation" allowed
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + "Something went wrong! Timespan (second parameter) must be 1-" + maxDataAge.ToString() + " hours![-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return true;
                            }

                            string PMmsg = "Players within " + r.ToString() + " meters (last " + t.ToString() + " hours):";
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            DoWho(x, y, z, r, t, _cInfo, color);
                        }
                        else
                        {
                            string PMmsg = "Players within 50 meters (last 24 hours):";
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            DoWho(x, y, z, 50, 24, _cInfo, color);
                        }
                    }
                    else
                    {
                        string PMmsg = "Locationrecording is OFF. Only command you can use is: " + command + " near";
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + PMmsg + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in loctrack chatcommand: " + e.ToString());
                return false;
            }
            return true;
        }


        private static void DoWho(int xp, int yp, int zp, int r, int t, ClientInfo _cInfo, string color)
        {
            DirectoryInfo d = new DirectoryInfo(LocationTracker.StatisticsPath);
            bool playerFound = false;
            foreach (var file in d.GetFiles("*.db"))
            {
                using (var db = new LiteDatabase(file.FullName))
                {
                    var col = db.GetCollection<PlayerLocation>("playerlocation");

                    var count = col.Count(x => (DateTime.Now - x.Dt).TotalHours < t && Math.Abs(x.X - xp) < r && Math.Abs(x.Y - yp) < r && Math.Abs(x.Z - zp) < r);

                    if (count > 0)
                    {
                        //nameresolving
                        var colID = db.GetCollection<Identifiers>("identifiers");
                        var res = colID.FindOne(Query.All(Query.Descending));

                        //found players in range and timespan -> pm to calling player
                        playerFound = true;
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + res.Name + "[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    }

                }
            }
            if (!playerFound) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, color + "No players recorded on your location yet.[-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

        }

    }
}
