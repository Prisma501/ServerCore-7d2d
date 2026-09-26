using PrismaCore.JSON;
using System.Collections.Generic;
using System.Net;

namespace PrismaCore.Web.API
{
    public class GetPlayerHomes : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            List<DbPlayer> lstDbPlayers = Database.Instance.GetAllDbPlayers();

            JSONObject result = new JSONObject();
            result.Add("homesize", new JSONNumber(GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BedrollDeadZoneSize"))));

            JSONArray claimOwners = new JSONArray();
            result.Add("homeowners", claimOwners);

            foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
            {
                //bedroll too
                if (keyValuePair.Value.HasBedrollPos)
                {
                    bool active = false;
                    double bedrollexpireTime = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;

                    if (keyValuePair.Value.OfflineHours < bedrollexpireTime)
                    {
                        active = true;
                    }

                    Vector3i bedrollPos = keyValuePair.Value.BedrollPos;
                    JSONObject owner = new JSONObject();
                    claimOwners.Add(owner);

                    string steamId = string.Empty;
                    string name = string.Empty;
                    string nameID = string.Empty;

                    DbPlayer dbPlayer = lstDbPlayers.Find((DbPlayer e) => e.EOS_Id.Equals(keyValuePair.Value.PlayerData.PrimaryId.ToString()));
                    if (dbPlayer != null)
                    {
                        steamId = dbPlayer.Id;
                        name = dbPlayer.Name;
                        nameID = $"{name} - {steamId}";
                    }

                    owner.Add("steamid", new JSONString(nameID));
                    owner.Add("eos_id", new JSONString(keyValuePair.Value.PlayerData.PrimaryId.ToString()));
                    owner.Add("active", new JSONBoolean(active));
                    owner.Add("x", new JSONNumber(bedrollPos.x));
                    owner.Add("y", new JSONNumber(bedrollPos.y));
                    owner.Add("z", new JSONNumber(bedrollPos.z));
                }
            }

            WriteJSON(_resp, result);
        }
    }
}