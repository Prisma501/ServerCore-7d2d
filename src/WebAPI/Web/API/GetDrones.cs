using ServerCore.JSON;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace ServerCore.Web.API
{
    public class GetDrones : WebAPI
    {

        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            JSONObject result = new JSONObject();

            JSONArray drones = new JSONArray();
            result.Add("Drones", drones);

            try
            {
                List<EntityDrone> drons = new List<EntityDrone>(RegionReset.drones);
                List<EntityCreationData> stubs = new List<EntityCreationData>(RegionReset.droneStubs);

                for (int i = 0; i < stubs.Count; i++)
                {
                    EntityCreationData ecd = stubs[i];
                    if (ecd != null)
                    {
                        Vector3 dronpos = ecd.pos;

                        JSONObject drone = new JSONObject();
                        drones.Add(drone);

                        string owner = Database.Instance.GetDroneOwner(ecd.id);

                        if (string.IsNullOrEmpty(owner))
                        {
                            owner = "unknown";
                        }

                        drone.Add("name", new JSONString($"(Unloaded){EntityClass.GetEntityClassName(ecd.entityClass).Replace("drone", string.Empty)} (Id: {ecd.id})<BR>Owner: {owner}"));
                        drone.Add("posX", new JSONNumber(Utils.Fastfloor(dronpos.x)));
                        drone.Add("posY", new JSONNumber(Utils.Fastfloor(dronpos.y)));
                        drone.Add("posZ", new JSONNumber(Utils.Fastfloor(dronpos.z)));
                    }
                }

                for (int i = 0; i < drons.Count; i++)
                {
                    EntityDrone dron = drons[i];
                    if (dron != null)
                    {

                        Vector3i dronpos = dron.GetBlockPosition();

                        JSONObject drone = new JSONObject();
                        drones.Add(drone);

                        //get the current owner
                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(dron.GetOwner());
                        string owner = string.Empty;

                        if (playerDataFromEntityID != null)
                        {
                            owner = playerDataFromEntityID.PlayerName.DisplayName;
                            Database.Instance.SetDroneOwner(dron.entityId, owner);
                        }
                        if (string.IsNullOrEmpty(owner))
                        {
                            drone.Add("name", new JSONString(dron.LocalizedEntityName));
                        }
                        else
                        {
                            drone.Add("name", new JSONString($"{dron.LocalizedEntityName} (Id: {dron.entityId})<BR>Owner: {owner}"));
                        }
                        drone.Add("posX", new JSONNumber(dronpos.x));
                        drone.Add("posY", new JSONNumber(dronpos.y));
                        drone.Add("posZ", new JSONNumber(dronpos.z));
                    }
                }

                WriteJSON(_resp, result);

            }
            catch { }
        }
    }
}