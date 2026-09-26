using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class GetMotorcycleCommand : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Get lost or stuck motorcycle to player";
        }

        public override string getHelp()
        {
            return "Get motorcycle Usage:\n   getmotorcycle <steam id/player name/entity id>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-getmotorcycle", "getmotorcycle" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Usage: getmotorcycle <entityid|playername|steamid>");
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
                                    if (entity is EntityMotorcycle)
                                    {
                                        EntityMotorcycle entityMoto = (EntityMotorcycle)entity;

                                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(clientInfo.entityId);

                                        if (entityMoto.IsOwner(playerDataFromEntityID.PlayerData.PrimaryId))
                                        {
                                            position = GameManager.Instance.World.Players.dict[clientInfo.entityId].GetPosition();
                                            position.x += 1f;
                                            position.z += 1f;
                                            entityMoto.SetPosition(position);
                                            SdtdConsole.Instance.Output("Motorcycle " + entityMoto.entityId.ToString() + " teleported.");
                                            flag = true;
                                            break;
                                        }
                                        flag = false;
                                    }
                                }
                                if (!flag)
                                {
                                    SdtdConsole.Instance.Output("Motorcycle Could not be found.");
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Out("Error in getMotorcycle: " + e);
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
                    SdtdConsole.Instance.Output("Usage: getmotorcycle <entityid|playername|steamid>");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in getMotorcycle Command: " + e);
            }
        }
    }
}
