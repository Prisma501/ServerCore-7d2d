using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerCore
{
    public class PermaDeathClass
    {
        public static SortedDictionary<string, string> DictPermaDeath = new SortedDictionary<string, string>();
        public const string file = "PrismaCorePermaDeath.xml";
        public static string filePath = string.Format("{0}/{1}", API.GamePath, file);


        public static void Loadxml()
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
                if (childNode.Name == "PermaDeathPlayers")
                {
                    DictPermaDeath.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[PrismaCore] Unexpected XML node found in 'PermaDeathPlayers' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("SteamId"))
                        {
                            Log.Warning(string.Format("[PrismaCore] Ignoring player entry because of missing 'steamid' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("name"))
                        {
                            Log.Warning(string.Format("[PrismaCore] Ignoring player entry because of missing 'name' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        string _steamid = _line.GetAttribute("SteamId");
                        if (!DictPermaDeath.ContainsKey(_steamid))
                        {
                            DictPermaDeath.Add(_steamid, _line.GetAttribute("name"));
                        }
                    }
                }
            }
        }

        public static void UpdateXml()
        {
            RegionWatcher.fileWatcher.EnableRaisingEvents = false;

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<PermaDeath>");
                sw.WriteLine("    <PermaDeathPlayers>");
                sw.WriteLine("        <!-- <Player SteamId=\"Steam_76560000000000000\" name=\"Prisma501\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"Steam_76560580000000000\" name=\"You\" /> -->");
                sw.WriteLine("        <!-- <Player SteamId=\"Steam_76574740000000000\" name=\"\" /> -->");
                foreach (KeyValuePair<string, string> _key in DictPermaDeath)
                {
                    sw.WriteLine(string.Format("        <Player SteamId=\"{0}\" name=\"{1}\" />", _key.Key, _key.Value));
                }
                sw.WriteLine("    </PermaDeathPlayers>");
                sw.WriteLine("</PermaDeath>");
                sw.Flush();
                sw.Close();
            }
            RegionWatcher.fileWatcher.EnableRaisingEvents = true;
        }
    }
}