using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class MarkResetRegion : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Manage the reset regions list.";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   " mrr\n" +
                   " mrr add [<regionName>]\n" +
                   " mrr add <w_boundary> <e_boundary> <n_boundary> <s_boundary>\n" +
                   " mrr remove [<regionName>]\n" +
                   " mrr remove <w_boundary> <e_boundary> <n_boundary> <s_boundary>\n" +
                   " mrr list\n" +
                   " mrr notificationtext <enter:exit>\n";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-mrr", "mrr", "markresetregion" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                //check for 2 parameters and add/remove and allow remote execution
                if (_params.Count == 2 || _params.Count == 5)
                {
                    if (_params[0].EqualsCaseInsensitive("add"))
                    {
                        if (_params.Count == 2)
                        {
                            if (_params[1].Trim().ToLower().StartsWith("r.") && (_params[1].Trim().ToLower().EndsWith(".7rg")))
                            {
                                string rgName = _params[1].Trim().ToLower();
                                if (RegionReset.lstRegions.Contains(rgName))
                                {
                                    SdtdConsole.Instance.Output("ERR: The region you are trying to add is already on the reset list!");
                                    return;
                                }
                                else
                                {
                                    RegionReset.lstRegions.Add(rgName);
                                    RegionReset.SaveRegionsList();
                                    SdtdConsole.Instance.Output(string.Format("The region {0} has been added to the region reset list.", rgName));
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: Invalid regionName given.");
                                return;
                            }
                        }
                        else
                        {
                            //add by area
                            if (!int.TryParse(_params[1], out int W))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for W_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[2], out int E))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for E_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[3], out int N))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for N_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[4], out int S))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for S_boundary is not a valid integer!");
                                return;
                            }

                            GetRegionsFromArea(W, E, N, S, "add");
                            return;
                        }
                    }
                    else if (_params[0].EqualsCaseInsensitive("remove"))
                    {
                        if (_params.Count == 2)
                        {
                            if (_params[1].Trim().ToLower().StartsWith("r.") && (_params[1].Trim().ToLower().EndsWith(".7rg")))
                            {

                                string rgName = _params[1].Trim().ToLower();
                                if (RegionReset.lstRegions.Contains(rgName))
                                {
                                    RegionReset.lstRegions.Remove(rgName);
                                    RegionReset.SaveRegionsList();
                                    SdtdConsole.Instance.Output(string.Format("The region {0} has been removed from the region reset list.", rgName));
                                    return;
                                }
                                else
                                {
                                    SdtdConsole.Instance.Output("ERR: The region you are trying to delete is not on the reset list!");
                                    return;
                                }
                            }
                            else
                            {
                                SdtdConsole.Instance.Output("ERR: Invalid regionName given.");
                                return;
                            }
                        }
                        else
                        {
                            //remove by area
                            if (!int.TryParse(_params[1], out int W))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for W_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[2], out int E))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for E_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[3], out int N))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for N_boundary is not a valid integer!");
                                return;
                            }
                            if (!int.TryParse(_params[4], out int S))
                            {
                                SdtdConsole.Instance.Output("ERR: The parameter for S_boundary is not a valid integer!");
                                return;
                            }

                            GetRegionsFromArea(W, E, N, S, "remove");
                            return;
                        }
                    }
                }

                if (_params.Count != 0 && _params.Count != 1 && _params.Count != 2)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid number of parameters given. Expected 0, 1 or 2. Found: " + _params.Count);
                    return;
                }

                if (_params.Count == 1)
                {
                    if (_params[0].EqualsCaseInsensitive("add"))
                    {
                        ClientInfo ci = _senderInfo.RemoteClientInfo;
                        if (ci == null)
                        {
                            string errMsg = "ERR: Command can only be used ingame!";
                            SdtdConsole.Instance.Output(errMsg);
                            return;
                        }

                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                        Vector3i pos = new Vector3i();
                        pos = ep.GetBlockPosition();
                        string rgFile = GetRegion(pos);
                        if (RegionReset.lstRegions.Contains(rgFile))
                        {
                            SdtdConsole.Instance.Output("ERR: The region you are trying to add is already on the reset list!");
                            return;
                        }
                        else
                        {
                            RegionReset.lstRegions.Add(rgFile);
                            RegionReset.SaveRegionsList();
                            SdtdConsole.Instance.Output(string.Format("The region {0} has been added to the region reset list.", rgFile));
                            return;
                        }
                    }
                    else if (_params[0].EqualsCaseInsensitive("remove"))
                    {
                        ClientInfo ci = _senderInfo.RemoteClientInfo;
                        if (ci == null)
                        {
                            string errMsg = "ERR: Command can only be used ingame!";
                            SdtdConsole.Instance.Output(errMsg);
                            return;
                        }

                        EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                        Vector3i pos = new Vector3i();
                        pos = ep.GetBlockPosition();
                        string rgFile = GetRegion(pos);
                        if (RegionReset.lstRegions.Contains(rgFile))
                        {
                            RegionReset.lstRegions.Remove(rgFile);
                            RegionReset.SaveRegionsList();
                            SdtdConsole.Instance.Output(string.Format("The region {0} has been removed from the region reset list.", rgFile));
                            return;
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("ERR: The region you are trying to delete is not on the reset list!");
                            return;
                        }
                    }
                    else if (_params[0].EqualsCaseInsensitive("list"))
                    {
                        SdtdConsole.Instance.Output("List of regions marked for reset:");
                        foreach (string rg in RegionReset.lstRegions)
                        {
                            SdtdConsole.Instance.Output(rg);
                        }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("Invalid subcommand given.");
                        return;
                    }
                }
                else if (_params.Count == 2)
                {
                    if (_params[0].EqualsCaseInsensitive("notificationtext"))
                    {
                        PrismaCoreStrings.Instance.Resetregion_EnterNotification = _params[1].Trim().Split(':')[0];
                        PrismaCoreStrings.Instance.Resetregion_ExitNotification = _params[1].Trim().Split(':')[1];
                        PrismaCoreStrings.Instance.Save();
                        SdtdConsole.Instance.Output("Notification text (enter:exit) has been set to: " + _params[1]);
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("ERR: Invalid subcommand given.");
                        return;
                    }
                }
                else
                {
                    //display region file and resetregion sentence
                    ClientInfo ci = _senderInfo.RemoteClientInfo;
                    if (ci == null)
                    {
                        string errMsg = "ERR: Command can only be used ingame!";
                        SdtdConsole.Instance.Output(errMsg);
                        return;
                    }

                    EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
                    Vector3i pos = new Vector3i();
                    pos = ep.GetBlockPosition();

                    string notifyText = $"{PrismaCoreStrings.Instance.Resetregion_EnterNotification}:{PrismaCoreStrings.Instance.Resetregion_ExitNotification}";

                    SdtdConsole.Instance.Output("You are standing on region: " + GetRegion(pos));
                    SdtdConsole.Instance.Output("Notification text (enter:exit): " + notifyText);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in MarkResetRegion.Run: {0}.", e));
            }
        }

        private string GetRegion(Vector3i coords)
        {
            int rgLat = Math.DivRem(coords.x, 512, out int remainderLat);
            int rgLng = Math.DivRem(coords.z, 512, out int remainderLng);

            if (coords.x < 0) rgLat -= 1;
            if (coords.z < 0) rgLng -= 1;

            string regionFile = string.Format("r.{0}.{1}.7rg", rgLat.ToString(), rgLng.ToString());
            return regionFile;
        }

        private void GetRegionsFromArea(int W, int E, int N, int S, string operation)
        {
            if (E < W || N < S)
                return;

            List<string> regions = new List<string>();

            int rgLatW = Math.DivRem(W, 512, out int remainderLatW);
            int rgLatE = Math.DivRem(E, 512, out int remainderLatE);
            int rgLngN = Math.DivRem(N, 512, out int remainderLngN);
            int rgLngS = Math.DivRem(S, 512, out int remainderLngS);

            if (W < 0) rgLatW -= 1;
            if (E < 0) rgLatE -= 1;
            if (N < 0) rgLngN -= 1;
            if (S < 0) rgLngS -= 1;

            for (int i = rgLatE; i >= rgLatW; i--)
            {
                for (int j = rgLngN; j >= rgLngS; j--)
                {
                    string regionFileName = string.Format("r.{0}.{1}.7rg", i, j);
                    if (!regions.Contains(regionFileName))
                        regions.Add(regionFileName);
                }
            }

            //add or remove
            foreach (string s in regions)
            {
                if (operation.Equals("add"))
                {
                    if (!RegionReset.lstRegions.Contains(s))
                    {
                        RegionReset.lstRegions.Add(s);
                        SdtdConsole.Instance.Output(string.Format("The region {0} has been added to the region reset list.", s));
                    }
                }
                else
                {
                    if (RegionReset.lstRegions.Contains(s))
                    {
                        RegionReset.lstRegions.Remove(s);
                        SdtdConsole.Instance.Output(string.Format("The region {0} has been removed from the region reset list.", s));
                    }
                }
            }
            RegionReset.SaveRegionsList();
            SdtdConsole.Instance.Output(regions.Count.ToString() + " regions handled.");
        }
    }
}
