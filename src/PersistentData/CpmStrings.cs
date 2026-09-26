using JetBrains.Annotations;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PrismaCore
{
    [Serializable]
    [XmlRoot("PrismaCoreStrings")]
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public class PrismaCoreStrings
    {
        private static PrismaCoreStrings _instance;
        public static PrismaCoreStrings Instance => _instance ?? (_instance = new PrismaCoreStrings());

        public const string SaveFileName = "PrismaCoreStrings.xml";
        
        #region Persistent values to be saved
        //AdvClaims
        public string AdvClaims_Reversed = "[F7FE2E]You are not allowed to leave this area![-]";
        public string AdvClaims_Normal = "[F7FE2E]Advanced claim intrusion: This area is claimed and you may not enter.[-]";
        public string AdvClaims_PlayerLevel = "[F7FE2E]Advanced claim intrusion: Your level is not {logic} and you may not enter.[-]";
        public string AdvClaims_OpenTime = "[F7FE2E]Its open from {openTime}[-]";
        public string AdvClaims_LcbFree = "[F7FE2E]You are in a LCB free adv. claimed area. You are not allowed to place a LCB.[-]";
        public string AdvClaims_OnVehicleWarning = "[F7FE2E]You entered an advanced claimed area. Do NOT leave/step of your vehicle or LOSE it! Move away from this base/area!![-]";
        public string AdvClaims_AntiBlock = "[F7FE2E]You are not allowed to place {blockName} here![-]";
        public string AdvClaims_Reset_KickMessage = "Server is resetting (parts of) the world. This may take a while.";
        public string AdvClaims_ProBlock = "[F7FE2E]You are only allowed to place block(s) {proBlocks} here![-]";
        public string AdvClaims_Landclaim = "[F7FE2E]Claimed area. You are not allowed to place any blocks here![-]";

        //block forbidden chars in name
        public string SpecialCharactersNameBlock_KickMessage = "You have forbidden characters in your playername. {forbiddenChars} are not allowed.";

        //arrest
        public string Arrest_Notification = "[00FF00]You have been jailed by an Admin![-]";
        public string Arrest_Notification_Timed = "[00FF00]You have been jailed by an Admin! You will be released automatically in {minutes} minutes[-]";
        public string Arrest_AutoReleaseMsg = "[00FF00]You have been released from jail automatically![-]";

        //BlockUTF8Names
        public string BlockUTF8Names_KickMessage = "Only ASCII characters allowed (a-z, A-Z, 0-9) in playername on this server.";

        //ChatCommandPermissions
        public string ChatCommandPermissions_NotAllowedMessage = "[F7FE2E]You are not allowed to run this command.[-]";

        //Chatcommands
        public string Acf_NoAdvancedClaim = "[F7FE2E]You have no advanced claim![-]";
        public string Acf_PlayerNotFound = "[F7FE2E]The player cannot be found![-]";
        public string Acf_PlayerAllreadyWhitelisted = "[F7FE2E]Player is already whitelisted for your claim(s).[-]";
        public string Acf_WhitelistSuccess = "[F7FE2E]{playerName} has been whitelisted for your claim(s).[-]";
        public string Acf_Error = "[FFA07A]Something went wrong! Could not add player to claim whitelist.[-]";

        public string Bag_PositionNotFound = "[F7FE2E]Your last dropped backpack position cannot be found.[-]";
        public string Bag_Error = "[FFA07A]Something went wrong! I was not able to tp you to your backpack.[-]";

        public string Bed_NoBed = "[F7FE2E]No active bed can be found![-]";
        public string Bed_Error = "[FFA07A]Something went wrong! I was not able to tp you to your bed.[-]";

        public string Aaf_Raf_NoAdancedClaim = "[F7FE2E]You have no advanced claim![-]";
        public string Aaf_Raf_NoIngameFriends = "[F7FE2E]You dont have any ingame friends to add/remove from whitelist![-]";
        public string Aaf_AddSucces = "[F7FE2E]Player {playerName} has been added to the whitelist of claim {claimName}[-]";
        public string Raf_RemoveSucces = "[F7FE2E]Player {playerName} has been removed from the whitelist of claim {claimName}[-]";
        public string Aaf_Error = "[FFA07A]Something went wrong! I was not able to add your friends to claim whitelist.[-]";
        public string Raf_Error = "[FFA07A]Something went wrong! I was not able to remove your friends from claim whitelist.[-]";

        public string Day7_BloodmoonWarningB4Four = "[40FF00]Bloodmoon is [FF0000]TONIGHT at 22:00[-] !!![-]";
        public string Day7_BloodmoonWarningAfterFour = "[40FF00]Bloodmoon is [FF0000]TONIGHT[-] !!![-]";
        public string Day7_BloodmoonWarningDuring = "[40FF00]Bloodmoon is [FF0000]TONIGHT[-] !!!. Hang on until 04:00 ![-]";
        public string Day7_BloodmoonWarningDaysleft = "[40FF00]Bloodmoon is in {daysLeft} {textDay} !  ( Day {nextBloodmoonDay} )[-]";
        public string Day7_Day = "day";
        public string Day7_Days = "days";
        public string Day7_Stats = "[40FF00]Players: {players} Zombies: {zombies}[-]";
        public string Day7_Fps = "[40FF00]Server FPS: {fps}[-]";
        public string Day7_Random = "[40FF00]Bloodmoon is random![-]";
        public string Day7_Error = "[FFA07A]Something went wrong! I was not able to get info on day7 hordenight.[-]";

        public string Delwp_RemoveSuccess = "[F7FE2E]Waypoint with name={nameWaypoint} has been removed.[-]";
        public string Delwp_Error = "[FFA07A]Something went wrong! I was not able to delete the waypoint.[-]";

        public string DroneDupePrevention = "[F7FE2E]You are NOT allowed to teleport with a deployed drone![-]";

        public string Ft_TargetOffline = "[F7FE2E]{playerName} is offline now. You have been teleported to last known position.[-]";
        public string Ft_NoParameters = "[FFA07A]Really!? FLy to...eeh...well...space???[-]";
        public string Ft_Error = "[FFA07A]Something went wrong! I think your wings are broken.[-]";

        public string Ftw_Error = "[FFA07A]Something went wrong! You were not able to fly to waypoint.[-]";

        public string Get_PlayerOffline = "[F7FE2E]{playerName} is not online now![-]";
        public string Get_PlayerNotFound = "[F7FE2E]Player can not be found![-]";
        public string Get_MoveSuccess = "[F7FE2E]Succesfully moved {namePlayer} to you.[-]";
        public string Get_InvalidParameters = "[FFA07A]Invalid number of parameters given.[-]";
        public string Get_Error = "[FFA07A]Something went wrong! Could not get the player.[-]";

        public string Hostiles_NoHostiles = "[40FF00]No hostiles detected within 50 meters of your location, [FE2E2E]YET![-]. But they are coming...I can feel it...[-]";
        public string Hostiles_OneHostile = "[FE2E2E]Be carefull! There is 1 hostile detected within 50 meters of your location.[-]";
        public string Hostiles_OneHostile_FerRad = "[FE2E2E] Its a feralradiated zombie![-]";
        public string Hostiles_OneHostile_Feral = "[FE2E2E] Its a feral zombie![-]";
        public string Hostiles_OneHostile_Cop = "[FE2E2E] Its a fat zombie cop![-]";
        public string Hostiles_OneHostile_Dog = "[FE2E2E] Its a dog![-]";
        public string Hostiles_OneHostile_Zbear = "[FE2E2E] Its a zombie bear![-]";
        public string Hostiles_OneHostile_Wolf = "[FE2E2E] Its a wolf![-]";
        public string Hostiles_OneHostile_DireWolf = "[FE2E2E] Its a dire wolf![-]";
        public string Hostiles_OneHostile_Snake = "[FE2E2E] Its a snake![-]";
        public string Hostiles_OneHostile_Vulture = "[FE2E2E] Its a vulture![-]";
        public string Hostiles_OneHostile_Bear = "[FE2E2E] Its a bear![-]";
        public string Hostiles_MoreHostiles = "[FE2E2E]Be carefull! There are {hostileCount} hostiles detected within 50 meters of your location. [-]";
        public string Hostiles_Including = "[FE2E2E]Including: [-]";
        public string Hostiles_Including_Dog = "[FE2E2E]dog(s), [-]";
        public string Hostiles_Including_Wolf = "[FE2E2E]wolf(s), [-]";
        public string Hostiles_Including_DireWolf = "[FE2E2E]dire wolf(s), [-]";
        public string Hostiles_Including_Cop = "[FE2E2E]fat zombie Cop(s), [-]";
        public string Hostiles_Including_Feral = "[FE2E2E]feral zombie(s), ";
        public string Hostiles_Including_FerRad = "[FE2E2E]feralradiated zombie(s), [-]";
        public string Hostiles_Including_Zbear = "[FE2E2E]zombie bear(s), [-]";
        public string Hostiles_Including_Snake = "[FE2E2E]snake(s), [-]";
        public string Hostiles_Including_Vulture = "[FE2E2E]vulture(s), [-]";
        public string Hostiles_Including_Bear = "[FE2E2E]bears(s), [-]";
        public string Hostiles_Error = "[FFA07A]Something went wrong! I was not able to do detection on hostiles.[-]";

        public string Lcf_NoAdvancedClaim = "[F7FE2E]You have no advanced claim![-]";
        public string Lcf_EmptyWhitelist = "[F7FE2E]You have no whitelisted friends for your claim.[-]";
        public string Lcf_Error = "[FFA07A]Something went wrong! Could not list claim whitelist.[-]";

        public string Listwp_NoWaypoints = "[F7FE2E]There are no waypoints available.[-]";
        public string Listwp_ListTitle = "[F7FE2E]Available waypoints:[-]";
        public string Listwp_Error = "[FFA07A]Something went wrong! I was not able get the waypoints.[-]";

        public string Mv_PlayerOffline = "[F7FE2E]Player is offline and will be teleported on next spawn.[-]";
        public string Mv_TargetOffline = "[F7FE2E]Succesfully moved {movingPlayer} to {targetPlayer} (offline).[-]";
        public string Mv_SuccesPlayer = "[F7FE2E]Succesfully moved {movingPlayer} to {targetPlayer}.[-]";
        public string Mv_SuccesCoords = "[F7FE2E]Succesfully moved {movingPlayer} to {coords}.[-]";
        public string Mv_Error = "[FFA07A]Something went wrong! Could not perform the move.[-]";

        public string Mvw_PlayerOffline = "[F7FE2E]Player is offline and will be teleported to waypoint on next spawn.[-]";
        public string Mvw_Success = "[F7FE2E]Succesfully moved {movingPlayer} to waypoint {waypointName}.[-]";
        public string Mvw_Error = "[FFA07A]Something went wrong! I was not able to move the player to waypoint.[-]";

        public string Rcf_NoAdvancedClaim = "[F7FE2E]You have no advanced claim![-]";
        public string Rcf_PlayerNotFound = "[F7FE2E]The player cannot be found![-]";
        public string Rcf_NotInWhitelist = "[F7FE2E]Player is not in your whitelist.[-]";
        public string Rcf_Success = "[F7FE2E]{playerName} has been removed from your whitelist.[-]";
        public string Rcf_Error = "[FFA07A]Something went wrong! Could not remove player from claim whitelist.[-]";

        //ResetPrefabs
        public string ResetPrefabs_KickMessage = "RWG prefabs are being reset. This can take a few minutes.";

        public string Ls_OnlineNow = "[F7FE2E]{playerName} is on server now![-]";
        public string Ls_Success = "[F7FE2E]{playerName} was last online: {time} ago.[-]";
        public string Ls_NoParameters = "[FFA07A]Check for who???[-]";
        public string Ls_Error = "[FFA07A]Something went wrong! I cant check players last online time.[-]";

        public string Setwp_Success = "[F7FE2E]Waypoint with name={waypointName} added with coordinates {coords}.[-]";
        public string Setwp_Error = "[FFA07A]Something went wrong! I was not able to set the waypoint.[-]";

        //Chat
        public string Chat_Muted = "Your chat is muted!";
        public string Chat_MaxLength = "Your message was too long. So we blocked it!";

        //DonorSlots
        public string DonorSlots_WelcomeDonor = "[ffff4d]Welcome [4da6ff]{playerName}[-], you are connecting to your donor slot. Your donor slot expires on [ff8533]{endOfDonorship}[-][-]";
        public string DonorSlots_WelcomeDonorExpired = "[ffff4d]Welcome [4da6ff]{playerName}[-]. Your donor slot has expired on [ff8533]{endOfDonorship}[-][-]";
        public string DonorSlots_Kick = "Sorry to be rude {playerName}, but there are no spaces left. Maximum players on server is {maxPlayers}. Remaining slots are reserved for donors and admins. Please try again when there are {maxMinusOne} or less players connected.";
        public string DonorSlots_KickDonorExpired = "Sorry {playerName}, but there are no spaces left. Your donor slot has expired on {endOfDonorship}.";

        //NighttimeAnnouncer
        public string NighttimeAnnouncer_AnnouncerName = "[B43104]{Server-Auto}[-]";
        public string NighttimeAnnouncer_NightTimeText = "[00ff00]Nighttime in {hours} hours !!![-]";
        public string NighttimeAnnouncer_BloodDayText = "[00ff00]Better hurry! Its bloodmoon [FF0000]TONIGHT[-] !!![-]";
        public string NighttimeAnnouncer_BloodDayTomorrowText = "[00ff00]Remember: Bloodmoon is tomorrow !!![-]";
        public string NighttimeAnnouncer_CounterText = "[00ff00]Remember: Bloodmoon is in {daysleft} days !!![-]";
        public string NighttimeAnnouncer_RandomBloodmoon = "[40FF00]Bloodmoon is random![-]";

        //Permadeath
        public string Permadeath_Kickmessage = "You died and are playing permadeath. Your character has been reset. Better luck next round!";

        //Offline_Teleport
        public string Offline_Teleport = "[F7FE2E]You have been teleported by an admin while offline.[-]";

        //Quest POI Protection
        public string QuestPoiProtection_LcbMessage = "[F7FE2E]Claim area overlaps a quest POI! You are not allowed to place a LCB here![-]";
        public string QuestPoiProtection_BedMessage = "[F7FE2E]Bed(roll) deadzone overlaps a quest POI! You are not allowed to place a bed(roll) here! Bed(roll) deactivated.[-]";

        //All POI Protection
        public string AllPoiProtection_LcbMessage = "[F7FE2E]Claim area overlaps a POI! You are not allowed to place a LCB here![-]";
        public string AllPoiProtection_BedMessage = "[F7FE2E]Bed(roll) deadzone overlaps a POI! You are not allowed to place a bed(roll) here! Bed(roll) deactivated.[-]";

        //ResetRegion
        public string Resetregion_EnterNotification = "[00FF00]You are entering a reset area! Do [FF0000]NOT[-] build here![-]";
        public string Resetregion_ExitNotification = "[00FF00]You are leaving a reset area![-]";
        public string Resetregion_LCB2Close = "[00FF00]You have placed a LCB too close to a reset region! For preventing half base wipes, the LCB has been put back in your inventory.[-]";
        public string Resetregion_LCBinRegion = "[00FF00]You have placed a LCB in a reset region! The LCB has been put back in your inventory.[-]";

        //ServerChatName
        public string ServerChatName = "Server";

        //ShutdownBA
        public string ShutdownBA_CountdownMessage = "[00FF00]Server will restart in [FF0000]{Minutes} MINUTES[-]";
        public string ShutdownBA_RestartDelayedMessage = "[00FF00]Server restart delayed until after bloodmoon at {DelayUntil}[-]";

        #endregion

        public void Save()
        {
            lock (SaveFileName)
            {
                try
                {
                    var saveFilePath = $"{API.GamePath}/{SaveFileName}";
                    using (var writer = new XmlTextWriter(new StreamWriter(saveFilePath)))
                    {
                        writer.Formatting = Formatting.Indented;
                        var serializer = new XmlSerializer(this.GetType());
                        serializer.Serialize(writer, this);
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[PrismaCore] Could not save PrismaCore strings: " + ex);
                }
            }
        }

        public static void Load()
        {
            var saveFilePath = $"{API.GamePath}/{SaveFileName}";
            lock (SaveFileName)
            {
                if (!File.Exists(saveFilePath))
                {
                    Instance.Save();
                    return;
                }

                try
                {
                    using (var reader = new StreamReader(saveFilePath))
                    {
                        var serializer = new XmlSerializer(typeof(PrismaCoreStrings));
                        _instance = serializer.Deserialize(reader) as PrismaCoreStrings;
                    }
                    Log.Out($"[PrismaCore] PrismaCoreStrings loaded from {SaveFileName}.");
                }
                catch (Exception ex)
                {
                    Log.Error("[PrismaCore] Could not load PrismaCoreStrings: " + ex);
                }
            }
        }
    }
}
