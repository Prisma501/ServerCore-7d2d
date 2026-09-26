using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore.CustomCommands
{
    public class GetGyroCommand : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Get lost or stuck gyrocopter to player";
        }

        public override string getHelp()
        {
            return "Get gyrocopter Usage:\n   getgyrocopter <steam id/player name/entity id>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-getgyrocopter", "getgyrocopter" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Usage: getgyrocopter <entityid|playername|steamid>");
                }
                else if (_params.Count == 1)
                {
                    ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);
                    if (clientInfo == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                    }
                    else
                    {
                        if (_senderInfo.RemoteClientInfo == null || _senderInfo.RemoteClientInfo.PlatformId == null)
                        {
                            _senderInfo.RemoteClientInfo = clientInfo;
                        }
                        if (_senderInfo.RemoteClientInfo == clientInfo || GameManager.Instance.adminTools.Users.HasEntry(_senderInfo.RemoteClientInfo))
                        {
                            try
                            {
                                //SdtdConsole.Instance.Output("Lost MiniBike command sent by player: " + _cInfo.playerName + " id: " + _cInfo.playerId);
                                Vector3 position = default(Vector3);
                                List<Entity> list = GameManager.Instance.World.Entities.list;
                                //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage> ().Setup(EnumGameMessages.Chat, "Looking for your Minibike", "", false, "", false));
                                bool flag = false;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    Entity entity = list[i];
                                    if (entity is EntityVGyroCopter)
                                    {
                                        EntityVGyroCopter entityGyro = (EntityVGyroCopter)entity;

                                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(clientInfo.entityId);

                                        if (entityGyro.IsOwner(playerDataFromEntityID.PlayerData.PrimaryId))
                                        {
                                            //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage> ().Setup(EnumGameMessages.Chat, "Found Your MiniBike!", "", false, "", false));
                                            position = GameManager.Instance.World.Players.dict[clientInfo.entityId].GetPosition();
                                            position.x += 1f;
                                            position.z += 1f;
                                            entityGyro.SetPosition(position);
                                            SdtdConsole.Instance.Output("Gyrocopter " + entityGyro.entityId.ToString() + " teleported.");
                                            //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage> ().Setup(EnumGameMessages.Chat, "MiniBike Teleported", "", false, "", false));
                                            //entityGyro.UseHorn();
                                            flag = true;
                                            break;
                                        }
                                        flag = false;
                                    }
                                }
                                if (!flag)
                                {
                                    //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage> ().Setup(EnumGameMessages.Chat, "Could not find your Minibike", "", false, "", false));
                                    SdtdConsole.Instance.Output("Gyrocopter Could not be found.");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Out("Error in getGyrocopter: " + e);
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("ERR: You are not the owner or admin.");
                        }
                    }
                }
                else if (_params.Count == 0)
                {
                    SdtdConsole.Instance.Output("Usage: getgyrocopter <entityid|playername|steamid>");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in getgyrocopter Command: " + e);
            }
        }
    }
}
