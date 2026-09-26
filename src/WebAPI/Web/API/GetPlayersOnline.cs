using ServerCore.JSON;
using System.Collections.Generic;
using System.Net;

namespace ServerCore.Web.API
{
    public class GetPlayersOnline : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user,
            int _permissionLevel)
        {

            JSONArray players = new JSONArray();

            World w = GameManager.Instance.World;
            foreach (KeyValuePair<int, EntityPlayer> current in w.Players.dict)
            {
                ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(current.Key);

                JSONObject pos = new JSONObject();
                pos.Add("x", new JSONNumber((int)current.Value.GetPosition().x));
                pos.Add("y", new JSONNumber((int)current.Value.GetPosition().y));
                pos.Add("z", new JSONNumber((int)current.Value.GetPosition().z));

                JSONObject p = new JSONObject();
                p.Add("steamid", new JSONString(ci.PlatformId.ToString()));
                p.Add("name", new JSONString(current.Value.EntityName));
                p.Add("position", pos);

                players.Add(p);
            }

            WriteJSON(_resp, players);
        }
    }
}