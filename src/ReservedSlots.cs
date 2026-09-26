using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerCore
{
    public class ReservedSlots
    {
        public static SortedDictionary<string, DateTime> Dict = new SortedDictionary<string, DateTime>();
        public static SortedDictionary<string, string> Dict1 = new SortedDictionary<string, string>();
        public static SortedDictionary<string, string> dicWelcomeStatus = new SortedDictionary<string, string>();
        public static string file = "PrismaCoreDonorSlots.xml";
        private static string filePath = $"{API.GamePath}/{file}";
        public static int DonorBuffer = 8;
        public static bool Enabled = false;

        public static void LoadXml()
        {
            if (!File.Exists(filePath))
            {
                UpdateXml();
            }

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[PrismaCore] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Settings")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[PrismaCore] Unexpected XML node found in 'Settings' section: {0}", subChild.OuterXml));
                            continue;
                        }

                        XmlElement _line = (XmlElement)subChild;
                        if (_line.HasAttribute("Enabled"))
                        {
                            bool.TryParse(_line.GetAttribute("Enabled"), out bool enabled);
                            Enabled = enabled;
                            continue;
                        }

                        if (_line.HasAttribute("Donorbuffer"))
                        {
                            int.TryParse(_line.GetAttribute("Donorbuffer"), out int buffer);
                            DonorBuffer = buffer;
                            continue;
                        }
                    }
                }

                if (childNode.Name == "Donors")
                {
                    Dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[PrismaCore] Unexpected XML node found in 'Donors' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[PrismaCore] Ignoring Donor entry because of missing 'SteamId' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Name"))
                        {
                            Log.Warning(string.Format("[PrismaCore] Ignoring Donor entry because of missing 'Name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("EndOfDonorship"))
                        {
                            Log.Warning(string.Format("[PrismaCore] Ignoring Donor entry because of missing 'EndOfDonorship' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        DateTime _dt;
                        if (_line.GetAttribute("EndOfDonorship") == "")
                        {
                            _dt = DateTime.Parse("04/21/2025 00:00:00 AM");
                        }
                        else
                        {
                            if (!DateTime.TryParse(_line.GetAttribute("EndOfDonorship"), out _dt))
                            {
                                Log.Warning(string.Format("[PrismaCore] Ignoring Player entry because of invalid (date) value for 'EndOfDonorship' attribute: {0}", subChild.OuterXml));
                                continue;
                            }

                        }
                        if (!Dict.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            Dict.Add(_line.GetAttribute("SteamId"), _dt);
                        }
                        if (!Dict1.ContainsKey(_line.GetAttribute("SteamId")))
                        {
                            Dict1.Add(_line.GetAttribute("SteamId"), _line.GetAttribute("Name"));
                        }
                    }
                }
            }
        }

        public static void WelcomePlayer(ClientInfo ci)
        {
            if (dicWelcomeStatus.ContainsKey(ci.PlatformId.ToString()))
            {
                DbPlayer player = Database.Instance.GetDbPlayer(ci.PlatformId.ToString());

                TimeSpan t = TimeSpan.FromSeconds(player.TotalPlaytime);

                string playtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                t.Hours,
                                t.Minutes,
                                t.Seconds);

                dicWelcomeStatus.TryGetValue(ci.PlatformId.ToString(), out string type);

                switch (type)
                {
                    case "admin":
                        string pMsg = string.Format("[ffff4d]Welcome [4da6ff]{0}[-], you are connecting to your admin slot.[-]", ci.playerName);

                        GameManager.Instance.ChatMessageServer(ci, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, pMsg), null, EMessageSender.None);
                        
                        dicWelcomeStatus.Remove(ci.PlatformId.ToString());
                        break;
                    case "donor":
                        //donor msg
                        DateTime _dt;
                        Dict.TryGetValue(ci.PlatformId.ToString(), out _dt);

                        string dMsg = ServerCoreStrings.Instance.DonorSlots_WelcomeDonor;
                        dMsg = dMsg.Replace("{playerName}", ci.playerName);
                        dMsg = dMsg.Replace("{endOfDonorship}", _dt.ToShortDateString());
                        
                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, dMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        dicWelcomeStatus.Remove(ci.PlatformId.ToString());
                        break;
                    case "expired":
                        //expired msg
                        DateTime _dt2;
                        Dict.TryGetValue(ci.PlatformId.ToString(), out _dt2);

                        string edMsg = ServerCoreStrings.Instance.DonorSlots_WelcomeDonorExpired;
                        edMsg = edMsg.Replace("{playerName}", ci.playerName);
                        edMsg = edMsg.Replace("{endOfDonorship}", _dt2.ToShortDateString());
                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, edMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        dicWelcomeStatus.Remove(ci.PlatformId.ToString());
                        break;
                    case "nondonor":
                        //new or known player msg
                        if (Database.Instance.TotalPlayTime(ci.PlatformId.ToString()) > 40)
                        {
                            //existing nondonor
                            string endMsg = string.Format("[ffff4d]Welcome back [4da6ff]{0}[-][-]", ci.playerName);
                            GameManager.Instance.ChatMessageServer(ci, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, endMsg), null, EMessageSender.None);
                        }
                        else
                        {
                            //new nondonor
                            string gndMsg = string.Format("[99ff33]We have a new player: [4da6ff]{0}[-][-]", ci.playerName);
                            GameManager.Instance.ChatMessageServer(ci, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, gndMsg), null, EMessageSender.None);
                            
                            string pndMsg = string.Format("[ffff4d]Welcome [4da6ff]{0}[-][-]", ci.playerName);
                            GameManager.Instance.ChatMessageServer(ci, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, pndMsg), null, EMessageSender.None);
                        }
                        dicWelcomeStatus.Remove(ci.PlatformId.ToString());
                        break;
                }
            }
        }

        public static bool IsDonor(ClientInfo _cInfo)
        {
            if (Dict.ContainsKey(_cInfo.PlatformId.ToString()))
            {
                //isdonor or donor-expired
                DateTime _dt;
                Dict.TryGetValue(_cInfo.PlatformId.ToString(), out _dt);
                if (DateTime.Now <= _dt)
                {
                    return true; //donor
                }
                else
                {
                    return false; //expired donor
                }
            }
            else
            {
                //non-donor
                return false;
            }
        }

        public static void CheckReservedSlot(ClientInfo _cInfo)
        {
            int _playerCount = ConnectionManager.Instance.ClientCount();

            //admin, non-donor, donor with time, donor with expired slot, GEO ip check, completely new players

            if (dicWelcomeStatus.ContainsKey(_cInfo.PlatformId.ToString())) dicWelcomeStatus.Remove(_cInfo.PlatformId.ToString());

            if (GameManager.Instance.adminTools.Users.HasEntry(_cInfo))
            {
                return;
            }

            //identify type of player (new, non-donor, donor, donor-expired)

            if (Dict.ContainsKey(_cInfo.PlatformId.ToString()))
            {
                //isdonor or donor-expired
                DateTime _dt;
                Dict.TryGetValue(_cInfo.PlatformId.ToString(), out _dt);
                if (DateTime.Now <= _dt)
                {
                    dicWelcomeStatus.Add(_cInfo.PlatformId.ToString(), "donor");
                }
                else
                {
                    //log for CSMM hook
                    Log.Out($"[PrismaCore] Donorslot expired! Playername: {_cInfo.playerName} SteamID: {_cInfo.PlatformId}");

                    AlertAdmins(_cInfo);

                    if (_playerCount > API.MaxPlayers - DonorBuffer)
                    {
                        //server full -> kick expired donor
                        string kickMsg = ServerCoreStrings.Instance.DonorSlots_KickDonorExpired;
                        kickMsg = kickMsg.Replace("{playerName}", _cInfo.playerName);
                        kickMsg = kickMsg.Replace("{endOfDonorship}", _dt.ToShortDateString());

                        SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.PlatformId, kickMsg), _cInfo);
                    }
                    else dicWelcomeStatus.Add(_cInfo.PlatformId.ToString(), "expired");
                }

            }
            else
            {
                //non-donor
                if (_playerCount > API.MaxPlayers - DonorBuffer)
                {
                    //server full -> kick nondonor
                    string maxplayers = (API.MaxPlayers - DonorBuffer).ToString();
                    int maxminusone = API.MaxPlayers - DonorBuffer - 1;

                    string kickMsg = ServerCoreStrings.Instance.DonorSlots_Kick;
                    kickMsg = kickMsg.Replace("{playerName}", _cInfo.playerName);
                    kickMsg = kickMsg.Replace("{maxPlayers}", maxplayers);
                    kickMsg = kickMsg.Replace("{maxMinusOne}", maxminusone.ToString());
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"{1}\"", _cInfo.PlatformId, kickMsg), _cInfo);

                }
            }
        }

        public static void UpdateXml()
        {
            RegionWatcher.fileWatcherDonorSlots.EnableRaisingEvents = false;

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<DonorSlots>");
                sw.WriteLine("    <Settings>");
                sw.WriteLine($"        <Setting Enabled=\"{Enabled}\"/>");
                sw.WriteLine($"        <Setting Donorbuffer=\"{DonorBuffer.ToString()}\"/>");
                sw.WriteLine("    </Settings>");
                sw.WriteLine("    <Donors>");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in Dict)
                    {
                        string _name = "";
                        Dict1.TryGetValue(kvp.Key, out _name);
                        sw.WriteLine(string.Format("        <Donor SteamId=\"{0}\" Name=\"{1}\" EndOfDonorship=\"{2}\" />", kvp.Key, _name, kvp.Value.ToString()));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <!--<Donor SteamId=\"Steam_76561159484330234\" Name=\"playerName\" EndOfDonorship=\"04/21/2025 00:00:00 AM\" />-->"));
                }
                sw.WriteLine("    </Donors>");
                sw.WriteLine("</DonorSlots>");
                sw.Flush();
                sw.Close();
            }
            RegionWatcher.fileWatcherDonorSlots.EnableRaisingEvents = true;
        }

        private static void AlertAdmins(ClientInfo ci)
        {
            World w = GameManager.Instance.World;
            if (w == null) return;

            string msg = $"[F7FE2E]Donorslot expired! Playername: [4da6ff]{ci.playerName}[-] SteamID: [4da6ff]{ci.PlatformId}[-][-]";

            foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
            {
                ClientInfo ci2 = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                if (ci2 == null) continue;

                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(ci2);

                if (AdminLvL <= ServerCoreSettings.Instance.NotifyAdmin_Level)
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
        }
    }
}