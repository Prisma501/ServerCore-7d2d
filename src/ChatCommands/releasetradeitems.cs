//using Platform.Steam;
//using System;
//using System.Collections.Generic;

//namespace PrismaCore
//{
//    class releasetradeitems
//    {

//        public static bool Exec(ClientInfo _cInfo, string tradingtarget)
//        {
//            try
//            {
//                ClientInfo clientinfo = null;
//                string sItems = "";
//                string targetSteamId = "";

//                foreach (KeyValuePair<int, EntityPlayer> player in GameManager.Instance.World.Players.dict)
//                {
//                    clientinfo = ConnectionManager.Instance.Clients.ForEntityId(player.Key);
//                    if (clientinfo == null) continue;

//                    string myTradingtarget = tradingtarget;

//                    if (myTradingtarget.StartsWith("\"") && myTradingtarget.EndsWith("\""))
//                    {
//                        myTradingtarget = myTradingtarget.Replace("\"", string.Empty);

//                        if (clientinfo.playerName.ToUpper().Equals(myTradingtarget.ToUpper()))
//                        {
//                            targetSteamId = clientinfo.PlatformId.ToString();
//                            break;
//                        }
//                    }
//                    else
//                    {
//                        if (clientinfo.playerName.IndexOf(myTradingtarget, StringComparison.OrdinalIgnoreCase) >= 0)
//                        {
//                            targetSteamId = clientinfo.PlatformId.ToString();
//                            break;
//                        }
//                    }
//                }

//                //check if target player object has been initiated
//                if (targetSteamId.Equals(""))
//                {
//                    string errMsg = PrismaCoreStrings.Instance.Rti_PlayerNotFound;
//                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, errMsg, PrismaCoreStrings.Instance.ServerChatName, null);
//                    return true;
//                }

//                if (_cInfo.PlatformId.ToString() == clientinfo.PlatformId.ToString())
//                {
//                    //AdminTools admTools = GameManager.Instance.adminTools;
//                    //if (admTools == null) return false;

//                    int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

//                    if (AdminLvL != 0)
//                    {
//                        //trading with self -> exploit, so prevent
//                        string errMsg = PrismaCoreStrings.Instance.Rti_SelfTrade;
//                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, errMsg, PrismaCoreStrings.Instance.ServerChatName, null);
//                        return true;
//                    }
//                }

//                bool allowed = true;
//                if (allowed)
//                {
//                    List<DbTradingChest> lstTradingchests = Database.Instance.GetAllDbTradingchests();
//                    if (lstTradingchests == null) return false;

//                    bool exists = false;
//                    foreach (var chest in lstTradingchests)
//                    {
//                        if (chest.Id == clientinfo.PlatformId.ToString())
//                        {
//                            //virtual chest of target exist
//                            exists = true;
//                        }

//                    }

//                    if (!exists)
//                    {
//                        string Msg = PrismaCoreStrings.Instance.Rti_NoTargetChest;
//                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, Msg, PrismaCoreStrings.Instance.ServerChatName, null);
//                        return true;
//                    }

//                    foreach (var chest in lstTradingchests)
//                    {
//                        if (_cInfo.PlatformId.ToString() == chest.Id)
//                        {
//                            if (!string.IsNullOrEmpty(chest.Items))
//                            {
//                                //allready registered items -> trade or cancel
//                                string Msg = PrismaCoreStrings.Instance.Rti_AllreadyReleased;
//                                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, Msg, PrismaCoreStrings.Instance.ServerChatName, null);
//                                return true;
//                            }


//                            //store existing values of tradingchest
//                            int ex = chest.X;
//                            int ey = chest.Y;
//                            int ez = chest.Z;

//                            //now get to the ingame chest and save items
//                            ChunkClusterList chunklist = GameManager.Instance.World.ChunkClusters;
//                            Dictionary<long, Chunk> dic = new Dictionary<long, Chunk>();
//                            for (int i = 0; i < chunklist.Count; i++)
//                            {
//                                ChunkCluster chunk = chunklist[i];
//                                LinkedList<Chunk> clist = chunk.GetChunkArray();
//                                foreach (Chunk c in clist)
//                                {
//                                    DictionaryList<Vector3i, TileEntity> tiles = c.GetTileEntities();
//                                    foreach (TileEntity tile in tiles.dict.Values)
//                                    {
//                                        TileEntityType type = tile.GetTileEntityType();

//                                        if (type.ToString().Equals("SecureLoot"))
//                                        {
//                                            TileEntitySecureLootContainer secureLoot = (TileEntitySecureLootContainer)tile;

//                                            Vector3i vec = secureLoot.ToWorldPos();

