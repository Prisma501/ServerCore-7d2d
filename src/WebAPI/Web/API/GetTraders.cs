using PrismaCore.JSON;
using System;
using System.Collections.Generic;
using System.Net;

namespace PrismaCore.Web.API
{
    public class GetTraders : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            JSONObject result = new JSONObject();

            JSONArray questPOIs = new JSONArray();
            result.Add("Traders", questPOIs);

            try
            {
                DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();

                if (dynamicPrefabDecorator != null)
                {
                    List<PrefabInstance> allPrefabs = new List<PrefabInstance>();
                    dynamicPrefabDecorator.GetWorldPrefabs(allPrefabs);
                    for (int i = 0; i < allPrefabs.Count; i++)
                    {
                        PrefabInstance fab = allPrefabs[i];
                        if (fab != null)
                        {
                            if (fab.name.ContainsCaseInsensitive("trader") && !fab.name.ContainsCaseInsensitive("tile_filler"))
                            {
                                Vector3i BoxMin = fab.boundingBoxPosition;
                                Vector3i BoxMax = fab.boundingBoxPosition + fab.boundingBoxSize;
                                var pcenter = fab.GetCenterXZ();

                                JSONObject questPOI = new JSONObject();
                                questPOIs.Add(questPOI);

                                questPOI.Add("name", new JSONString(fab.name));
                                questPOI.Add("minx", new JSONNumber(BoxMin.x));
                                questPOI.Add("minz", new JSONNumber(BoxMin.z));
                                questPOI.Add("maxx", new JSONNumber(BoxMax.x));
                                questPOI.Add("maxz", new JSONNumber(BoxMax.z));
                                questPOI.Add("x", new JSONNumber((int)Math.Floor(pcenter.x)));
                                questPOI.Add("z", new JSONNumber((int)Math.Floor(pcenter.y)));
                            }
                        }
                    }

                    WriteJSON(_resp, result);
                }
            }
            catch { }
        }
    }
}