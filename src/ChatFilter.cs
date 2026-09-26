using System;
using System.Collections.Generic;

namespace PrismaCore
{
    public class ChatFilter
    {

        public static ModEvents.EModEventResult Exec(ClientInfo _cInfo, EChatType _type, string _message, string _playerName, List<int> _recipientEntityIds)
        {
            if (!string.IsNullOrEmpty(_message))
            {
                if (_cInfo != null)
                {

                    string message = _message.Trim().ToLower();


                    string chatName;
                    string pf = PrismaCoreSettings.Instance.PrismaCorePrefix;
                    if (string.IsNullOrEmpty(pf))
                        pf = "/";

                    string serverChatName = PrismaCoreStrings.Instance.ServerChatName;

                    if (message.StartsWith(pf + "ls"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_ls >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "ls received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                            if (message.Equals(pf + "ls"))
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Ls_NoParameters), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                            else
                            {
                                string parameter = message.Split(new string[] { pf + "ls" }, StringSplitOptions.None)[1].Trim();
                                bool success = seen.Exec(_cInfo, parameter);
                                if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Ls_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                        }
                    }

                    if (message.StartsWith(pf + "day7"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_day7 >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "day7 received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = day7.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Day7_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "aaf"))
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "aaf received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        bool success = batchFriends.Exec(_cInfo, "add");
                        if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Aaf_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return ModEvents.EModEventResult.StopHandlersAndVanilla;
                    }

                    if (message.StartsWith(pf + "raf"))
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "raf received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        bool success = batchFriends.Exec(_cInfo, "del");
                        if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Raf_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return ModEvents.EModEventResult.StopHandlersAndVanilla;
                    }

                    if (message.StartsWith(pf + "rt"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_rt >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "rt received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = ReturnAdmin.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FFA07A]Something went wrong! I was not able to tp you back to origin.[-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "hostiles"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_hostiles >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "hostiles received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = Hostiles.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Hostiles_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "bag"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_bag >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "bag received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = Bag.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Bag_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "get"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_get >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Received Command: " + pf + "get"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "get" }, StringSplitOptions.None)[1].Trim();
                            bool success = Get.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Get_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "bed"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_bed >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Received Command: " + pf + "bed"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = Bed.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Bed_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "tb"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_tb >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Received Command: " + pf + "tb"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "tb" }, StringSplitOptions.None)[1].Trim();
                            bool success = Return.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FFA07A]Something went wrong! Could not tp the player to origin.[-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "bubble"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_bubble >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Received Command: " + pf + "bubble"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "bubble" }, StringSplitOptions.None)[1].Trim();
                            bool success = Protect.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FFA07A]Something went wrong! Could not activate protective bubble![-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "ftw"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_ftw >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "ftw received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "ftw" }, StringSplitOptions.None)[1].Trim();
                            bool success = Flytowp.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Ftw_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "ft"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_ft >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "ft received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            
                            if (message.Equals(pf + "ft"))
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Ft_NoParameters), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                            else
                            {
                                string parameter = message.Split(new string[] { pf + "ft" }, StringSplitOptions.None)[1].Trim();
                                bool success = Flyto.Exec(_cInfo, parameter);
                                if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Ft_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                        }
                    }