//                                            if (vec.x == chest.X && vec.y == chest.Y && vec.z == chest.Z)
//                                            {
//                                                //get items from own tradingchest
//                                                sItems = "";
//                                                foreach (ItemStack item in secureLoot.items)
//                                                {
//                                                    if (!item.IsEmpty())
//                                                    {
//                                                        ItemClass ib = ItemClass.list[item.itemValue.type];
//                                                        string itemName = ib.GetItemName();
//                                                        int maxUsedTimes = Math.Max(1, item.itemValue.MaxUseTimes);
//                                                        float usedTimes = (item.itemValue.UseTimes * 100) / maxUsedTimes;
//                                                        int qual = 0;
//                                                        string mods = string.Empty;

//                                                        //address Mods!!!!
//                                                        if (item.itemValue.HasModSlots)
//                                                        {
//                                                            string partsMsg = DoParts(item.itemValue.Modifications, 1, "");
//                                                            if (partsMsg != string.Empty)
//                                                            {
//                                                                mods = partsMsg;
//                                                                qual = item.itemValue.Quality;
//                                                                sItems += string.Format("{0}|{1}|{2}|{3}|{4}@", itemName, item.count, qual, usedTimes, mods);
//                                                            }
//                                                            else
//                                                            {
//                                                                //no mods on item with modslots
//                                                                qual = item.itemValue.Quality;
//                                                                sItems += string.Format("{0}|{1}|{2}|{3}@", itemName, item.count, qual, usedTimes);
//                                                            }
//                                                        }
//                                                        else if (item.itemValue.HasQuality)
//                                                        {
//                                                            qual = item.itemValue.Quality;
//                                                            sItems += string.Format("{0}|{1}|{2}|{3}@", itemName, item.count, qual, usedTimes);
//                                                        }
//                                                        else
//                                                        {
//                                                            sItems += string.Format("{0}|{1}@", itemName, item.count);
//                                                        }
//                                                    }
//                                                }

//                                                //check if empty items -> abort
//                                                if (string.IsNullOrEmpty(sItems))
//                                                {
//                                                    string emsg = PrismaCoreStrings.Instance.Rti_NoItems;
//                                                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, emsg, PrismaCoreStrings.Instance.ServerChatName, null);
//                                                    return true;
//                                                }

//                                                if (sItems.EndsWith("@")) sItems = sItems.Substring(0, sItems.Length - 1);
//                                                //Log.Out("1: " + sItems);
//                                                secureLoot.SetEmpty();
//                                                PlatformUserIdentifierAbs userId = new UserIdentifierSteam("11111111111111111");
//                                                secureLoot.SetOwner(userId);
//                                                secureLoot.SetLocked(true);

//                                                //existing chest
//                                                bool removed = Database.Instance.DeleteTradingChest(chest.Id);
//                                                if (removed)
//                                                {
//                                                    Log.Out("rti: " + sItems);

//                                                    Database.Instance.SaveDbTradingChest(_cInfo.PlatformId.ToString(), targetSteamId, sItems, false, chest.X, chest.Y, chest.Z);

//                                                    string Pmmsg = PrismaCoreStrings.Instance.Rti_SuccessSender.Replace("{playerName}", clientinfo.playerName);
//                                                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, Pmmsg, PrismaCoreStrings.Instance.ServerChatName, null);

//                                                    //inform other player of released items
//                                                    Pmmsg = PrismaCoreStrings.Instance.Rti_SuccessReceiver.Replace("{playerName}", _cInfo.playerName);
//                                                    GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, Pmmsg, PrismaCoreStrings.Instance.ServerChatName, null);

//                                                    //interprete sItems rockSmall|1000|0|0@rockSmall|2000|0|0@firstAidBandage|1|0|0

//                                                    //if (sItems == "") Log.Out("no item gotten from chest???");

//                                                    //address Mods!!!!


//                                                    string[] arrItems = sItems.Split('@');

//                                                    foreach (string item in arrItems)
//                                                    {
//                                                        string itemName = string.Empty;
//                                                        string quantity = string.Empty;
//                                                        string quality = "0";
//                                                        string usedTimes = string.Empty;
//                                                        string mods = "none";

//                                                        string[] Item = item.Split('|');
//                                                        if (Item.Length == 2)
//                                                        {
//                                                            itemName = Item[0];
//                                                            quantity = Item[1];

//                                                            Pmmsg = string.Format(" {0},  C: {1}", itemName, quantity);

//                                                            GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[FFFFFF]{Pmmsg}[-]", "*", null);
//                                                            GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[FFFFFF]{Pmmsg}[-]", "*", null);

//                                                            //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[FFFFFF]{Pmmsg}[-]", "*", false, null));
//                                                            //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[FFFFFF]{Pmmsg}[-]", "*", false, null));
//                                                            continue;
//                                                        }
//                                                        else if (Item.Length == 4)
//                                                        {
//                                                            itemName = Item[0];
//                                                            quantity = Item[1];
//                                                            quality = Item[2];
//                                                            usedTimes = Item[3];

