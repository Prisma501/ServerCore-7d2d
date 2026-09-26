using PrismaCore.JSON;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace PrismaCore.Web.API
{
    public class GetVehicles : WebAPI
    {

        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            JSONObject result = new JSONObject();

            JSONArray vehicles = new JSONArray();
            result.Add("Vehicles", vehicles);

            //lock (result)
            //{
            try
            {
                //LoadVehicles();
                List<EntityVehicle> vehics = new List<EntityVehicle>(RegionReset.vehicles);
                List<EntityCreationData> stubs = new List<EntityCreationData>(RegionReset.vehicleStubs);

                for (int i = 0; i < stubs.Count; i++)
                {
                    EntityCreationData ecd = stubs[i];
                    if (ecd != null)
                    {
                        Vector3 vehpos = ecd.pos;

                        //EntityClass.list.TryGetValue(ecd.entityClass, out EntityClass entClass);

                        JSONObject vehicle = new JSONObject();
                        vehicles.Add(vehicle);

                        string owner = Database.Instance.GetVehicleOwner(ecd.id);

                        if (string.IsNullOrEmpty(owner))
                        {
                            owner = "unknown";
                        }

                        //vehicle.Add("name", new JSONString(entClass.classname.Name + " (Id: " + ecd.id + ")"));
                        vehicle.Add("name", new JSONString($"(Unloaded){EntityClass.GetEntityClassName(ecd.entityClass).Replace("vehicle", string.Empty)} (Id: {ecd.id})<BR>Owner: {owner}"));
                        vehicle.Add("posX", new JSONNumber(Utils.Fastfloor(vehpos.x)));
                        vehicle.Add("posY", new JSONNumber(Utils.Fastfloor(vehpos.y)));
                        vehicle.Add("posZ", new JSONNumber(Utils.Fastfloor(vehpos.z)));
                    }
                }

                for (int i = 0; i < vehics.Count; i++)
                {
                    EntityVehicle veh = vehics[i];
                    if (veh != null)
                    {

                        Vector3i vehpos = veh.GetBlockPosition();

                        JSONObject vehicle = new JSONObject();
                        vehicles.Add(vehicle);

                        //get the current owner
                        PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(veh.GetOwner());
                        string owner = string.Empty;

                        if (playerDataFromEntityID != null)
                        {
                            owner = playerDataFromEntityID.PlayerName.DisplayName;
                        }
                        if (string.IsNullOrEmpty(owner))
                        {
                            vehicle.Add("name", new JSONString(veh.LocalizedEntityName));
                        }
                        else
                        {
                            vehicle.Add("name", new JSONString($"{veh.LocalizedEntityName} (Id: {veh.entityId})<BR>Owner: {owner}"));
                        }
                        vehicle.Add("posX", new JSONNumber(vehpos.x));
                        vehicle.Add("posY", new JSONNumber(vehpos.y));
                        vehicle.Add("posZ", new JSONNumber(vehpos.z));
                    }
                }

                WriteJSON(_resp, result);

            }
            catch { }
            //}
        }
    }
}