                    if (message.StartsWith(pf + "mvw"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_mvw >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "mvw received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "mvw" }, StringSplitOptions.None)[1].Trim();
                            bool success = Movewp.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Mvw_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "mv"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_mv >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "mv received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "mv" }, StringSplitOptions.None)[1].Trim();
                            bool success = Move.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Mv_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "listwp"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_listwp >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "listwp received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            bool success = Listwp.Exec(_cInfo);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Listwp_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "setwp"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_setwp >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "setwp received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "setwp" }, StringSplitOptions.None)[1].Trim();
                            bool success = Setwp.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Setwp_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "delwp"))
                    {
                        if (PrismaCoreSettings.Instance.ChatCommandPermissions_delwp >= 0)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "delwp received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { pf + "delwp" }, StringSplitOptions.None)[1].Trim();
                            bool success = Delwp.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Delwp_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (message.StartsWith(pf + "acf"))
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "acf received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        string parameter = message.Split(new string[] { pf + "acf" }, StringSplitOptions.None)[1].Trim();
                        bool success = Addclaimfriend.Exec(_cInfo, parameter);
                        if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Acf_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return ModEvents.EModEventResult.StopHandlersAndVanilla;
                    }

                    if (message.StartsWith(pf + "rcf"))
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "rcf received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        string parameter = message.Split(new string[] { pf + "rcf" }, StringSplitOptions.None)[1].Trim();
                        bool success = Removeclaimfriend.Exec(_cInfo, parameter);
                        if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Rcf_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return ModEvents.EModEventResult.StopHandlersAndVanilla;
                    }

                    if (message.StartsWith(pf + "lcf"))
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command " + pf + "lcf received"), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        string parameter = message.Split(new string[] { pf + "lcf" }, StringSplitOptions.None)[1].Trim();
                        bool success = Listclaimfriends.Exec(_cInfo, parameter);
                        if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Lcf_Error), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        return ModEvents.EModEventResult.StopHandlersAndVanilla;
                    }

                    if (message.StartsWith(PrismaCoreSettings.Instance.LocationTracker_ChatCommand.Trim().ToLower()))
                    {
                        string command = PrismaCoreSettings.Instance.LocationTracker_ChatCommand.Trim().ToLower();
                        if (PrismaCoreSettings.Instance.LocationTracker_ChatCommandEnabled)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Received Command: " + command), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            string parameter = message.Split(new string[] { command }, StringSplitOptions.None)[1].Trim();
                            bool success = Who.Exec(_cInfo, parameter);
                            if (!success) _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FFA07A]Something went wrong! I was not able to check who was here.[-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                        else
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FFA07A]Locationtrack command is not enabled![-]", null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    if (PrismaCoreSettings.Instance.MaxChatLength > 0)
                    {
                        if (_message.Length > PrismaCoreSettings.Instance.MaxChatLength)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Chat_MaxLength), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }
                    }

                    //Check its a command
                    if (PrismaCoreSettings.Instance.HideChatCommandPrefixes_Enabled)
                    {
                        if (PrismaCoreSettings.Instance.HideChatCommandPrefixes_Prefixes.Contains(","))
                        {
                            string[] arrPrefixes = PrismaCoreSettings.Instance.HideChatCommandPrefixes_Prefixes.Split(',');
                            for (int i = 0; i < arrPrefixes.Length; i++)
                            {
                                if (_message.Trim().StartsWith(arrPrefixes[i]))
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command received " + _message), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                    Log.Out($"Chat (from '{_cInfo.PlatformId}', entity id '{_cInfo.entityId}', to 'Global'): '{_cInfo.playerName}':{_message}");
                                    return ModEvents.EModEventResult.StopHandlersAndVanilla;
                                }
                            }
                        }
                        else
                        {
                            if (_message.Trim().StartsWith(PrismaCoreSettings.Instance.HideChatCommandPrefixes_Prefixes))
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, "[" + PrismaCoreSettings.Instance.CommandReceivedColor + "]" + "Command received " + _message), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                                Log.Out($"Chat (from '{_cInfo.PlatformId}', entity id '{_cInfo.entityId}', to 'Global'): '{_cInfo.playerName}':{_message}");
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                        }
                    }

                    DbPlayer dbplayer = Database.Instance.GetDbPlayer(_cInfo.PlatformId.ToString());

                    if (dbplayer != null)
                    {
                        if (dbplayer.ChatMuted)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(serverChatName, PrismaCoreStrings.Instance.Chat_Muted), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }

                        //individual coloring
                        if (!string.IsNullOrEmpty(dbplayer.ChatColor))
                        {
                            chatName = dbplayer.ChatNameOverride;
                            if (string.IsNullOrEmpty(chatName))
                                chatName = _cInfo.playerName;

                            if (dbplayer.ChatName)
                            {
                                if (_type == EChatType.Friends)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }
                                if (_type == EChatType.Party)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }

                                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(dbplayer.ChatColor + chatName + "[-]", _message), null,EMessageSender.None);
                            }
                            else
                            {
                                if (_type == EChatType.Friends)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }
                                if (_type == EChatType.Party)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }

                                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(dbplayer.ChatColor + chatName + "[-]", dbplayer.ChatColor + _message + "[-]"), null, EMessageSender.None);
                            }

                            
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }

                        //groupChat coloring
                        string group = dbplayer.MemberOfGroup;
                        if (!string.IsNullOrEmpty(group))
                        {
                            string groupColor = Database.Instance.GetGroupColor(group);
                            if (!string.IsNullOrEmpty(groupColor))
                            {
                                groupColor = string.Format("[{0}]", groupColor);
                                chatName = dbplayer.ChatNameOverride;
                                if (string.IsNullOrEmpty(chatName))
                                    chatName = _cInfo.playerName;

                                if (_type == EChatType.Friends)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }
                                if (_type == EChatType.Party)
                                {
                                    return ModEvents.EModEventResult.StopHandlersRunVanilla;
                                }

                                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(groupColor + chatName + "[-]", _message), null, EMessageSender.None);

                                
                                return ModEvents.EModEventResult.StopHandlersAndVanilla;
                            }
                        }

                        //check for normal chat if chatname is overridden
                        chatName = dbplayer.ChatNameOverride;
                        if (!string.IsNullOrEmpty(chatName))
                        {
                            if (_type == EChatType.Friends)
                            {
                                return ModEvents.EModEventResult.StopHandlersRunVanilla;
                            }
                            if (_type == EChatType.Party)
                            {
                                return ModEvents.EModEventResult.StopHandlersRunVanilla;
                            }

                            GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(chatName, _message), null, EMessageSender.None);
                            
                            return ModEvents.EModEventResult.StopHandlersAndVanilla;
                        }

                    }
                }
                else
                {
                }
            }

            return ModEvents.EModEventResult.Continue;
        }

    }
}
