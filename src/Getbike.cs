using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore
{
    public class Getbike
    {
        public static void Findbikes(ClientInfo _cInfo)
        {
            try
            {
                Vector3 position = default(Vector3);
                List<Entity> list = GameManager.Instance.World.Entities.list;
                bool flag = false;
                for (int i = 0; i < list.Count; i++)
                {
                    Entity entity = list[i];
                    if (entity is EntityMinibike)
                    {
                        EntityMinibike entityMinibike = (EntityMinibike)entity;

                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_cInfo.entityId);

                        if (entityMinibike.IsOwner(playerDataFromEntityID.PlayerData.PrimaryId))
                        {
                            position = GameManager.Instance.World.Players.dict[_cInfo.entityId].GetPosition();
                            position.x += 1f;
                            position.z += 1f;
                            entityMinibike.SetPosition(position);
                            SdtdConsole.Instance.Output("MiniBike " + entityMinibike.entityId.ToString() + " teleported.");
                            flag = true;
                            break;
                        }
                        flag = false;
                    }
                }
                if (!flag)
                {
                    SdtdConsole.Instance.Output("MiniBike Could not be found.");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in getbike: " + e);
            }
        }
    }
}
