using ServerCore.JSON;
using System.Collections.Generic;
using System.Net;

namespace ServerCore.Web.API
{
    public class GetLandClaims : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user,
            int _permissionLevel)
        {
            // default user, cheap way to avoid 'null reference exception'
            PlatformUserIdentifierAbs userId = _user?.UserId;

            List<DbPlayer> lstDbPlayers = Database.Instance.GetAllDbPlayers();

            JSONObject result = new JSONObject();
            result.Add("claimsize", new JSONNumber(GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("LandClaimSize"))));

            JSONArray claimOwners = new JSONArray();
            result.Add("claimowners", claimOwners);

            Dictionary<Vector3i, PersistentPlayerData> allLandClaims = GameManager.Instance.GetPersistentPlayerList().m_lpBlockMap;

            foreach (KeyValuePair<Vector3i, PersistentPlayerData> kvp in allLandClaims)
            {
                JSONObject owner = new JSONObject();
                claimOwners.Add(owner);
                owner.Add("eos_id", new JSONString(kvp.Value.PlayerData.PrimaryId.ToString()));

                //get steamidbyEOS
                string steamId = string.Empty;
                string playerName = string.Empty;

                DbPlayer dbPlayer = lstDbPlayers.Find((DbPlayer e) => e.EOS_Id.Equals(kvp.Value.PlayerData.PrimaryId.ToString()));
                if (dbPlayer != null)
                {
                    steamId = dbPlayer.Id;
                    playerName = dbPlayer.Name;
                }

                owner.Add("steamid", new JSONString(steamId));
                owner.Add("claimactive", new JSONBoolean(GameManager.Instance.World.IsLandProtectionValidForPlayer(GameManager.Instance.GetPersistentPlayerList().GetPlayerData(kvp.Value.PlayerData.PrimaryId))));

                if (kvp.Value.PlayerName.playerName.ToString().Length > 0)
                {
                    owner.Add("playername", new JSONString(playerName));
                }
                else
                {
                    owner.Add("playername", new JSONNull());
                }

                JSONArray claimsJson = new JSONArray();
                owner.Add("claims", claimsJson);

                JSONObject claim = new JSONObject();
                claim.Add("x", new JSONNumber(kvp.Key.x));
                claim.Add("y", new JSONNumber(kvp.Key.y));
                claim.Add("z", new JSONNumber(kvp.Key.z));

                claimsJson.Add(claim);

            }

            WriteJSON(_resp, result);
        }
    }
}