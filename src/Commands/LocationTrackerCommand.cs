using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;

namespace PrismaCore.CustomCommands
{
    public class LocationTrackerCommand : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-loctrack", "loctrack" };
        }

        public override string getDescription()
        {
            return "Manage locationtracker settings and data";
        }

        public override string getHelp()
        {
            return "Manage locationtracker settings and data\n" +
                "Usage:\n" +
                "  1. loctrack search <steam id/player name/entity id> <radius> <numberOfHours>\n" +
                "  2. loctrack search <x> <y> <z> <radius> <numberOfHours>\n" +
                "  3. loctrack showtrack <steam id> <maxrecords> <timespan> <timeBetweenRecords>\n" +
                "  4. loctrack enabled <true/false>\n" +
                "  5. loctrack command <newChatCommand>\n" +
                "  6. loctrack commandenabled <true/false>\n" +
                "  7. loctrack interval <seconds>\n" +
                "  8. loctrack maxagedata <hours>\n" +
                "  9. loctrack neardistance <meters>\n" +
                " 10. loctrack responsecolor <hexstring>\n" +
                " 11. loctrack\n" +
                "1. list players within <radius> of <steam id/player name/entity id> last <numberOfHours>\n" +
                "2. list players within <radius> of <x> <y> <z> last <numberOfHours> (use y=-1 to ommit y search)\n" +
                "3. show location tracks of <steam id>. Maximum records <maxrecords>.\n" +
                "   Minimum time between records <timeBetweenRecords> (seconds). Within <timespan> (MMddHHmm-MMddHHmm).\n" +
                "4. Enable / Disable location recording\n" +
                "5. Define the ingame chatcommand (include the prefix)\n" +
                "6. Enable/Disable ingame chatcommand for querying databases\n" +
                "7. Set the interval of recording locations (seconds)\n" +
                "8. Set the maximum time the data stays in databases (hours)(0=forever)\n" +
                "9. Set the distance for use with chatcommand for reporting near live players\n" +
                "10. Set the responsecolor for use with chatcommand. Has to be 6 long hexadecimal number\n" +
                "11. Show all active location tracking settings";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count >= 1)
            {
                switch (_params[0].ToLower().Trim())
                {
                    case "search":
                        ExecuteSearch(_params);
                        break;
                    case "enabled":
                        SetEnabled(_params);
                        break;
                    case "showtrack":
                        GetTrack(_params);
                        break;
                    case "command":
                        SetCommand(_params);
                        break;
                    case "commandenabled":
                        SetCommandEnabled(_params);
                        break;
                    case "interval":
                        SetInterval(_params);
                        break;
                    case "maxagedata":
                        SetMaxAgeData(_params);
                        break;
                    case "neardistance":
                        SetNearDistance(_params);
                        break;
                    case "responsecolor":
                        SetColor(_params);
                        break;
                    default:
                        SdtdConsole.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
                        return;
                }
            }
            else
            {
                SdtdConsole.Instance.Output("Active location tracker settings:");
                SdtdConsole.Instance.Output(string.Format("Enabled: {0}", PrismaCoreSettings.Instance.LocationTracker_Enabled));
                SdtdConsole.Instance.Output(string.Format("ChatCommand: {0}", PrismaCoreSettings.Instance.LocationTracker_ChatCommand));
                SdtdConsole.Instance.Output(string.Format("CommandEnabled: {0}", PrismaCoreSettings.Instance.LocationTracker_ChatCommandEnabled));
                SdtdConsole.Instance.Output(string.Format("RecordingInterval: {0} seconds", PrismaCoreSettings.Instance.LocationTracker_RecordingIntervalSeconds));
                SdtdConsole.Instance.Output(string.Format("MaxAgeData: {0} hours", PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours));
                SdtdConsole.Instance.Output(string.Format("NearDistance: {0} meters", PrismaCoreSettings.Instance.LocationTracker_NearDistance));
                SdtdConsole.Instance.Output(string.Format("ResponseColor: {0}", PrismaCoreSettings.Instance.LocationTracker_ResponseColor));
            }
        }

        private void ExecuteSearch(List<string> _params)
        {
            if (_params.Count == 4)
            {
                ClientInfo clientinfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                if (clientinfo == null)
                {
                    SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                    return;
                }

                EntityPlayer player = GameManager.Instance.World.Players.dict[clientinfo.entityId];
                int x = (int)Math.Floor(player.GetPosition().x);
                int y = (int)Math.Floor(player.GetPosition().y);
                int z = (int)Math.Floor(player.GetPosition().z);

                int radius = int.MinValue;
                if (!int.TryParse(_params[2], out radius))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter radius is not a valid integer.");
                    return;
                }

                int hours = int.MinValue;
                if (!int.TryParse(_params[3], out hours))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter numberOfHours is not a valid integer.");
                    return;
                }

                if (radius < 1)
                {
                    // only radius > 0 allowed
                    SdtdConsole.Instance.Output("ERR: Radius must be greater than 0");
                    return;
                }

                if (hours < 1)
                {
                    // only hours > 0 allowed
                    SdtdConsole.Instance.Output("ERR: numberOfHours must be greater than 0");
                    return;
                }

                string msg = string.Format("Players within {0} meters (last {1} hours) of player {2}", radius.ToString(), hours.ToString(), clientinfo.playerName);
                SdtdConsole.Instance.Output(msg);
                DoWhoCommand(x, y, z, radius, hours, clientinfo.playerName);
            }
            else if (_params.Count == 6)
            {
                int x = int.MinValue;
                if (!int.TryParse(_params[1], out x))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter x is not a valid integer.");
                    return;
                }

                int y = int.MinValue;
                if (!int.TryParse(_params[2], out y))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter y is not a valid integer.");
                    return;
                }

                int z = int.MinValue;
                if (!int.TryParse(_params[3], out z))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter z is not a valid integer.");
                    return;
                }

                int radius = int.MinValue;
                if (!int.TryParse(_params[4], out radius))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter radius is not a valid integer.");
                    return;
                }

                int hours = int.MinValue;
                if (!int.TryParse(_params[5], out hours))
                {
                    SdtdConsole.Instance.Output("ERR: Parameter numberOfHours is not a valid integer.");
                    return;
                }

                if (radius < 1)
                {
                    // only radius > 0 allowed
                    SdtdConsole.Instance.Output("ERR: Radius must be greater than 0");
                    return;
                }

                if (hours < 1)
                {
                    // only hours > 0 allowed
                    SdtdConsole.Instance.Output("ERR: numberOfHours must be greater than 0");
                    return;
                }

                string msg = string.Format("Players within {0} meters (last {1} hours) of coordinate {2},{3},{4}", radius.ToString(), hours.ToString(), x, y, z);
                SdtdConsole.Instance.Output(msg);
                DoWhoCommand(x, y, z, radius, hours);
            }
            else
            {
                SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments. Expected 4 or 6. Received: {0}", _params.Count));
            }
        }

        private void SetEnabled(List<string> _params)
        {
            bool enabled;
            if (!bool.TryParse(_params[1].ToLower().Trim(), out enabled))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for enabled is not true/false. Parameter: {0}", _params[1]));
            else
            {
                PrismaCoreSettings.Instance.LocationTracker_Enabled = enabled;
                if (enabled) LocationTracker.Start();
                else LocationTracker.Unload();
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("Enabled has been set to {0}", enabled));
            }
        }

        private void SetCommand(List<string> _params)
        {
            string command = string.Empty;

            if (string.IsNullOrEmpty(_params[1].ToLower().Trim()))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for command is not valid. Parameter: {0}", _params[1]));
            else
            {
                command = _params[1].ToLower().Trim();

                PrismaCoreSettings.Instance.LocationTracker_ChatCommand = command;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("Command has been set to {0}", command));
            }
        }

        private void SetColor(List<string> _params)
        {
            string color = string.Empty;

            if (string.IsNullOrEmpty(_params[1].ToLower().Trim()))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for responsecolor is not valid or empty. Parameter: {0}", _params[1]));
            else
            {
                color = _params[1].ToLower().Trim();
                if (color.Length != 6 || !OnlyHexInString(color))
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: The parameter for responsecolor is not a valid hexadecimal number. Parameter: {0}", _params[1]));
                    return;
                }
                PrismaCoreSettings.Instance.LocationTracker_ResponseColor = color;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("ResponseColor has been set to {0}", color));
            }
        }

        private void SetCommandEnabled(List<string> _params)
        {
            bool enabled;
            if (!bool.TryParse(_params[1].ToLower().Trim(), out enabled))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for enabled is not true/false. Parameter: {0}", _params[1]));
            else
            {
                PrismaCoreSettings.Instance.LocationTracker_ChatCommandEnabled = enabled;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("CommandEnabled has been set to {0}", enabled));
            }
        }

        private void SetInterval(List<string> _params)
        {
            int interval = int.MinValue;
            if (!int.TryParse(_params[1].Trim(), out interval))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for interval is not a valid integer. Parameter: {0}", _params[1]));
            else
            {
                PrismaCoreSettings.Instance.LocationTracker_RecordingIntervalSeconds = interval;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("Interval has been set to {0}", interval));
            }
        }

        private void GetTrack(List<string> _params)
        {
            string from = string.Empty;
            string to = string.Empty;
            DateTime fromDateTime;
            DateTime toDateTime;

            //ClientInfo clientinfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
            //if (clientinfo == null)
            //{
            //    SdtdConsole.Instance.Output("Playername or entity id not found.");
            //    return;
            //}

            if (_params[1].Length != 23)
            {
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for steamid is not a valid steamid."));
            }

            DbPlayer p = Database.Instance.GetDbPlayer(_params[1].Replace("steam_", "Steam_"));

            if (p == null)
            {
                SdtdConsole.Instance.Output("ERR: Player could not be found.");
                return;
            }

            int maxrecords = int.MinValue;
            if (!int.TryParse(_params[2].Trim(), out maxrecords))
            {
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for maxrecords is not a valid integer."));
                return;
            }

            if (!_params[3].Trim().Contains("-"))
            {
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for timespan has incorrect format (MMddhhmm-MMddhhmm)."));
                return;
            }
            else
            {
                from = _params[3].Trim().Split('-')[0];
                to = _params[3].Trim().Split('-')[1];
                fromDateTime = DateTime.ParseExact(from, "MMddHHmm", null);
                toDateTime = DateTime.ParseExact(to, "MMddHHmm", null);
                if (fromDateTime == null || toDateTime == null)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: The parameter for timespan has incorrect format (MMddhhmm-MMddhhmm)."));
                    return;
                }
            }

            int timeBetweenRecords = int.MinValue;
            if (!int.TryParse(_params[4].Trim(), out timeBetweenRecords))
            {
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for timeBetweenRecords is not a valid integer."));
                return;
            }

            SdtdConsole.Instance.Output(string.Format("Location track of player {0} MaxRecords: {1} TimeSpan: {2} TimeBetweenRecords: {3}", p.Name, maxrecords, _params[3].Trim(), timeBetweenRecords));
            PrintTrack(p, maxrecords, fromDateTime, toDateTime, timeBetweenRecords);
        }

        private void PrintTrack(DbPlayer p, int maxrec, DateTime from, DateTime to, int timeBetween)
        {
            bool recordFound = false;
            int i = 0;
            DateTime prevDateTime = new DateTime(2030, 12, 1);

            using (var db = new LiteDatabase(string.Format("{0}/{1}.db", LocationTracker.StatisticsPath, p.Id)))
            {
                var col = db.GetCollection<PlayerLocation>("playerlocation");

                var results = col.Find(x => x.Dt.CompareTo(from) > 0 && x.Dt.CompareTo(to) < 0);

                foreach (PlayerLocation loc in results)
                {
                    recordFound = true;
                    if (prevDateTime.Year == 2030 && prevDateTime.Month == 12 && prevDateTime.Day == 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("{0:yyyy-MM-dd HH:mm:ss}  {1},{2},{3}", loc.Dt, loc.X, loc.Y, loc.Z));
                        prevDateTime = loc.Dt;
                        i++;
                    }
                    else
                    {
                        //start checking timebetween and skip if below minimum
                        if ((loc.Dt - prevDateTime).TotalSeconds < timeBetween) continue;
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("{0:yyyy-MM-dd HH:mm:ss}  {1},{2},{3}", loc.Dt, loc.X, loc.Y, loc.Z));
                            prevDateTime = loc.Dt;
                            i++;
                        }

                    }
                    if (i > maxrec) break;
                }

                if (!recordFound) SdtdConsole.Instance.Output(string.Format("No location data available for given timespan"));
            }


        }

        private void SetMaxAgeData(List<string> _params)
        {
            int maxagedata = int.MinValue;
            if (!int.TryParse(_params[1].Trim(), out maxagedata))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for maxagedata is not a valid integer. Parameter: {0}", _params[1]));
            else
            {
                PrismaCoreSettings.Instance.LocationTracker_MaximumDataAgeHours = maxagedata;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("MaxAgeData has been set to {0}", maxagedata));
            }
        }

        private void SetNearDistance(List<string> _params)
        {
            int neardistance = int.MinValue;
            if (!int.TryParse(_params[1].Trim(), out neardistance))
                SdtdConsole.Instance.Output(string.Format("ERR: The parameter for neardistance is not a valid integer. Parameter: {0}", _params[1]));
            else
            {
                PrismaCoreSettings.Instance.LocationTracker_NearDistance = neardistance;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output(string.Format("NearDistance has been set to {0}", neardistance));
            }
        }

        private static void DoWhoCommand(int xp, int yp, int zp, int r, int t, string name)
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
                        if (res.Name != name)
                        {
                            playerFound = true;
                            SdtdConsole.Instance.Output(res.Name);
                        }
                    }
                }
            }
            if (!playerFound) SdtdConsole.Instance.Output(string.Format("No players recorded within {0} meters of player {1} yet.", r, name));
        }

        private static void DoWhoCommand(int xp, int yp, int zp, int r, int t)
        {
            DirectoryInfo d = new DirectoryInfo(LocationTracker.StatisticsPath);
            bool playerFound = false;
            foreach (var file in d.GetFiles("*.db"))
            {
                using (var db = new LiteDatabase(file.FullName))
                {
                    var count = 0;
                    var col = db.GetCollection<PlayerLocation>("playerlocation");
                    if (yp != -1)
                    {
                        count = col.Count(x => (DateTime.Now - x.Dt).TotalHours < t && Math.Abs(x.X - xp) < r && Math.Abs(x.Y - yp) < r && Math.Abs(x.Z - zp) < r);
                    }
                    else
                    {
                        count = col.Count(x => (DateTime.Now - x.Dt).TotalHours < t && Math.Abs(x.X - xp) < r && Math.Abs(x.Z - zp) < r);
                    }

                    if (count > 0)
                    {
                        //nameresolving
                        var colID = db.GetCollection<Identifiers>("identifiers");
                        var res = colID.FindOne(Query.All(Query.Descending));
                        playerFound = true;
                        SdtdConsole.Instance.Output(res.Name);
                    }
                }
            }
            if (!playerFound)
            {
                if (yp != -1)
                    SdtdConsole.Instance.Output(string.Format("No players recorded within {0} meters of coordinate {1},{2},{3} yet.", r, xp, yp, zp));
                else
                    SdtdConsole.Instance.Output(string.Format("No players recorded within {0} meters of coordinate {1},{2} yet.", r, xp, zp));
            }

        }

        private static bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}
