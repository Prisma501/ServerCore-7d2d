using PrismaCore.JSON;
using System.Collections.Generic;
using System.Net;

namespace PrismaCore.Web.API
{
    public class GetAdvClaims : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {
            string claimType = string.Empty;
            if (_req.QueryString["type"] != null)
            {
                claimType = _req.QueryString["type"];
            }

            if (claimType.Equals(string.Empty))
            {
                _resp.StatusCode = (int)HttpStatusCode.BadRequest;
                Web.SetResponseTextContent(_resp, "type is a required parameter");
                return;
            }

            switch (claimType.ToLower())
            {
                case "hostilefree":
                    JsonAdvancedClaims(_resp, "hostilefree");
                    break;
                case "notify":
                    JsonAdvancedClaims(_resp, "notify");
                    break;
                case "command":
                    JsonAdvancedClaims(_resp, "command");
                    break;
                case "leveled":
                    JsonAdvancedClaims(_resp, "leveled");
                    break;
                case "reversed":
                    JsonAdvancedClaims(_resp, "reversed");
                    break;
                case "normal":
                    JsonAdvancedClaims(_resp, "normal");
                    break;
                case "timed":
                    JsonAdvancedClaims(_resp, "timed");
                    break;
                case "portal":
                    JsonAdvancedClaims(_resp, "portal");
                    break;
                case "openhours":
                    JsonAdvancedClaims(_resp, "openhours");
                    break;
                case "playerlevel":
                    JsonAdvancedClaims(_resp, "playerlevel");
                    break;
                case "lcbfree":
                    JsonAdvancedClaims(_resp, "lcbfree");
                    break;
                case "antiblock":
                    JsonAdvancedClaims(_resp, "antiblock");
                    break;
                case "reset":
                    JsonAdvancedClaims(_resp, "reset");
                    break;
                case "problock":
                    JsonAdvancedClaims(_resp, "problock");
                    break;
                case "landclaim":
                    JsonAdvancedClaims(_resp, "landclaim");
                    break;
                default:
                    _resp.StatusCode = (int)HttpStatusCode.BadRequest;
                    Web.SetResponseTextContent(_resp, "no valid type found");
                    return;

            }
        }

        private void JsonAdvancedClaims(HttpListenerResponse resp, string type)
        {
            JSONArray advClaims = new JSONArray();

            List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

            foreach (DbClaim activeClaim in lstClaims)
            {
                if (activeClaim == null) continue;

                if (!activeClaim.Id.StartsWith("r.") && !activeClaim.Id.EndsWith(".7rg"))
                {
                    if (string.IsNullOrEmpty(activeClaim.Type) && type.Equals("normal"))
                    {
                        JSONObject pJson = new JSONObject();
                        pJson.Add("Name", new JSONString(activeClaim.Id));
                        pJson.Add("W", new JSONNumber(activeClaim.W_bound));
                        pJson.Add("E", new JSONNumber(activeClaim.E_bound));
                        pJson.Add("N", new JSONNumber(activeClaim.N_bound));
                        pJson.Add("S", new JSONNumber(activeClaim.S_bound));
                        pJson.Add("Type", new JSONString(activeClaim.Type));

                        advClaims.Add(pJson);

                        continue;
                    }

                    if (!string.IsNullOrEmpty(activeClaim.Type) && activeClaim.Type.ToLower().Contains(type))
                    {
                        JSONObject pJson = new JSONObject();
                        pJson.Add("Name", new JSONString(activeClaim.Id));
                        pJson.Add("W", new JSONNumber(activeClaim.W_bound));
                        pJson.Add("E", new JSONNumber(activeClaim.E_bound));
                        pJson.Add("N", new JSONNumber(activeClaim.N_bound));
                        pJson.Add("S", new JSONNumber(activeClaim.S_bound));
                        pJson.Add("Type", new JSONString(activeClaim.Type));

                        advClaims.Add(pJson);
                    }
                }
            }

            WriteJSON(resp, advClaims);
        }
    }
}