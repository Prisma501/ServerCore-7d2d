using System;

namespace ServerCore
{
    class Hostiles
    {
        public static bool Exec(ClientInfo _cInfo)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= ServerCoreSettings.Instance.ChatCommandPermissions_hostiles)
                {

                    int hostiles = 0;
                    bool feral = false;
                    bool feralradiated = false;
                    bool cop = false;
                    bool dog = false;
                    bool zbear = false;
                    bool bear = false;
                    bool wolf = false;
                    bool direwolf = false;
                    bool snake = false;
                    bool vulture = false;
                    string PMmsg = "";

                    EntityPlayer player = GameManager.Instance.World.Players.dict[_cInfo.entityId];

                    for (int index = 0; index < GameManager.Instance.World.Entities.list.Count; ++index)
                    {
                        EntityAlive entityAlive = GameManager.Instance.World.Entities.list[index] as EntityAlive;
                        if (entityAlive != null && entityAlive.IsAlive() && EntityClass.list[entityAlive.entityClass].bIsEnemyEntity)
                        {

                            Vector3i hostilePos = new Vector3i(entityAlive.GetPosition());
                            int distanceX = 0;
                            int distanceY = 0;
                            int distanceZ = 0;

                            distanceX = Math.Abs(player.GetBlockPosition().x - hostilePos.x);
                            distanceY = Math.Abs(player.GetBlockPosition().y - hostilePos.y);
                            distanceZ = Math.Abs(player.GetBlockPosition().z - hostilePos.z);

                            if (distanceX < 50 && distanceY < 50 && distanceZ < 50)
                            {
                                hostiles++;
                                if (entityAlive.LocalizedEntityName.ToUpper().Contains("FERAL") && !entityAlive.LocalizedEntityName.ToUpper().Contains("RADIATED")) feral = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Contains("FERALRADIATED")) feralradiated = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ZOMBIEFATCOP")) cop = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALZOMBIEDOG")) dog = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALZOMBIEBEAR")) zbear = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALBEAR")) bear = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALWOLF")) wolf = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALDIREWOLF")) direwolf = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALSNAKE")) snake = true;
                                if (entityAlive.LocalizedEntityName.ToUpper().Equals("ANIMALVULTURE")) vulture = true;
                            }
                        }
                    }

                    if (hostiles == 0)
                    {
                        PMmsg = ServerCoreStrings.Instance.Hostiles_NoHostiles;
                    }
                    else if (hostiles == 1)
                    {
                        PMmsg = ServerCoreStrings.Instance.Hostiles_OneHostile;
                        if (feralradiated) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_FerRad;
                        else if (feral) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Feral;
                        else if (cop) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Cop;
                        else if (dog) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Dog;
                        else if (zbear) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Zbear;
                        else if (wolf) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Wolf;
                        else if (direwolf) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_DireWolf;
                        else if (snake) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Snake;
                        else if (vulture) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Vulture;
                        else if (bear) PMmsg += ServerCoreStrings.Instance.Hostiles_OneHostile_Bear;
                    }
                    else
                    {
                        PMmsg = ServerCoreStrings.Instance.Hostiles_MoreHostiles.Replace("{hostileCount}", hostiles.ToString());
                        if (feral || cop || dog || zbear || wolf || direwolf || bear || vulture || snake || feralradiated) PMmsg += ServerCoreStrings.Instance.Hostiles_Including;
                        if (dog) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Dog;
                        if (wolf) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Wolf;
                        if (direwolf) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_DireWolf;
                        if (cop) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Cop;
                        if (feral) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Feral;
                        if (feralradiated) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_FerRad;
                        if (zbear) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Zbear;
                        if (snake) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Snake;
                        if (vulture) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Vulture;
                        if (bear) PMmsg += ServerCoreStrings.Instance.Hostiles_Including_Bear;

                        if (PMmsg.EndsWith(", ")) PMmsg = PMmsg.Substring(0, PMmsg.Length - 2);
                    }

                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                    Log.Out(PMmsg);
                }
                else
                {
                    string errMsg = ServerCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(ServerCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch { return false; }
            return true;
        }
    }
}
