using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace PrismaCore.Web
{
    public class WebPermissions
    {
        private const string PERMISSIONS_FILE = "ClaimCreator_permissions.xml";
        private static WebPermissions instance;
        private readonly WebModulePermission defaultModulePermission = new WebModulePermission("", 0);

        private readonly Dictionary<string, WebModulePermission> knownModules =
            new CaseInsensitiveStringDictionary<WebModulePermission>();

        private readonly Dictionary<string, AdminToken> admintokens = new CaseInsensitiveStringDictionary<AdminToken>();
        private FileSystemWatcher fileWatcher;

        private readonly Dictionary<string, WebModulePermission> modules = new CaseInsensitiveStringDictionary<WebModulePermission>();

        public WebPermissions()
        {
            allModulesList = new List<WebModulePermission>();
            allModulesListRO = new ReadOnlyCollection<WebModulePermission>(allModulesList);
            Directory.CreateDirectory(GetFilePath());
            InitFileWatcher();
            Load();
        }

        public static WebPermissions Instance
        {
            get
            {
                lock (typeof(WebPermissions))
                {
                    if (instance == null)
                    {
                        instance = new WebPermissions();
                    }

                    return instance;
                }
            }
        }

        public bool ModuleAllowedWithLevel(string _module, int _level)
        {
            WebModulePermission permInfo = GetModulePermission(_module);
            return permInfo.permissionLevel >= _level;
        }

        public AdminToken GetWebAdmin(string _name, string _token)
        {
            if (IsAdmin(_name) && admintokens[_name].token == _token)
            {
                return admintokens[_name];
            }

            return null;
        }

        public WebModulePermission GetModulePermission(string _module)
        {
            WebModulePermission result;
            if (modules.TryGetValue(_module, out result))
            {
                return result;
            }

            if (knownModules.TryGetValue(_module, out result))
            {
                return result;
            }

            return defaultModulePermission;
        }


        // Admins
        public void AddAdmin(string _name, string _token, int _permissionLevel, bool _save = true)
        {
            AdminToken c = new AdminToken(_name, _token, _permissionLevel);
            lock (this)
            {
                admintokens[_name] = c;
                if (_save)
                {
                    Save();
                }
            }
        }

        public void RemoveAdmin(string _name, bool _save = true)
        {
            lock (this)
            {
                admintokens.Remove(_name);
                if (_save)
                {
                    Save();
                }
            }
        }

        public bool IsAdmin(string _name)
        {
            return admintokens.ContainsKey(_name);
        }

        public AdminToken[] GetAdmins()
        {
            AdminToken[] result = new AdminToken[admintokens.Count];
            admintokens.CopyValuesTo(result);
            return result;
        }


        // Commands
        public void AddModulePermission(string _module, int _permissionLevel, bool _save = true)
        {
            WebModulePermission p = new WebModulePermission(_module, _permissionLevel);
            lock (this)
            {
                allModulesList.Clear();
                modules[_module] = p;
                if (_save)
                {
                    Save();
                }
            }
        }

        public void AddKnownModule(string _module, int _defaultPermission)
        {
            if (string.IsNullOrEmpty(_module))
            {
                return;
            }

            WebModulePermission p = new WebModulePermission(_module, _defaultPermission);

            lock (this)
            {
                allModulesList.Clear();
                knownModules[_module] = p;
            }
        }

        public bool IsKnownModule(string _module)
        {
            if (string.IsNullOrEmpty(_module))
            {
                return false;
            }

            lock (this)
            {
                return knownModules.ContainsKey(_module);
            }

        }

        public void RemoveModulePermission(string _module, bool _save = true)
        {
            lock (this)
            {
                allModulesList.Clear();
                modules.Remove(_module);
                if (_save)
                {
                    Save();
                }
            }
        }

        private readonly List<WebModulePermission> allModulesList;
        private readonly ReadOnlyCollection<WebModulePermission> allModulesListRO;

        public IList<WebModulePermission> GetModules()
        {
            if (allModulesList.Count == 0)
            {
                foreach (KeyValuePair<string, WebModulePermission> kvp in knownModules)
                {
                    if (modules.ContainsKey(kvp.Key))
                    {
                        allModulesList.Add(modules[kvp.Key]);
                    }
                    else
                    {
                        allModulesList.Add(kvp.Value);
                    }
                }
            }

            return allModulesListRO;
        }


        private void InitFileWatcher()
        {
            fileWatcher = new FileSystemWatcher(GetFilePath(), GetFileName());
            fileWatcher.Changed += OnFileChanged;
            fileWatcher.Created += OnFileChanged;
            fileWatcher.Deleted += OnFileChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object _source, FileSystemEventArgs _e)
        {
            Log.Out("Reloading " + PERMISSIONS_FILE);
            Load();
        }

        private string GetFilePath()
        {
            return GameIO.GetSaveGameRootDir();
        }

        private string GetFileName()
        {
            return PERMISSIONS_FILE;
        }

        private string GetFullPath()
        {
            return GetFilePath() + "/" + GetFileName();
        }

        public void Load()
        {
            admintokens.Clear();
            modules.Clear();

            if (!File.Exists(GetFullPath()))
            {
                Log.Out(string.Format("[PrismaCore] Permissions file '{0}' not found, creating.", GetFileName()));
                Save();
                return;
            }

            Log.Out(string.Format("[PrismaCore] Loading permissions file at '{0}'", GetFullPath()));

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(GetFullPath());
            }
            catch (XmlException e)
            {
                Log.Error("[PrismaCore] Failed loading permissions file: " + e.Message);
                return;
            }

            XmlNode adminToolsNode = xmlDoc.DocumentElement;

            if (adminToolsNode == null)
            {
                Log.Error("[PrismaCore] Failed loading permissions file: No DocumentElement found");
                return;
            }

            foreach (XmlNode childNode in adminToolsNode.ChildNodes)
            {
                if (childNode.Name == "apiusers")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning("Unexpected XML node found in 'apiusers' section: " + subChild.OuterXml);
                            continue;
                        }

                        XmlElement lineItem = (XmlElement)subChild;

                        if (!lineItem.HasAttribute("username"))
                        {
                            Log.Warning("Ignoring apiuser-entry because of missing 'username' attribute: " +
                                         subChild.OuterXml);
                            continue;
                        }

                        if (!lineItem.HasAttribute("password"))
                        {
                            Log.Warning("Ignoring apiuser-entry because of missing 'password' attribute: " +
                                         subChild.OuterXml);
                            continue;
                        }

                        if (!lineItem.HasAttribute("permission_level"))
                        {
                            Log.Warning("Ignoring admintoken-entry because of missing 'permission_level' attribute: " +
                                         subChild.OuterXml);
                            continue;
                        }

                        string username = lineItem.GetAttribute("username");
                        string password = lineItem.GetAttribute("password");
                        int permissionLevel;
                        if (!int.TryParse(lineItem.GetAttribute("permission_level"), out permissionLevel))
                        {
                            Log.Warning(
                                "Ignoring admintoken-entry because of invalid (non-numeric) value for 'permission_level' attribute: " +
                                subChild.OuterXml);
                            continue;
                        }

                        AddAdmin(username, password, permissionLevel, false);
                    }
                }

                if (childNode.Name == "permissions")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning("[PrismaCore] Unexpected XML node found in 'permissions' section: " + subChild.OuterXml);
                            continue;
                        }

                        XmlElement lineItem = (XmlElement)subChild;

                        if (!lineItem.HasAttribute("module"))
                        {
                            Log.Warning("[PrismaCore] Ignoring permission-entry because of missing 'module' attribute: " +
                                         subChild.OuterXml);
                            continue;
                        }

                        if (!lineItem.HasAttribute("permission_level"))
                        {
                            Log.Warning("[PrismaCore] Ignoring permission-entry because of missing 'permission_level' attribute: " +
                                         subChild.OuterXml);
                            continue;
                        }

                        int permissionLevel;
                        if (!int.TryParse(lineItem.GetAttribute("permission_level"), out permissionLevel))
                        {
                            Log.Warning(
                                "[PrismaCore] Ignoring permission-entry because of invalid (non-numeric) value for 'permission_level' attribute: " +
                                subChild.OuterXml);
                            continue;
                        }

                        AddModulePermission(lineItem.GetAttribute("module"), permissionLevel, false);
                    }
                }
            }

            Log.Out("[PrismaCore] Loading ClaimCreator_permissions file done.");
        }

        public void Save()
        {
            fileWatcher.EnableRaisingEvents = false;

            using (StreamWriter sw = new StreamWriter(GetFullPath()))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<ClaimCreator_permissions>");
                sw.WriteLine("	<apiusers>");
                sw.WriteLine("		 <!-- <apiuser username=\"apiuser1\" password=\"apipassword\" permission_level=\"0\" /> -->");
                sw.WriteLine("	</apiusers>");
                sw.WriteLine("	<permissions>");
                sw.WriteLine("		<permission module=\"ClaimCreator.map\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.createadvclaims\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getlandclaims\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getadvclaims\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getresetregions\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getplayerhomes\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getplayersonline\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getquestpois\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getallpois\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.gettraders\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getvehicles\" permission_level=\"0\" />");
                sw.WriteLine("		<permission module=\"ClaimCreator.getdrones\" permission_level=\"0\" />");
                sw.WriteLine("	</permissions>");
                sw.WriteLine("</ClaimCreator_permissions>");


                sw.Flush();
                sw.Close();
            }

            fileWatcher.EnableRaisingEvents = true;
        }


        public class AdminToken
        {
            public string name;
            public int permissionLevel;
            public string token;

            public AdminToken(string _name, string _token, int _permissionLevel)
            {
                name = _name;
                token = _token;
                permissionLevel = _permissionLevel;
            }
        }

        public struct WebModulePermission
        {
            public string module;
            public int permissionLevel;

            public WebModulePermission(string _module, int _permissionLevel)
            {
                module = _module;
                permissionLevel = _permissionLevel;
            }
        }
    }
}