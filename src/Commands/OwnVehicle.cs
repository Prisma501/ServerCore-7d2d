using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class OwnVehicle : ConsoleCmdAbstract
    {
        private static PlatformUserIdentifierAbs prevOwner;

        public override string getDescription()
        {
            return "take ownership of a vehicle.";
        }

        public override string getHelp()
        {
            return "ov <vehicleID>";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-ov", "ov", "ownvehicle" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                PlatformUserIdentifierAbs SteamID = _senderInfo.RemoteClientInfo.PlatformId;

                if (SteamID == null)
                {
                    SdtdConsole.Instance.Output("ERR: This command can only be run ingame!");
                    return;
                }

                if (!int.TryParse(_params[0], out int bikeID))
                {
                    SdtdConsole.Instance.Output("ERR: Invalid vehicleID format!");
                    return;
                }

                List<Entity> entityList = GameManager.Instance.World.Entities.list;
                bool vehicleFound = false;
                for (int i = 0; i < entityList.Count; i++)
                {
                    Entity entity = entityList[i];
                    if (entity is EntityMinibike)
                    {
                        EntityMinibike entityMinibike = (EntityMinibike)entity;

                        if (entityMinibike.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityMinibike.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityMinibike.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityMinibike.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own minibike: " + entityMinibike.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityMinibike.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Minibike " + entityMinibike.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityVBlimp)
                    {
                        EntityVBlimp entityBlimp = (EntityVBlimp)entity;

                        if (entityBlimp.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityBlimp.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityBlimp.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityBlimp.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Jetpack: " + entityBlimp.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityBlimp.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Jetpack " + entityBlimp.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityVHelicopter)
                    {
                        EntityVHelicopter entityHelicopter = (EntityVHelicopter)entity;

                        if (entityHelicopter.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityHelicopter.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityHelicopter.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityHelicopter.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Helicopter: " + entityHelicopter.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityHelicopter.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Helicopter " + entityHelicopter.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityVJeep)
                    {
                        EntityVJeep entityJeep = (EntityVJeep)entity;

                        if (entityJeep.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityJeep.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityJeep.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityJeep.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Jeep: " + entityJeep.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityJeep.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Jeep " + entityJeep.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityMotorcycle)
                    {
                        EntityMotorcycle entityMotorcycle = (EntityMotorcycle)entity;

                        if (entityMotorcycle.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityMotorcycle.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityMotorcycle.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityMotorcycle.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Motorcycle: " + entityMotorcycle.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityMotorcycle.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Motorcycle " + entityMotorcycle.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityVGyroCopter)
                    {
                        EntityVGyroCopter entityGyrocopter = (EntityVGyroCopter)entity;

                        if (entityGyrocopter.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityGyrocopter.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityGyrocopter.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityGyrocopter.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Gyrocopter: " + entityGyrocopter.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityGyrocopter.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Gyrocopter " + entityGyrocopter.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }

                    if (entity is EntityBicycle)
                    {
                        EntityBicycle entityBicycle = (EntityBicycle)entity;

                        if (entityBicycle.entityId == bikeID)
                        {
                            PersistentPlayerData playerDataCaller = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(_senderInfo.RemoteClientInfo.entityId);

                            if (!entityBicycle.IsOwner(playerDataCaller.PlayerData.PrimaryId))
                            {
                                //find the steamID of current owner
                                PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(entityBicycle.GetOwner());

                                if (playerDataFromEntityID != null)
                                {
                                    prevOwner = playerDataFromEntityID.PlayerData.PrimaryId;
                                }

                                entityBicycle.SetOwner(playerDataCaller.PlayerData.PrimaryId);

                                SdtdConsole.Instance.Output("You now own Bicycle: " + entityBicycle.entityId + " After moving away from this area and come back.");
                            }
                            else
                            {
                                if (prevOwner != null)
                                {
                                    entityBicycle.SetOwner(prevOwner);
                                    SdtdConsole.Instance.Output("Bicycle " + entityBicycle.entityId + " Given back to: " + prevOwner);
                                    prevOwner = null;
                                }
                            }
                            vehicleFound = true;
                            break;
                        }
                    }
                }
                if (!vehicleFound) SdtdConsole.Instance.Output("ERR: Vehicle not found!");
            }
            catch (Exception e)
            {
                Log.Out("Error in OwnVehicle.Run: " + e);
            }

        }
    }
}
