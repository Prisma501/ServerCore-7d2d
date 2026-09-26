using ServerCore.JSON;
using System;
using System.Collections.Generic;
using System.Net;

namespace ServerCore.Web.API
{
    public class GetQuestPOIs : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            bool bedLcbOnly = false;
            string filter = string.Empty;

            if (_req.QueryString["filter"] != null)
            {
                filter = _req.QueryString["filter"];
            }

            if (filter.ToLower().Equals("bedlcbonly"))
            {
                bedLcbOnly = true;
            }

            JSONObject result = new JSONObject();

            JSONArray questPOIs = new JSONArray();
            result.Add("QuestPOIs", questPOIs);

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
                            if (fab.prefab.HasQuestTag() && fab.prefab.DifficultyTier > 0)
                            {
                                //Quest Pois only!
                                bool containsBed = fab.CheckForAnyPlayerHome(GameManager.Instance.World) != GameUtils.EPlayerHomeType.None;

                                if (bedLcbOnly)
                                {
                                    if (containsBed)
                                    {
                                        Vector3i BoxMin = fab.boundingBoxPosition;
                                        Vector3i BoxMax = fab.boundingBoxPosition + fab.boundingBoxSize;
                                        var pcenter = fab.GetCenterXZ();

                                        JSONObject questPOI = new JSONObject();
                                        questPOIs.Add(questPOI);

                                        questPOI.Add("name", new JSONString(fab.name));
                                        questPOI.Add("containsbed", new JSONBoolean(containsBed));
                                        questPOI.Add("minx", new JSONNumber(BoxMin.x));
                                        questPOI.Add("minz", new JSONNumber(BoxMin.z));
                                        questPOI.Add("maxx", new JSONNumber(BoxMax.x));
                                        questPOI.Add("maxz", new JSONNumber(BoxMax.z));
                                        questPOI.Add("x", new JSONNumber((int)Math.Floor(pcenter.x)));
                                        questPOI.Add("z", new JSONNumber((int)Math.Floor(pcenter.y)));
                                    }
                                }
                                else
                                {
                                    Vector3i BoxMin = fab.boundingBoxPosition;
                                    Vector3i BoxMax = fab.boundingBoxPosition + fab.boundingBoxSize;
                                    var pcenter = fab.GetCenterXZ();

                                    JSONObject questPOI = new JSONObject();
                                    questPOIs.Add(questPOI);

                                    questPOI.Add("name", new JSONString(fab.name));
                                    questPOI.Add("containsbed", new JSONBoolean(containsBed));
                                    questPOI.Add("minx", new JSONNumber(BoxMin.x));
                                    questPOI.Add("minz", new JSONNumber(BoxMin.z));
                                    questPOI.Add("maxx", new JSONNumber(BoxMax.x));
                                    questPOI.Add("maxz", new JSONNumber(BoxMax.z));
                                    questPOI.Add("x", new JSONNumber((int)Math.Floor(pcenter.x)));
                                    questPOI.Add("z", new JSONNumber((int)Math.Floor(pcenter.y)));
                                }
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