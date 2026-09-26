using ServerCore.JSON;
using System.Collections.Generic;
using System.Net;

namespace ServerCore.Web.API
{
    public class GetResetRegions : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {
            JSONArray resetRegions = new JSONArray();

            List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

            foreach (DbClaim activeClaim in lstClaims)
            {
                if (activeClaim == null) continue;

                if (activeClaim.Id.StartsWith("r.") && activeClaim.Id.EndsWith(".7rg"))
                {
                    JSONObject pJson = new JSONObject();
                    pJson.Add("W", new JSONNumber(activeClaim.W_bound));
                    pJson.Add("E", new JSONNumber(activeClaim.E_bound));
                    pJson.Add("N", new JSONNumber(activeClaim.N_bound));
                    pJson.Add("S", new JSONNumber(activeClaim.S_bound));

                    resetRegions.Add(pJson);
                }
            }

            WriteJSON(_resp, resetRegions);
        }
    }
}