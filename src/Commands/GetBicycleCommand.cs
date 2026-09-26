using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class GetBicycleCommand : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Get lost or stuck bicycle to player";
        }

        public override string getHelp()
        {
            return "Get bicycle Usage:\n   getbicycle <steam id/player name/entity id>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-getbicycle", "getbicycle" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Usage: getbicycle <entityid|playername|steamid>");
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
                                Vector3 position = default(Vector3);
                                List<Entity> list = GameManager.Instance.World.Entities.list;
                                bool flag = false;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    Entity entity = list[i];
                                    if (entity is EntityBicycle)
                                    {
                                        EntityBicycle entityBicycle = (EntityBicycle)entity;

                                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(clientInfo.entityId);

                                        if (entityBicycle.IsOwner(playerDataFromEntityID.PlayerData.PrimaryId))
                                        {
                                            position = GameManager.Instance.World.Players.dict[clientInfo.entityId].GetPosition();
                                            position.x += 1f;
                                            position.z += 1f;
                                            entityBicycle.SetPosition(position);
                                            SdtdConsole.Instance.Output("Bicycle " + entityBicycle.entityId.ToString() + " teleported.");
                                            flag = true;
                                            break;
                                        }
                                        flag = false;
                                    }
                                }
                                if (!flag)
                                {
                                    SdtdConsole.Instance.Output("Bicycle Could not be found.");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Out("Error in getBicycle: " + e);
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
                    SdtdConsole.Instance.Output("Usage: getbicycle <entityid|playername|steamid>");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in getbicycle Command: " + e);
            }
        }
    }
}
