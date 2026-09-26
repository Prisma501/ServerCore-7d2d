using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ReservedSlot : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Add, Remove and View platformIDs on the ReservedSlots list.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. ds add <platformID> <playerName> <days to expire>\n" +
                   "  2. ds remove <platformID>\n" +
                   "  3. ds list\n" +
                   "  4. ds donorbuffer <number of players>\n" +
                   "  5. ds enabled <true/false>\n" +
                   "1. Adds a platformID to the DonorSlots list\n" +
                   "2. Removes a platformID from the DonorSlots list\n" +
                   "3. Lists all platformIDs that have a DonorSlot\n" +
                   "4. Set the number of donor slots for donors\n" +
                   "5. Enable/Disable donor slots for donors";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-ds", "donorslot", "ds" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 4)
                {
                    SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1 to 4, found {0}", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count != 4)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 4, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Trim().Length != 23 && _params[1].Trim().Length != 44)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Can not add platformID. You MUST use steamID/XblId: Invalid SteamId/XblId {0}", _params[1].Trim()));
                        return;
                    }
                    int _daysToExpire;
                    string steamId = string.Empty;

                    if (_params[1].Trim().ToLower().StartsWith("steam_"))
                    {
                        steamId = _params[1].Replace("steam_", "Steam_");
                    }
                    else if (_params[1].Trim().ToLower().StartsWith("xbl_"))
                    {
                        steamId = _params[1].Replace("xbl_", "XBL_");
                    }

                    if (!int.TryParse(_params[3], out _daysToExpire))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Invalid days to expire: {0}", _params[3]));
                        return;
                    }
                    DateTime _expireDate;
                    if (_daysToExpire > 0d)
                    {
                        _expireDate = DateTime.Now.AddDays(_daysToExpire);
                    }
                    else
                    {
                        _expireDate = DateTime.Now.AddDays(18250d);
                    }

                    if (ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        //isdonor or donor-expired
                        DateTime _dt;
                        ReservedSlots.Dict.TryGetValue(steamId, out _dt);
                        if (DateTime.Now <= _dt)
                        {
                            //active donor
                            int newDays = 0;
                            TimeSpan ts = _dt - DateTime.Now;
                            int days = Math.Abs(ts.Days) + 1;
                            newDays = days + _daysToExpire;

                            DateTime expDate = DateTime.Now.AddDays(newDays);

                            ReservedSlots.Dict.Remove(steamId);
                            ReservedSlots.Dict1.Remove(steamId);
                            ReservedSlots.Dict.Add(steamId, expDate);
                            ReservedSlots.Dict1.Add(steamId, _params[2]);
                            SdtdConsole.Instance.Output(string.Format("Active donor {0} with name {1} with {2} days donorship left, has been granted {3} more days. New end of donorship: {4}", steamId, _params[2], days, _daysToExpire, expDate.ToString()));
                            ReservedSlots.UpdateXml();
                            return;
                        }
                        else
                        {
                            //expired donor
                            ReservedSlots.Dict.Remove(steamId);
                            ReservedSlots.Dict1.Remove(steamId);
                            ReservedSlots.Dict.Add(steamId, _expireDate);
                            ReservedSlots.Dict1.Add(steamId, _params[2]);
                            SdtdConsole.Instance.Output(string.Format("Expired donor {0} with name {1} has been reactivated. New end of donorship: {2}", steamId, _params[2], _expireDate.ToString()));
                            ReservedSlots.UpdateXml();
                            return;
                        }
                    }
                    else
                    {
                        //non-donor
                        ReservedSlots.Dict.Add(steamId, _expireDate);
                        ReservedSlots.Dict1.Add(steamId, _params[2]);
                        SdtdConsole.Instance.Output(string.Format("Added platformID {0} with name {1} that expires on {2} to the DonorSlots list.", steamId, _params[2], _expireDate.ToString()));
                        ReservedSlots.UpdateXml();
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }

                    if (_params[1].Trim().Length != 23 && _params[1].Trim().Length != 44)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Can not remove platformId. You MUST use steamID/XblId: Invalid SteamId/XblId {0}", _params[1].Trim()));
                        return;
                    }

                    string steamId = string.Empty;

                    if (_params[1].Trim().ToLower().StartsWith("steam_"))
                    {
                        steamId = _params[1].Replace("steam_", "Steam_");
                    }
                    else if (_params[1].Trim().ToLower().StartsWith("xbl_"))
                    {
                        steamId = _params[1].Replace("xbl_", "XBL_");
                    }

                    if (!ReservedSlots.Dict.ContainsKey(steamId))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: platformID {0} was not found.", steamId));
                        return;
                    }
                    ReservedSlots.Dict.Remove(steamId);
                    ReservedSlots.Dict1.Remove(steamId);
                    SdtdConsole.Instance.Output(string.Format("Removed platformID {0} from DonorSlots list.", steamId));
                    ReservedSlots.UpdateXml();
                }
                else if (_params[0].ToLower().Equals("donorbuffer"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }

                    if (!int.TryParse(_params[1], out ReservedSlots.DonorBuffer))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Parameter donorbuffer is not a valid integer: ", _params[1]));
                        return;

                    }
                    SdtdConsole.Instance.Output(string.Format("DonorBuffer is set to {0}", _params[1]));
                    ReservedSlots.UpdateXml();
                }
                else if (_params[0].ToLower().Equals("enabled"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 2, found {0}", _params.Count));
                        return;
                    }

                    if (!bool.TryParse(_params[1], out ReservedSlots.Enabled))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Parameter Enabled is not a valid boolean: ", _params[1]));
                        return;

                    }
                    SdtdConsole.Instance.Output(string.Format("DonorSlots enabled is set to {0}", _params[1]));
                    ReservedSlots.UpdateXml();
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (ReservedSlots.Dict.Count < 1)
                    {
                        SdtdConsole.Instance.Output("ERR: There are no platformIDs on the DonorSlots list.");
                        SdtdConsole.Instance.Output(string.Format("DonorSlots enabled: {0}", ReservedSlots.Enabled));
                        SdtdConsole.Instance.Output(string.Format("DonorBuffer size: {0}", ReservedSlots.DonorBuffer));
                        return;
                    }
                    foreach (KeyValuePair<string, DateTime> _key in ReservedSlots.Dict)
                    {
                        string _name;
                        if (ReservedSlots.Dict1.TryGetValue(_key.Key, out _name))
                        {
                            SdtdConsole.Instance.Output(string.Format("{0} {1} {2}", _key.Key, _name, _key.Value));
                        }
                    }

                    SdtdConsole.Instance.Output(string.Format("DonorSlots enabled: {0}", ReservedSlots.Enabled));
                    SdtdConsole.Instance.Output(string.Format("DonorBuffer size: {0}", ReservedSlots.DonorBuffer));
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in DonorSlots.Run: {0}.", e));
            }
        }
    }
}