//                                                        }
//                                                        else
//                                                        {
//                                                            //lenght = 5 -> has mods
//                                                            itemName = Item[0];
//                                                            quantity = Item[1];
//                                                            quality = Item[2];
//                                                            usedTimes = Item[3];
//                                                            if (!string.IsNullOrEmpty(Item[4]))
//                                                            {
//                                                                mods = Item[4];
//                                                            }
//                                                        }

//                                                        int qual = Int32.Parse(quality);
//                                                        string color = "";

//                                                        if (qual <= 1)
//                                                        {
//                                                            color = "9C8867";
//                                                        }
//                                                        else if (qual == 2)
//                                                        {
//                                                            color = "CF7F29";
//                                                        }
//                                                        else if (qual == 3)
//                                                        {
//                                                            color = "FFFF00";
//                                                        }
//                                                        else if (qual == 4)
//                                                        {
//                                                            color = "00FF00";
//                                                        }
//                                                        else if (qual == 5)
//                                                        {
//                                                            color = "4169E1";
//                                                        }
//                                                        else
//                                                        {
//                                                            color = "9932CC";
//                                                        }

//                                                        if (mods.Equals("none"))
//                                                        {
//                                                            Pmmsg = string.Format(" {0},  C:{1}, Q:{2}, %U:{3}, M:{4}", itemName, quantity, quality, usedTimes, mods);
//                                                        }
//                                                        else
//                                                        {
//                                                            if (mods.Contains(":"))
//                                                            {
//                                                                string[] arr = mods.Split(':');
//                                                                Pmmsg = string.Format(" {0},  C:{1}, Q:{2}, %U:{3}", itemName, quantity, quality, usedTimes);

//                                                                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);
//                                                                GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);

//                                                                //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));

//                                                                foreach (string mod in arr)
//                                                                {
//                                                                    string m = mod;
//                                                                    m = m.Replace(";", ":");
//                                                                    Pmmsg = string.Format("   Mod: {0}", m);
//                                                                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);
//                                                                    GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);

//                                                                    //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                    //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                }
//                                                                continue;
//                                                            }
//                                                            else
//                                                            {
//                                                                //just one mod
//                                                                Pmmsg = string.Format(" {0},  C:{1}, Q:{2}, %U:{3}", itemName, quantity, quality, usedTimes);

//                                                                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);
//                                                                GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);

//                                                                //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));

//                                                                string m = mods;
//                                                                m = m.Replace(";", ":");
//                                                                Pmmsg = string.Format("   Mod: {0}", m);
//                                                                GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);
//                                                                GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);

//                                                                //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                                continue;
//                                                            }

//                                                        }

//                                                        GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);
//                                                        GameManager.Instance.ChatMessageServer(clientinfo, EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", null);

//                                                        //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                        //clientinfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat> ().Setup(EChatType.Whisper, -1, $"[{color}]{Pmmsg}[-]", "*", false, null));
//                                                    }
//                                                    return true;
//                                                }
//                                                return true;
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                    //if your here no virtual chest was found
//                    string errMsg = PrismaCoreStrings.Instance.Rti_NoChest;
//                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, errMsg, PrismaCoreStrings.Instance.ServerChatName, null);
//                    return true;
//                    // _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage> ().Setup(EnumGameMessages.Chat, "[F7FE2E]" + Pmmsg + "[-]", "Lara", false, "", false));
//                }
//                else
//                {
//                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
//                    GameManager.Instance.ChatMessageServer(_cInfo, EChatType.Whisper, -1, errMsg, PrismaCoreStrings.Instance.ServerChatName, null);
//                    return true;
//                }
//            }
//            catch { return false; }
//        }

//        private static string DoParts(ItemValue[] _parts, int _indent, string returnMsg)
//        {
//            if (_parts != null && _parts.Length > 0)
//            {
//                for (int i = 0; i < _parts.Length; i++)
//                {
//                    if (_parts[i] != null)
//                    {
//                        if (_parts[i].type != ItemValue.None.type)
//                        {
//                            if (!returnMsg.Trim().Equals(""))
//                            {
//                                returnMsg += ":";
//                            }

//                            ItemClass ib = ItemClass.list[_parts[i].type];
//                            int qual = _parts[i].Quality;
//                            string itemName = ib.GetItemName();

//                            //if (_parts[i].HasQuality)
//                            //    returnMsg += itemName + "@" + _parts[i].Quality + "@" + _parts[i].UseTimes;
//                            //else
//                            //{
//                            returnMsg += $"{itemName};{qual.ToString()}";
//                            //}
//                        }

//                        //returnMsg = DoParts(_parts[i].Modifications, _indent + 1, returnMsg);
//                    }
//                }
//            }
//            return returnMsg;
//        }
//    }
//}
