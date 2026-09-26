using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static vp_Message;

namespace ServerCore
{
    class ClaimProtector
    {
        public static volatile bool IsRunning = false;
        public static bool IsEnabled = true;
        public static int Interval = 3000;
        public static int TpExtraDistance = 5;
        public static List<string> adminBubble = new List<string>();
        private static int counter = 0;
        private static bool didAnnounce = false;
        public static Thread threadClaimprotector;

        public static void Start()
        {
            if (IsEnabled)
            {
                if (!IsRunning)
                {
                    IsRunning = true;
                    ThreadStart tsCP = new ThreadStart(ProtectClaims);
                    threadClaimprotector = new Thread(tsCP);
                    threadClaimprotector.IsBackground = true;
                    threadClaimprotector.Start();
                    Log.Out("[PrismaCore] Claimprotector has started.");
                }
            }
        }

        public static void Unload()
        {
            try
            {
                if (IsRunning)
                {
                    threadClaimprotector.Abort();
                    IsRunning = false;
                }
            }
            catch { }
        }

        private static void ProtectClaims()
        {
            Dictionary<string, List<string>> notified = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> onNotifyClaimNow = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> notOnNotifyClaimNow = new Dictionary<string, List<string>>();

            Dictionary<string, List<string>> cmdExecuted = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> onCmdClaimNow = new Dictionary<string, List<string>>();

            List<Entity> enemies = new List<Entity>();

            List<string> GodModeCommandFired = new List<string>();

            List<string> SpectatorModeCommandFired = new List<string>();

            List<string> toRemove = new List<string>();

            ClientInfoCollection clients;

            List<DbClaim> lstClaims;

            ClientInfo ci;

            ClientInfo clientInfo;

            DbPlayer dbplayer;

            while (IsRunning)
            {
                try
                {
                    if (!IsEnabled)
                    {
                        IsRunning = false;
                        break;
                    }

                    try
                    {
                        //remove from notified and cmdExecuted lists if player is not online anymore
                        clients = ConnectionManager.Instance.Clients;

                        if (clients != null)
                        {
                            toRemove.Clear();
                            foreach (KeyValuePair<string, List<string>> notifiedPlayer in notified)
                            {
                                if (clients.GetForNameOrId(notifiedPlayer.Key) == null)
                                {
                                    toRemove.Add(notifiedPlayer.Key);
                                }
                            }

                            //remove from notified
                            foreach (string s in toRemove)
                            {
                                notified.Remove(s);
                            }

                            toRemove.Clear();
                            foreach (KeyValuePair<string, List<string>> executedPlayer in cmdExecuted)
                            {
                                if (clients.GetForNameOrId(executedPlayer.Key) == null)
                                {
                                    toRemove.Add(executedPlayer.Key);
                                }
                            }

                            //remove from cmdExecuted
                            foreach (string s in toRemove)
                            {
                                cmdExecuted.Remove(s);
                            }
                        }
                    }
                    catch { }

                    if (ConnectionManager.Instance.ClientCount() > 0)
                    {
                        lstClaims = Database.Instance.GetAllDbClaims();

                        if (lstClaims != null && lstClaims.Count != 0)
                        {
                            string name = "";

                            foreach (DbClaim activeClaim in lstClaims)
                            {
                                try
                                {
                                    //hostilefree claims
                                    if (activeClaim == null) continue;

                                    name = activeClaim.Id;

                                    if (activeClaim.Id.Trim().ToLower().Contains("bmonly("))
                                    {
                                        //check if bloodmoon (within treshold) and skip if not
                                        string bmp = activeClaim.Id.Trim().ToLower().Split('_')[0];
                                        if (!IsBloodMoonTime(bmp)) continue;

                                    }

                                    if (string.IsNullOrEmpty(activeClaim.Type))
                                        continue;

                                    if (activeClaim.Type.ToLower().Equals("hostilefree"))
                                    {
                                        enemies.Clear();
                                        
                                        List<Entity> list = new List<Entity>(GameManager.Instance.World.Entities.list);
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            Entity entity = list[i];
                                            if (entity != null && !(entity is EntityPlayer) && entity is EntityAlive && !(entity is EntityVehicle) && !(entity is EntityTurret) && EntityClass.list[entity.entityClass].bIsEnemyEntity)
                                            {
                                                Vector3i hostilePos = new Vector3i(entity.GetPosition());
                                                if (hostilePos.x > activeClaim.W_bound && hostilePos.x < activeClaim.E_bound && hostilePos.z > activeClaim.S_bound && hostilePos.z < activeClaim.N_bound)
                                                {
                                                    enemies.Add(entity);
                                                }
                                            }
                                        }

                                        if (enemies.Count > 0)
                                        {
                                            foreach (EntityAlive ea in enemies.Cast<EntityAlive>())
                                            {
                                                if (ea.GetType().ToString() == "EntityTrader")
                                                {
                                                    continue;
                                                }
                                                try
                                                {
                                                    GameManager.Instance.World.RemoveEntity(ea.entityId, EnumRemoveEntityReason.Killed);
                                                }
                                                catch { continue; }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Out($"[PrismaCore] Error in hostilefree claim: {name}. Error: {ex.ToString()}");
                                    continue;
                                }
                            }
                        }

                        //check if bubble dic count = 0 OR no adv. claim present. No playerloop needed then.
                        if (adminBubble.Count != 0 || (lstClaims != null && lstClaims.Count != 0))
                        {
                            foreach (KeyValuePair<int, EntityPlayer> player in GameManager.Instance.World.Players.dict)
                            {
                                ci = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
                                if (ci == null) continue;

                                try
                                {
                                    if (!notified.ContainsKey(ci.PlatformId.ToString()))
                                    {
                                        int count = player.Value.Buffs.ActiveBuffs.Count;
                                        for (int i = 0; i < count; i++)
                                        {
                                            BuffValue buffValue = player.Value.Buffs.ActiveBuffs[i];
                                            if (buffValue.BuffName.StartsWith("prismacore_tooltip"))
                                            {
                                                player.Value.Buffs.RemoveBuff(buffValue.BuffName);
                                            }
                                        }
                                    }
                                }
                                catch { }

                                Vector3i pos = new Vector3i();
                                if (player.Value == null) continue;

                                pos = player.Value.GetBlockPosition();
                                if (pos == null) continue;

                                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(ci);

                                //protective admin bubble
                                if (adminBubble.Contains(ci.PlatformId.ToString())) ProtectAdmin(ci, pos);

                                if (lstClaims != null && lstClaims.Count != 0)
                                {
                                    onNotifyClaimNow.Clear();
                                    notOnNotifyClaimNow.Clear();
                                    onCmdClaimNow.Clear();

                                    foreach (DbClaim activeClaim in lstClaims)
                                    {
                                        if (activeClaim == null) continue;

                                        if (!string.IsNullOrEmpty(activeClaim.Type))
                                        {
                                            if (activeClaim.Type.Trim().EqualsCaseInsensitive("hostilefree")) continue;
                                            if (activeClaim.Type.Trim().EqualsCaseInsensitive("lcbfree")) continue;
                                            if (activeClaim.Type.Trim().ContainsCaseInsensitive("antiblock:")) continue;
                                            if (activeClaim.Type.Trim().EqualsCaseInsensitive("reset")) continue;
                                            if (activeClaim.Type.Trim().ContainsCaseInsensitive("problock:")) continue;
                                            if (activeClaim.Type.Trim().EqualsCaseInsensitive("landclaim")) continue;
                                        }

                                        if (activeClaim.Id.ToLower().Equals("jail"))
                                        {
                                            try
                                            {
                                                //check if player can be released from jail
                                                string steamID = "";
                                                string playerName = "";

                                                steamID = ci.PlatformId.ToString();
                                                playerName = ci.playerName;

                                                if (!activeClaim.Whitelist.Contains(steamID))
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    dbplayer = Database.Instance.GetDbPlayer(ci.PlatformId.ToString());

                                                    if (dbplayer.AutoRelease)
                                                    {
                                                        if (dbplayer.ReleaseTime < DateTime.Now)
                                                        {
                                                            string currentWhitelist = activeClaim.Whitelist;
                                                            currentWhitelist = currentWhitelist.Replace(playerName + "(" + steamID + ")", string.Empty);
                                                            Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);

                                                            Database.Instance.SetAutoRelease(steamID, false);
                                                            string notiMsg = ServerCoreStrings.Instance.Arrest_AutoReleaseMsg;
                                                                                                                                                                                    
                                                            ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                            Log.Out(playerName + " has been automatically released from jail.");
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                Log.Out($"[PrismaCore] Error in Jail claim. Not handled.");
                                                continue;
                                            }
                                        }

                                        if (activeClaim.Id.Trim().ToLower().Contains("bmonly("))
                                        {
                                            //check if bloodmoon (within treshold) and skip if not
                                            string bmp = activeClaim.Id.Trim().ToLower().Split('_')[0];
                                            if (!IsBloodMoonTime(bmp)) continue;
                                        }

                                        //check if time to live has passed on a timed claim
                                        if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("timed:"))
                                        {
                                            try
                                            {
                                                string sDateTime = activeClaim.Type.Split(new string[] { "timed:" }, StringSplitOptions.None)[1].Trim();
                                                DateTime claimEndsDateTime = DateTime.ParseExact(sDateTime, "yyyy-MM-dd HH:mm:ss", null);
                                                if (DateTime.Now.CompareTo(claimEndsDateTime) > 0)
                                                {
                                                    //claim has expired -> delete it
                                                    Database.Instance.DeleteDbClaim(activeClaim.Id);
                                                    continue;
                                                }
                                            }
                                            catch { continue; }
                                        }

                                        if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Equals("reversed"))
                                        {
                                            try
                                            {
                                                if (activeClaim.Whitelist.Contains(ci.PlatformId.ToString()))
                                                {
                                                    if (pos.x < activeClaim.W_bound || pos.x > activeClaim.E_bound || pos.z < activeClaim.S_bound || pos.z > activeClaim.N_bound)
                                                    {
                                                        if (player.Value.IsSpawned()) //stange behaviour when player is not fully spawned
                                                        {
                                                            ReversedHandler(ci, activeClaim);
                                                            continue;
                                                        }
                                                    }
                                                }
                                                else continue;
                                            }
                                            catch
                                            {
                                                Log.Out($"[PrismaCore] Error in reversed claim: {activeClaim.Id}. Not handled.");
                                                continue;
                                            }
                                        }

                                        if (pos.x > activeClaim.W_bound && pos.x < activeClaim.E_bound && pos.z > activeClaim.S_bound && pos.z < activeClaim.N_bound)
                                        {
                                            if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("portal:"))
                                            {
                                                try
                                                {
                                                    if (AdminLvL <= activeClaim.AccessLevel)
                                                    {
                                                        //allowed admin permission level can always use portal
                                                        if (player.Value.IsSpawned())
                                                        {
                                                            PortalHandler(ci, pos, activeClaim);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(activeClaim.Whitelist) && (activeClaim.Whitelist.ToLower().Contains("public(public)") || activeClaim.Whitelist.Contains(ci.PlatformId.ToString())))
                                                        {
                                                            if (player.Value.IsSpawned())
                                                            {
                                                                PortalHandler(ci, pos, activeClaim);
                                                            }
                                                        }
                                                    }
                                                    continue;
                                                }
                                                catch
                                                {
                                                    Log.Out($"[PrismaCore] Error in portal claim: {activeClaim.Id}. Not handled.");
                                                    continue;
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("command:"))
                                            {
                                                try
                                                {
                                                    if (AdminLvL > activeClaim.AccessLevel)
                                                    {
                                                        if (player.Value.IsSpawned())
                                                        {
                                                            string command = string.Empty;
                                                            string[] arrCommand = activeClaim.Type.Split(':');
                                                            if (arrCommand.Length == 2)
                                                            {
                                                                command = arrCommand[1];
                                                            }
                                                            else
                                                            {
                                                                command = string.Join(":", arrCommand, 1, arrCommand.Length - 1).Trim();
                                                            }

                                                            //record that this player is in a command claim right now
                                                            if (onCmdClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                                            {
                                                                //player is detected on a claim allready -? check if new and add
                                                                if (!onCmdClaimNow[ci.PlatformId.ToString()].Contains(activeClaim.Id))
                                                                {
                                                                    onCmdClaimNow[ci.PlatformId.ToString()].Add(activeClaim.Id);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //first record of presence on a claim
                                                                List<string> newList = new List<string>
                                                                {
                                                                    activeClaim.Id
                                                                };
                                                                onCmdClaimNow.Add(ci.PlatformId.ToString(), newList);
                                                            }

                                                            if (cmdExecuted.ContainsKey(ci.PlatformId.ToString()))
                                                            {
                                                                //has been executed b4. Check if its a new claim enter
                                                                if (!cmdExecuted[ci.PlatformId.ToString()].Contains(activeClaim.Id))
                                                                {
                                                                    //entered a new commandclaim. 
                                                                    if (command.Contains(";"))
                                                                    {
                                                                        //multiple commands
                                                                        string[] arrCommands = command.Split(';');
                                                                        CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                                        foreach (string s in arrCommands)
                                                                        {
                                                                            string cmd = s;
                                                                            cmd = cmd.Replace("'", "\"");
                                                                            cmd = cmd.Replace("${steamId}", ci.PlatformId.ToString());
                                                                            cmd = cmd.Replace("${platformId}", ci.PlatformId.ToString());
                                                                            cmd = cmd.Replace("${entityId}", ci.entityId.ToString());
                                                                            cmd = cmd.Replace("${playerName}", ci.playerName);

                                                                            SdtdConsole.Instance.ExecuteAsync(cmd, iConsole);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        //just 1 command
                                                                        command = command.Replace("'", "\"");
                                                                        command = command.Replace("${steamId}", ci.PlatformId.ToString());
                                                                        command = command.Replace("${platformId}", ci.PlatformId.ToString());
                                                                        command = command.Replace("${entityId}", ci.entityId.ToString());
                                                                        command = command.Replace("${playerName}", ci.playerName);

                                                                        CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                                        SdtdConsole.Instance.ExecuteAsync(command, iConsole);
                                                                    }

                                                                    cmdExecuted[ci.PlatformId.ToString()].Add(activeClaim.Id);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //never had a cmd execution. Run cmd for this claim
                                                                if (command.Contains(";"))
                                                                {
                                                                    //multiple commands
                                                                    string[] arrCommands = command.Split(';');
                                                                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                                    foreach (string s in arrCommands)
                                                                    {
                                                                        string cmd = s;
                                                                        cmd = cmd.Replace("'", "\"");
                                                                        cmd = cmd.Replace("${steamId}", ci.PlatformId.ToString());
                                                                        cmd = cmd.Replace("${platformId}", ci.PlatformId.ToString());
                                                                        cmd = cmd.Replace("${entityId}", ci.entityId.ToString());
                                                                        cmd = cmd.Replace("${playerName}", ci.playerName);

                                                                        SdtdConsole.Instance.ExecuteAsync(cmd, iConsole);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //just 1 command
                                                                    command = command.Replace("'", "\"");
                                                                    command = command.Replace("${steamId}", ci.PlatformId.ToString());
                                                                    command = command.Replace("${platformId}", ci.PlatformId.ToString());
                                                                    command = command.Replace("${entityId}", ci.entityId.ToString());
                                                                    command = command.Replace("${playerName}", ci.playerName);

                                                                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                                    SdtdConsole.Instance.ExecuteAsync(command, iConsole);
                                                                }

                                                                List<string> newList = new List<string>
                                                                {
                                                                    activeClaim.Id
                                                                };
                                                                cmdExecuted.Add(ci.PlatformId.ToString(), newList);
                                                            }
                                                        }
                                                    }
                                                    continue;
                                                }
                                                catch
                                                {
                                                    Log.Out($"[PrismaCore] Error in command claim: {activeClaim.Id}. Not handled.");
                                                    continue;
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("playerlevel:"))
                                            {
                                                try
                                                {
                                                    if (AdminLvL > activeClaim.AccessLevel)
                                                    {
                                                        if (player.Value.IsSpawned())
                                                        {
                                                            string param = activeClaim.Type.Split(':')[1];
                                                            if (param.Contains("&"))
                                                            {
                                                                //multiple expressions
                                                                string expr1 = param.Split('&')[0];
                                                                string expr2 = param.Split('&')[1];
                                                                string operand1 = string.Empty;
                                                                string op1 = string.Empty;
                                                                string operand2 = string.Empty;
                                                                string op2 = string.Empty;

                                                                if (expr1.Contains("<=") || expr1.Contains("=<"))
                                                                {
                                                                    if (expr1.Contains("<="))
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "<=" }, StringSplitOptions.None)[1];
                                                                        op1 = "<=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "=<" }, StringSplitOptions.None)[1];
                                                                        op1 = "=<";
                                                                    }
                                                                }
                                                                else if (expr1.Contains(">=") || expr1.Contains("=>"))
                                                                {
                                                                    if (expr1.Contains(">="))
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { ">=" }, StringSplitOptions.None)[1];
                                                                        op1 = ">=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "=>" }, StringSplitOptions.None)[1];
                                                                        op1 = "=>";
                                                                    }
                                                                }
                                                                else if (expr1.Contains("!=") || expr1.Contains("=!"))
                                                                {
                                                                    if (expr1.Contains("!="))
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "!=" }, StringSplitOptions.None)[1];
                                                                        op1 = "!=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "=!" }, StringSplitOptions.None)[1];
                                                                        op1 = "!=";
                                                                    }
                                                                }
                                                                else if (expr1.Contains("==") || expr1.Contains("="))
                                                                {
                                                                    if (expr1.Contains("=="))
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "==" }, StringSplitOptions.None)[1];
                                                                        op1 = "==";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand1 = expr1.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                                                                        op1 = "==";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //invalid operator -> abort
                                                                    Log.Out("There is an invalid operator in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                if (expr2.Contains("<=") || expr2.Contains("=<"))
                                                                {
                                                                    if (expr2.Contains("<="))
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "<=" }, StringSplitOptions.None)[1];
                                                                        op2 = "<=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "=<" }, StringSplitOptions.None)[1];
                                                                        op2 = "=<";
                                                                    }
                                                                }
                                                                else if (expr2.Contains(">=") || expr2.Contains("=>"))
                                                                {
                                                                    if (expr2.Contains(">="))
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { ">=" }, StringSplitOptions.None)[1];
                                                                        op2 = ">=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "=>" }, StringSplitOptions.None)[1];
                                                                        op2 = "=>";
                                                                    }
                                                                }
                                                                else if (expr2.Contains("!=") || expr2.Contains("=!"))
                                                                {
                                                                    if (expr2.Contains("!="))
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "!=" }, StringSplitOptions.None)[1];
                                                                        op2 = "!=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "=!" }, StringSplitOptions.None)[1];
                                                                        op2 = "!=";
                                                                    }
                                                                }
                                                                else if (expr2.Contains("==") || expr2.Contains("="))
                                                                {
                                                                    if (expr2.Contains("=="))
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "==" }, StringSplitOptions.None)[1];
                                                                        op2 = "==";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand2 = expr2.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                                                                        op2 = "==";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //invalid operator -> abort
                                                                    Log.Out("There is an invalid operator in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                if (!int.TryParse(operand1, out int restrictLevel1))
                                                                {
                                                                    //not a valid int
                                                                    Log.Out("There is an invalid integer in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                if (!int.TryParse(operand2, out int restrictLevel2))
                                                                {
                                                                    //not a valid int
                                                                    Log.Out("There is an invalid integer in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                int level = player.Value.Progression.GetLevel();

                                                                if (!op1.UseAsOperator(level, restrictLevel1) || !op2.UseAsOperator(level, restrictLevel2))
                                                                {
                                                                    //not allowed -> use intrusionhandler
                                                                    LevelIntrusionHandler(ci, pos, activeClaim, param);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //single expression
                                                                string operand = string.Empty;
                                                                string op = string.Empty;

                                                                if (param.Contains("<=") || param.Contains("=<"))
                                                                {
                                                                    if (param.Contains("<="))
                                                                    {
                                                                        operand = param.Split(new string[] { "<=" }, StringSplitOptions.None)[1];
                                                                        op = "<=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand = param.Split(new string[] { "=<" }, StringSplitOptions.None)[1];
                                                                        op = "=<";
                                                                    }
                                                                }
                                                                else if (param.Contains(">=") || param.Contains("=>"))
                                                                {
                                                                    if (param.Contains(">="))
                                                                    {
                                                                        operand = param.Split(new string[] { ">=" }, StringSplitOptions.None)[1];
                                                                        op = ">=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand = param.Split(new string[] { "=>" }, StringSplitOptions.None)[1];
                                                                        op = "=>";
                                                                    }
                                                                }
                                                                else if (param.Contains("!=") || param.Contains("=!"))
                                                                {
                                                                    if (param.Contains("!="))
                                                                    {
                                                                        operand = param.Split(new string[] { "!=" }, StringSplitOptions.None)[1];
                                                                        op = "!=";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand = param.Split(new string[] { "=!" }, StringSplitOptions.None)[1];
                                                                        op = "!=";
                                                                    }
                                                                }
                                                                else if (param.Contains("==") || param.Contains("="))
                                                                {
                                                                    if (param.Contains("=="))
                                                                    {
                                                                        operand = param.Split(new string[] { "==" }, StringSplitOptions.None)[1];
                                                                        op = "==";
                                                                    }
                                                                    else
                                                                    {
                                                                        operand = param.Split(new string[] { "=" }, StringSplitOptions.None)[1];
                                                                        op = "==";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //invalid operator -> abort
                                                                    Log.Out("There is an invalid operator in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                if (!int.TryParse(operand, out int restrictLevel))
                                                                {
                                                                    //not a valid int
                                                                    Log.Out("There is an invalid integer in your playerlevel expression: " + param);
                                                                    continue;
                                                                }

                                                                int level = player.Value.Progression.GetLevel();
                                                                if (!op.UseAsOperator(level, restrictLevel))
                                                                {
                                                                    //not allowed -> use intrusionhandler
                                                                    LevelIntrusionHandler(ci, pos, activeClaim, param);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    continue;
                                                }
                                                catch
                                                {
                                                    Log.Out($"[PrismaCore] Error in playerlevel claim: {activeClaim.Id}. Not handled.");
                                                    continue;
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("notify:"))
                                            {
                                                try
                                                {
                                                    if (player.Value.IsSpawned())
                                                    {
                                                        //record that this player is in a notified claim right now
                                                        if (onNotifyClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                                        {
                                                            //player is detected on a claim allready -? check if new and add
                                                            if (!onNotifyClaimNow[ci.PlatformId.ToString()].Contains(activeClaim.Id))
                                                            {
                                                                onNotifyClaimNow[ci.PlatformId.ToString()].Add(activeClaim.Id);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //first record of presence on a claim
                                                            List<string> newList = new List<string>
                                                        {
                                                            activeClaim.Id
                                                        };
                                                            onNotifyClaimNow.Add(ci.PlatformId.ToString(), newList);
                                                        }

                                                        if (notified.ContainsKey(ci.PlatformId.ToString()))
                                                        {
                                                            //has been notified b4. Check if its a new claim enter
                                                            if (!notified[ci.PlatformId.ToString()].Contains(activeClaim.Id))
                                                            {
                                                                //entered a new notifyclaim (notify, pve or resetregion). 
                                                                string notiMsg = activeClaim.Type.Split(':')[1];

                                                                if (activeClaim.Id.StartsWith("r.") && activeClaim.Id.EndsWith(".7rg"))
                                                                {
                                                                    //resetregion
                                                                    if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterresetregion"))
                                                                    {
                                                                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                        if (!ep.Buffs.HasBuff("prismacore_tooltip_enterresetregion"))
                                                                            showTooltip(ep, "prismacore_tooltip_enterresetregion");
                                                                    }
                                                                    else
                                                                    {
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                                                                    }
                                                                }
                                                                else if (activeClaim.Id.ContainsCaseInsensitive("pve"))
                                                                {
                                                                    //pve claim
                                                                    if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterpveclaim"))
                                                                    {
                                                                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                        if (!ep.Buffs.HasBuff("prismacore_tooltip_enterpveclaim"))
                                                                            showTooltip(ep, "prismacore_tooltip_enterpveclaim");
                                                                    }
                                                                    else
                                                                    {
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                    }

                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 0", true));
                                                                }
                                                                else if (activeClaim.Id.ContainsCaseInsensitive("pvp"))
                                                                {
                                                                    //pvp claim
                                                                    if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterpvpclaim"))
                                                                    {
                                                                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                        if (!ep.Buffs.HasBuff("prismacore_tooltip_enterpvpclaim"))
                                                                            showTooltip(ep, "prismacore_tooltip_enterpvpclaim");
                                                                    }
                                                                    else
                                                                    {
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                    }

                                                                    int killMode = ServerCoreSettings.Instance.AdvClaims_PVP_KillingMode;
                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup($"sgs PlayerKillingMode {killMode}", true));
                                                                }
                                                                else
                                                                {
                                                                    //normal notifyclaim
                                                                    if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{activeClaim.Id}_enter"))
                                                                    {
                                                                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                        if (!ep.Buffs.HasBuff($"prismacore_tooltip_{activeClaim.Id}_enter"))
                                                                            showTooltip(ep, $"prismacore_tooltip_{activeClaim.Id}_enter");
                                                                    }
                                                                    else
                                                                    {
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                    }
                                                                }

                                                                notified[ci.PlatformId.ToString()].Add(activeClaim.Id);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //never had a notificaton. Notify for this claim (notify, pve or resetregion)
                                                            string notiMsg = activeClaim.Type.Split(':')[1];

                                                            if (activeClaim.Id.StartsWith("r.") && activeClaim.Id.EndsWith(".7rg"))
                                                            {
                                                                //resetregion
                                                                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterresetregion"))
                                                                {
                                                                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                    showTooltip(ep, "prismacore_tooltip_enterresetregion");
                                                                }
                                                                else
                                                                {
                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                }
                                                            }
                                                            else if (activeClaim.Id.ContainsCaseInsensitive("pve"))
                                                            {
                                                                //pve claim
                                                                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterpveclaim"))
                                                                {
                                                                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                    showTooltip(ep, "prismacore_tooltip_enterpveclaim");
                                                                }
                                                                else
                                                                {
                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                }

                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("sgs PlayerKillingMode 0", true));
                                                            }
                                                            else if (activeClaim.Id.ContainsCaseInsensitive("pvp"))
                                                            {
                                                                //pvp claim
                                                                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_enterpvpclaim"))
                                                                {
                                                                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                    showTooltip(ep, "prismacore_tooltip_enterpvpclaim");
                                                                }
                                                                else
                                                                {
                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                }

                                                                int killMode = ServerCoreSettings.Instance.AdvClaims_PVP_KillingMode;
                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup($"sgs PlayerKillingMode {killMode}", true));
                                                            }
                                                            else
                                                            {
                                                                //normal notifyclaim
                                                                if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{activeClaim.Id}_enter"))
                                                                {
                                                                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                    showTooltip(ep, $"prismacore_tooltip_{activeClaim.Id}_enter");
                                                                }
                                                                else
                                                                {
                                                                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, notiMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                }
                                                            }

                                                            List<string> newList = new List<string>
                                                            {
                                                            activeClaim.Id
                                                            };
                                                            notified.Add(ci.PlatformId.ToString(), newList);
                                                        }
                                                    }
                                                    continue;
                                                }
                                                catch
                                                {
                                                    Log.Out($"[PrismaCore] Error in notify claim: {activeClaim.Id}. Not handled.");
                                                    continue;
                                                }
                                            }

                                            if (!activeClaim.Id.Contains(ci.PlatformId.ToString()))
                                            {
                                                try
                                                {
                                                    if (AdminLvL > activeClaim.AccessLevel)
                                                    {
                                                        if (!activeClaim.Whitelist.Contains(ci.PlatformId.ToString()))
                                                        {
                                                            if (player.Value.IsSpawned())
                                                            {
                                                                bool _isOnVehicle = player.Value.AttachedToEntity is EntityVehicle;

                                                                if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("leveled:"))
                                                                {
                                                                    string[] hiLow = activeClaim.Type.Split(':')[1].Split(',');
                                                                    int hi = int.Parse(hiLow[0]);
                                                                    int low = int.Parse(hiLow[1]);
                                                                    if (pos.y < hi && pos.y > low)
                                                                    {
                                                                        if (_isOnVehicle)
                                                                        {
                                                                            if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_onvehiclewarning"))
                                                                            {
                                                                                EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                                showTooltip(ep, "prismacore_tooltip_onvehiclewarning");
                                                                            }
                                                                            else
                                                                            {
                                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OnVehicleWarning), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                            }

                                                                            continue;
                                                                        }

                                                                        IntrusionHandler(ci, pos, activeClaim, "");
                                                                    }
                                                                }
                                                                else if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains("openhours:"))
                                                                {
                                                                    string[] arrOpen = activeClaim.Type.Split(':')[1].Split('-');
                                                                    int openFrom = int.Parse(arrOpen[0]);
                                                                    int openTo = int.Parse(arrOpen[1]);

                                                                    int currentHour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);

                                                                    bool open = false;

                                                                    for (int i = openFrom; i < 48; i++)
                                                                    {
                                                                        if (i == openTo) break;

                                                                        if (i == currentHour)
                                                                        {
                                                                            open = true;
                                                                            break;
                                                                        }

                                                                        if (i == 23)
                                                                        {
                                                                            for (int j = 0; j < openTo; j++)
                                                                            {
                                                                                if (j == currentHour)
                                                                                {
                                                                                    open = true;
                                                                                    break;
                                                                                }
                                                                            }
                                                                            //all checked 
                                                                            break;
                                                                        }
                                                                    }

                                                                    if (!open)
                                                                    {
                                                                        if (_isOnVehicle)
                                                                        {
                                                                            if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_onvehiclewarning"))
                                                                            {
                                                                                EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                                showTooltip(ep, "prismacore_tooltip_onvehiclewarning");
                                                                            }
                                                                            else
                                                                            {
                                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OnVehicleWarning), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                            }

                                                                            continue;
                                                                        }

                                                                        IntrusionHandler(ci, pos, activeClaim, activeClaim.Type.Split(':')[1]);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (_isOnVehicle)
                                                                    {
                                                                        if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_onvehiclewarning"))
                                                                        {
                                                                            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                            showTooltip(ep, "prismacore_tooltip_onvehiclewarning");
                                                                        }
                                                                        else
                                                                        {
                                                                            ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OnVehicleWarning), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                        }

                                                                        continue;
                                                                    }

                                                                    IntrusionHandler(ci, pos, activeClaim, "");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    Log.Out($"[PrismaCore] Error in adv. claim: {activeClaim.Id}. Not handled.");
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                //not in active claim -> check if type notify and add to notOnNotifyClaimNow
                                                if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.Contains("notify:"))
                                                {
                                                    if (notOnNotifyClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                                    {
                                                        if (!notOnNotifyClaimNow[ci.PlatformId.ToString()].Contains(activeClaim.Id))
                                                        {
                                                            notOnNotifyClaimNow[ci.PlatformId.ToString()].Add(activeClaim.Id);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //first record of presence on a claim
                                                        List<string> newList = new List<string>
                                                        {
                                                            activeClaim.Id
                                                        };
                                                        notOnNotifyClaimNow.Add(ci.PlatformId.ToString(), newList);
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    //all claims checked
                                    //first check if player is standing on a reset region claim now
                                    if (player.Value.IsSpawned())
                                    {
                                        try
                                        {
                                            bool onResetRegion = false;

                                            //check notifyclaims
                                            if (onNotifyClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                            {
                                                foreach (string s in onNotifyClaimNow[ci.PlatformId.ToString()])
                                                {
                                                    if (s.StartsWith("r.") && s.EndsWith(".7rg"))
                                                    {
                                                        onResetRegion = true;
                                                        break;
                                                    }
                                                }
                                            }

                                            //check if player is standing on any commandClaim
                                            if (onCmdClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                            {
                                                if (cmdExecuted.ContainsKey(ci.PlatformId.ToString()))
                                                {
                                                    foreach (string cmdNoti in cmdExecuted[ci.PlatformId.ToString()])
                                                    {
                                                        //loop through cmd claims the player is on now -> if not on -> remove entry from cmdExecuted
                                                        bool found = false;
                                                        foreach (string onCmd in onCmdClaimNow[ci.PlatformId.ToString()])
                                                        {
                                                            if (cmdNoti.EqualsCaseInsensitive(onCmd))
                                                            {
                                                                found = true;
                                                            }
                                                        }

                                                        if (!found)
                                                        {
                                                            //cmd executed but not on claim anymore -> remove from cmdExecuted
                                                            cmdExecuted[ci.PlatformId.ToString()].Remove(cmdNoti);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //not on any commandclaim now -> remove from cmdExecuted
                                                if (cmdExecuted.ContainsKey(ci.PlatformId.ToString()))
                                                    cmdExecuted.Remove(ci.PlatformId.ToString());
                                            }

                                            //loop through notifyclaim the player is NOT on right now and check for exit msg and player not on reset claim
                                            if (notOnNotifyClaimNow.ContainsKey(ci.PlatformId.ToString()))
                                            {
                                                if (notified.ContainsKey(ci.PlatformId.ToString()))
                                                {
                                                    foreach (string c in notOnNotifyClaimNow[ci.PlatformId.ToString()])
                                                    {
                                                        if (notified[ci.PlatformId.ToString()].Contains(c))
                                                        {
                                                            //player not on a claim that has given notification b4 -> check if reset claim(and if standing on a reset claim) or normal
                                                            if (c.StartsWith("r.") && c.EndsWith(".7rg"))
                                                            {
                                                                if (!onResetRegion)
                                                                {
                                                                    //player not on reset region. Left one. Give exit message
                                                                    //get claim for exit msg
                                                                    string notification = Database.Instance.GetDbClaim(c).Type;
                                                                    string[] arr = notification.Split(':');
                                                                    if (arr.Length == 3 && arr[0].EqualsCaseInsensitive("notify"))
                                                                    {
                                                                        string exitMsg = arr[2];

                                                                        if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_exitresetregion"))
                                                                        {
                                                                            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                            showTooltip(ep, "prismacore_tooltip_exitresetregion");
                                                                            removeTooltip(ep, "prismacore_tooltip_enterresetregion");
                                                                        }
                                                                        else
                                                                        {
                                                                            ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, exitMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //not a reset claim. Check if there is a exit message in claim config
                                                                //get claim for exit msg
                                                                var dbClaim = Database.Instance.GetDbClaim(c);
                                                                var notification = dbClaim.Type;
                                                                var id = dbClaim.Id;

                                                                var arr = notification.Split(':');
                                                                if (arr[0].EqualsCaseInsensitive("notify"))
                                                                {
                                                                    //check if normal or pve or pvp
                                                                    if (id.ContainsCaseInsensitive("pve"))
                                                                    {
                                                                        //pve claim
                                                                        string exitMsg = arr[2];

                                                                        if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_exitpveclaim"))
                                                                        {
                                                                            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                            showTooltip(ep, "prismacore_tooltip_exitpveclaim");
                                                                            removeTooltip(ep, "prismacore_tooltip_enterpveclaim");
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!string.IsNullOrEmpty(exitMsg))
                                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, exitMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                        }

                                                                        int serverKillingMode = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("PlayerKillingMode"));
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup($"sgs PlayerKillingMode {serverKillingMode}", true));
                                                                    }
                                                                    else if (id.ContainsCaseInsensitive("pvp"))
                                                                    {
                                                                        //pvp claim
                                                                        string exitMsg = arr[2];

                                                                        if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_exitpvpclaim"))
                                                                        {
                                                                            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                            showTooltip(ep, "prismacore_tooltip_exitpvpclaim");
                                                                            removeTooltip(ep, "prismacore_tooltip_enterpvpclaim");
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!string.IsNullOrEmpty(exitMsg))
                                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, exitMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                        }

                                                                        int serverKillingMode = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("PlayerKillingMode"));
                                                                        ci.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup($"sgs PlayerKillingMode {serverKillingMode}", true));
                                                                    }
                                                                    else
                                                                    {
                                                                        //normal claim
                                                                        string exitMsg = arr[2];

                                                                        if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{id}_exit"))
                                                                        {
                                                                            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                                                                            showTooltip(ep, $"prismacore_tooltip_{id}_exit");
                                                                            removeTooltip(ep, $"prismacore_tooltip_{id}_enter");
                                                                        }
                                                                        else
                                                                        {
                                                                            if (!string.IsNullOrEmpty(exitMsg))
                                                                                ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, exitMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            notified[ci.PlatformId.ToString()].Remove(c);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        if (ServerCoreSettings.Instance.NighttimeAnnouncer_Enabled)
                            Announce();

                        //Check for illegal GodMode or flying
                        foreach (KeyValuePair<int, EntityPlayer> player in GameManager.Instance.World.Players.dict)
                        {
                            try
                            {
                                clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.Value.entityId);
                                if (clientInfo == null) continue;

                                foreach (BuffValue bv in player.Value.Buffs.ActiveBuffs)
                                {
                                    try
                                    {
                                        if (bv.BuffName.ToLower() == "god")
                                        {
                                            int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo);

                                            if (AdminLvL > ServerCoreSettings.Instance.MaxAdminLevelGodMode)
                                            {
                                                Log.Out($"[PrismaCore]Unauthorized GodMode detected on {clientInfo.playerName} ({clientInfo.PlatformId}) !!!!!");
                                                Log.Out($"[PrismaCore]Permissionlevel: {AdminLvL} MaxAdminLevelGodMode: {ServerCoreSettings.Instance.MaxAdminLevelGodMode}");

                                                //fire optional command(s)
                                                if (!GodModeCommandFired.Contains(clientInfo.PlatformId.ToString()))
                                                {
                                                    string command = ServerCoreSettings.Instance.GodModeDetectedCommand;
                                                    if (!string.IsNullOrEmpty(command))
                                                    {
                                                        if (command.Contains(";"))
                                                        {
                                                            //multiple commands
                                                            string[] arrCommands = command.Split(';');
                                                            CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                            foreach (string s in arrCommands)
                                                            {
                                                                string cmd = s;
                                                                cmd = cmd.Replace("${steamId}", clientInfo.PlatformId.ToString());
                                                                cmd = cmd.Replace("${platformId}", clientInfo.PlatformId.ToString());
                                                                cmd = cmd.Replace("${entityId}", clientInfo.entityId.ToString());
                                                                cmd = cmd.Replace("${playerName}", clientInfo.playerName);

                                                                GodModeCommandFired.Add(clientInfo.PlatformId.ToString());

                                                                SdtdConsole.Instance.ExecuteAsync(cmd, iConsole);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //just 1 command
                                                            command = command.Replace("${steamId}", clientInfo.PlatformId.ToString());
                                                            command = command.Replace("${platformId}", clientInfo.PlatformId.ToString());
                                                            command = command.Replace("${entityId}", clientInfo.entityId.ToString());
                                                            command = command.Replace("${playerName}", clientInfo.playerName);

                                                            CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                            GodModeCommandFired.Add(clientInfo.PlatformId.ToString());
                                                            SdtdConsole.Instance.ExecuteAsync(command, iConsole);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch { continue; }
                                }

                                //check if flying
                                if (ServerCoreSettings.Instance.PlayerFlying_TriggerHeight > 0)
                                {
                                    if (player.Value.IsSpawned())
                                    {
                                        bool _isOnGyrocopter = player.Value.AttachedToEntity is EntityVGyroCopter;
                                        bool _isOnHelicopter = player.Value.AttachedToEntity is EntityVHelicopter;
                                        bool _isOnBlimp = player.Value.AttachedToEntity is EntityVBlimp;
                                        bool _isSwimming = player.Value.isSwimming;
                                        bool _isInWater = player.Value.IsInWater();

                                        if (!_isOnGyrocopter && !_isOnHelicopter && !_isOnBlimp && !_isSwimming && !_isInWater)
                                        {
                                            int height = getPlayerHeight(player.Value);
                                            if (height >= ServerCoreSettings.Instance.PlayerFlying_TriggerHeight)
                                            {
                                                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo);

                                                if (AdminLvL > ServerCoreSettings.Instance.PlayerFlying_MaxAdminLevelFlying)
                                                {
                                                    Log.Out($"[PrismaCore]Player {clientInfo.playerName} ({clientInfo.PlatformId}) seems to be flying !!!!!");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { continue; }

                            try
                            {
                                if (player.Value.IsSpectator)
                                {
                                    clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.Value.entityId);
                                    if (clientInfo == null) continue;

                                    int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(clientInfo);

                                    if (AdminLvL > ServerCoreSettings.Instance.MaxAdminLevelSpectatorMode)
                                    {
                                        Log.Out($"[PrismaCore]Unauthorized SpectatorMode detected on {clientInfo.playerName} ({clientInfo.PlatformId}) !!!!!");
                                        Log.Out($"[PrismaCore]Permissionlevel: {AdminLvL} MaxAdminLevelSpectatorMode: {ServerCoreSettings.Instance.MaxAdminLevelSpectatorMode}");

                                        //fire optional command(s)
                                        if (!SpectatorModeCommandFired.Contains(clientInfo.PlatformId.ToString()))
                                        {
                                            string command = ServerCoreSettings.Instance.SpectatorModeDetectedCommand;
                                            if (!string.IsNullOrEmpty(command))
                                            {
                                                if (command.Contains(";"))
                                                {
                                                    //multiple commands
                                                    string[] arrCommands = command.Split(';');
                                                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                    foreach (string s in arrCommands)
                                                    {
                                                        string cmd = s;
                                                        cmd = cmd.Replace("${steamId}", clientInfo.PlatformId.ToString());
                                                        cmd = cmd.Replace("${platformId}", clientInfo.PlatformId.ToString());
                                                        cmd = cmd.Replace("${entityId}", clientInfo.entityId.ToString());
                                                        cmd = cmd.Replace("${playerName}", clientInfo.playerName);

                                                        SpectatorModeCommandFired.Add(clientInfo.PlatformId.ToString());

                                                        SdtdConsole.Instance.ExecuteAsync(cmd, iConsole);
                                                    }
                                                }
                                                else
                                                {
                                                    //just 1 command
                                                    command = command.Replace("${steamId}", clientInfo.PlatformId.ToString());
                                                    command = command.Replace("${platformId}", clientInfo.PlatformId.ToString());
                                                    command = command.Replace("${entityId}", clientInfo.entityId.ToString());
                                                    command = command.Replace("${playerName}", clientInfo.playerName);

                                                    CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                                                    SpectatorModeCommandFired.Add(clientInfo.PlatformId.ToString());
                                                    SdtdConsole.Instance.ExecuteAsync(command, iConsole);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            catch { continue; }
                        }
                    }
                    Thread.Sleep(Interval);
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void Announce()
        {
            int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int hours = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
            int warnhours = ServerCoreSettings.Instance.NighttimeAnnouncer_Warnhours;

            int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));
            int BMrange = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonRange"));

            if (hours == (22 - warnhours) && !didAnnounce)
            {
                string announcer = ServerCoreStrings.Instance.NighttimeAnnouncer_AnnouncerName;
                string nta = ServerCoreStrings.Instance.NighttimeAnnouncer_NightTimeText;
                string bdt = ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayText;
                string bdtt = ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayTomorrowText;
                string ct = ServerCoreStrings.Instance.NighttimeAnnouncer_CounterText;

                nta = nta.Replace("{hours}", warnhours.ToString());
                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, nta), null, EMessageSender.None);
                //Bloodmoon announcement
                if (BMcycle > 0)
                {
                    int cycle = BMcycle;
                    int remainder = int.MinValue;
                    int q = Math.DivRem(days, cycle, out remainder);
                    string PMmsg = "";
                    if (remainder == 0) PMmsg = bdt;
                    else if (remainder == cycle - 1) PMmsg = bdtt;
                    else
                    {
                        int daysleft = cycle - remainder;
                        PMmsg = ct.Replace("{daysleft}", daysleft.ToString());
                    }
                    if (BMrange > 0)
                    {
                        PMmsg = ServerCoreStrings.Instance.NighttimeAnnouncer_RandomBloodmoon;
                        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None);
                    }
                    else
                    {
                        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None);
                    }
                }

                didAnnounce = true;
            }
            else
            {
                if (hours != (22 - warnhours)) didAnnounce = false;
            }
        }

        private static void ReversedHandler(ClientInfo ci, DbClaim claim)
        {
            try
            {
                string denied = ServerCoreStrings.Instance.AdvClaims_Reversed;

                int halfwayX = Math.Abs((claim.E_bound - claim.W_bound) / 2);
                int halfwayY = Math.Abs((claim.N_bound - claim.S_bound) / 2);

                int destX = claim.E_bound - halfwayX;
                int destZ = claim.N_bound - halfwayY;

                Vector3 destPos = new Vector3();

                destPos.x = Convert.ToSingle(destX);
                destPos.y = Convert.ToSingle(ServerCoreSettings.Instance.AdvClaims_Reversed_TpHeight);
                destPos.z = Convert.ToSingle(destZ);

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_reversed"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, "prismacore_tooltip_reversed");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, denied), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch (Exception e) { Log.Error(e.ToString()); }
        }

        private static void PortalHandler(ClientInfo ci, Vector3i pos, DbClaim claim)
        {
            string[] coords = claim.Type.Split(':')[2].Split(',');
            int portalHeight = int.Parse(claim.Type.Split(':')[1]);

            if (Math.Abs(portalHeight - pos.y) <= 2)
            {
                Vector3 destPos = new Vector3();

                destPos.x = float.Parse(coords[0]);
                destPos.y = float.Parse(coords[1]);
                destPos.z = float.Parse(coords[2]);

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(destPos);

                ci.SendPackage(pkg);
            }
        }

        private static void LevelIntrusionHandler(ClientInfo ci, Vector3i pos, DbClaim claim, string logic)
        {
            Vector3 newPos = new Vector3();
            string msg = ServerCoreStrings.Instance.AdvClaims_PlayerLevel.Replace("{logic}", logic);

            int halfwayX = Math.Abs((claim.E_bound - claim.W_bound) / 2);
            int halfwayY = Math.Abs((claim.N_bound - claim.S_bound) / 2);

            int boundaryX = claim.E_bound - halfwayX;
            int boundaryY = claim.N_bound - halfwayY;

            //left/above
            if (pos.x <= boundaryX && pos.z >= boundaryY)
            {
                //tele corner w_bound, n_bound
                newPos.x = claim.W_bound - TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.N_bound + TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{claim.Id}"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, $"prismacore_tooltip_{claim.Id}");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }

            //right/above
            if (pos.x >= boundaryX && pos.z >= boundaryY)
            {
                //tele corner e_bound, n_bound
                newPos.x = claim.E_bound + TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.N_bound + TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{claim.Id}"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, $"prismacore_tooltip_{claim.Id}");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
            //left/below
            if (pos.x <= boundaryX && pos.z <= boundaryY)
            {
                //tele corner w_bound, s_bound
                newPos.x = claim.W_bound - TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.S_bound - TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{claim.Id}"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, $"prismacore_tooltip_{claim.Id}");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
            //right/below
            if (pos.x >= boundaryX && pos.z <= boundaryY)
            {
                //tele corner e_bound, s_bound
                newPos.x = claim.E_bound + TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.S_bound - TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey($"prismacore_tooltip_{claim.Id}"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, $"prismacore_tooltip_{claim.Id}");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
        }

        private static void IntrusionHandler(ClientInfo ci, Vector3i pos, DbClaim claim, string opentimes)
        {
            Vector3 newPos = new Vector3();
            string msg = ServerCoreStrings.Instance.AdvClaims_Normal;

            int halfwayX = Math.Abs((claim.E_bound - claim.W_bound) / 2);
            int halfwayY = Math.Abs((claim.N_bound - claim.S_bound) / 2);

            int boundaryX = claim.E_bound - halfwayX;
            int boundaryY = claim.N_bound - halfwayY;

            //left/above
            if (pos.x <= boundaryX && pos.z >= boundaryY)
            {
                //tele corner w_bound, n_bound
                newPos.x = claim.W_bound - TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.N_bound + TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_normal"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, "prismacore_tooltip_normal");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                if (!string.IsNullOrEmpty(opentimes))
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OpenTime.Replace("{openTime}", opentimes)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }

            //right/above
            if (pos.x >= boundaryX && pos.z >= boundaryY)
            {
                //tele corner e_bound, n_bound
                newPos.x = claim.E_bound + TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.N_bound + TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_normal"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, "prismacore_tooltip_normal");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                if (!string.IsNullOrEmpty(opentimes))
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OpenTime.Replace("{openTime}", opentimes)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
            //left/below
            if (pos.x <= boundaryX && pos.z <= boundaryY)
            {
                //tele corner w_bound, s_bound
                newPos.x = claim.W_bound - TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.S_bound - TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_normal"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, "prismacore_tooltip_normal");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                if (!string.IsNullOrEmpty(opentimes))
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OpenTime.Replace("{openTime}", opentimes)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
            //right/below
            if (pos.x >= boundaryX && pos.z <= boundaryY)
            {
                //tele corner e_bound, s_bound
                newPos.x = claim.E_bound + TpExtraDistance;
                newPos.y = -1;
                newPos.z = claim.S_bound - TpExtraDistance;

                NetPackageTeleportPlayer pkg = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(newPos);
                ci.SendPackage(pkg);

                if (BuffManager.Buffs.ContainsKey("prismacore_tooltip_normal"))
                {
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    showTooltip(ep, "prismacore_tooltip_normal");
                }
                else
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, msg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                if (!string.IsNullOrEmpty(opentimes))
                {
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, ServerCoreStrings.Instance.AdvClaims_OpenTime.Replace("{openTime}", opentimes)), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }

                return;
            }
        }

        private static bool IsBloodMoonTime(string param)
        {
            //bmonly(17-10)
            string time = param.Replace("bmonly(", string.Empty);
            time = time.Replace(")", string.Empty);
            string from = time.Split('-')[0];
            string to = time.Split('-')[1];

            int openFrom = int.Parse(from);
            int openTo = int.Parse(to);

            int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int currentHour = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
            int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));

            int q = Math.DivRem(days, BMcycle, out int remainder);

            if (remainder == 0)
            {
                //if remainder=0 but hours < 4 report 22:00
                if (currentHour >= openFrom)
                {
                    return true;
                }
            }
            else if (remainder == 1 && currentHour < openTo)
            {
                //day after BM
                return true;
            }

            return false;

        }

        private static void ProtectAdmin(ClientInfo ci, Vector3i pos2)
        {
            List<Entity> hostiles = new List<Entity>();

            try
            {
                counter++;
                if (counter >= 6)
                {
                    //FE2E2E
                    ci.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, "[FE2E2E]Protective bubble is ACTIVE![-]"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    counter = 0;
                }

                hostiles.Clear();

                List<Entity> list = new List<Entity>(GameManager.Instance.World.Entities.list);
                for (int i = 0; i < list.Count; i++)
                {
                    Entity entity = list[i];
                    if (entity != null && !(entity is EntityPlayer) && entity is EntityAlive && !(entity is EntityVehicle) && !(entity is EntityTurret) && EntityClass.list[entity.entityClass].bIsEnemyEntity)
                    {
                        Vector3i hostilePos = new Vector3i(entity.GetPosition());
                        if (Math.Abs(pos2.x - hostilePos.x) < 25 && Math.Abs(pos2.y - hostilePos.y) < 25 && Math.Abs(pos2.z - hostilePos.z) < 25)
                        {
                            hostiles.Add(entity);
                        }
                    }
                }

                if (hostiles.Count > 0)
                {
                    foreach (EntityAlive ea in hostiles.Cast<EntityAlive>())
                    {
                        if (ea.GetType().ToString() == "EntityTrader")
                        {
                            continue;
                        }
                        try
                        {
                            GameManager.Instance.World.RemoveEntity(ea.entityId, EnumRemoveEntityReason.Killed);
                        }
                        catch { continue; }
                    }
                }
            }
            catch (Exception e) { Log.Error(e.ToString()); }
        }

        private static int getPlayerHeight(EntityPlayer ep)
        {
            Vector3i blockpos = ep.GetBlockPosition();
            BlockValue Block = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y - 1, blockpos.z);

            int height = 0;
            bool platformChecked = false;

            if (Block.type == BlockValue.Air.type)
            {
                while (Block.type == BlockValue.Air.type)
                {
                    if (!platformChecked)
                    {
                        BlockValue platformBlock = GameManager.Instance.World.GetBlock(blockpos.x - 1, blockpos.y - 1, blockpos.z);
                        BlockValue platformBlockRampOrHalfBlock = GameManager.Instance.World.GetBlock(blockpos.x - 1, blockpos.y, blockpos.z);
                        if (platformBlock.type != BlockValue.Air.type) return 0;
                        else if (platformBlockRampOrHalfBlock.type != BlockValue.Air.type) return 0;
                        platformBlock = GameManager.Instance.World.GetBlock(blockpos.x + 1, blockpos.y - 1, blockpos.z);
                        platformBlockRampOrHalfBlock = GameManager.Instance.World.GetBlock(blockpos.x + 1, blockpos.y, blockpos.z);
                        if (platformBlock.type != BlockValue.Air.type) return 0;
                        else if (platformBlockRampOrHalfBlock.type != BlockValue.Air.type) return 0;
                        platformBlock = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y - 1, blockpos.z - 1);
                        platformBlockRampOrHalfBlock = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y, blockpos.z - 1);
                        if (platformBlock.type != BlockValue.Air.type) return 0;
                        else if (platformBlockRampOrHalfBlock.type != BlockValue.Air.type) return 0;
                        platformBlock = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y - 1, blockpos.z + 1);
                        platformBlockRampOrHalfBlock = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y, blockpos.z + 1);
                        if (platformBlock.type != BlockValue.Air.type) return 0;
                        else if (platformBlockRampOrHalfBlock.type != BlockValue.Air.type) return 0;
                        platformChecked = true;
                    }
                    height++;
                    Block = GameManager.Instance.World.GetBlock(blockpos.x, blockpos.y - 1 - height, blockpos.z);
                }
            }

            return height;
        }

        private static void showTooltip(EntityPlayer ep, string tooltip)
        {
            if (BuffManager.Buffs.ContainsKey(tooltip))
            {
                ep.Buffs.AddBuff(tooltip, -1, true, false);
            }
            else
            {
                Log.Out($"[PrismaCore] ERR: showTooltip: Tooltip {tooltip} can not be found.");
            }
        }

        private static void removeTooltip(EntityPlayer ep, string tooltip)
        {
            if (BuffManager.Buffs.ContainsKey(tooltip))
            {
                ep.Buffs.RemoveBuff(tooltip);
            }
            else
            {
                Log.Out($"[PrismaCore] ERR: removeTooltip: Tooltip {tooltip} can not be found.");
            }
        }
    }

    public static class Extension
    {
        public static bool UseAsOperator(this string op, int x, int y)
        {
            switch (op)
            {
                case ">=": return x >= y;
                case "=>": return x >= y;
                case "<=": return x <= y;
                case "=<": return x <= y;
                case "==": return x == y;
                case "!=": return x != y;
                default: throw new Exception("invalid logic in expression for playerlevel claim");
            }
        }
    }
}
