using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class ClaimControl : ConsoleCmdAbstract
    {
        private static Dictionary<int, Vector3i> location = new Dictionary<int, Vector3i>();
        private static readonly string modPath = (Application.platform != RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../Mods") : (Application.dataPath + "/../../Mods");

        public override string[] getCommands()
        {
            return new[] { "pc-ccc", "ccc" };
        }

        public override string getDescription()
        {
            return "Manage advanced claims";
        }

        public override string getHelp()
        {
            return "Add/Remove/List/Configure advanced claims\n" +
                "Usage:\n" +
                "  1. ccc add <claimid/steamid> <w_boundary> <e_boundary> <n_boundary> <s_boundary> <accessLevel> [<type>]\n" +
                "  2. ccc remove <claimid/steamid>\n" +
                "  3. ccc list\n" +
                "  4. ccc wl add <claimid/steamid> <steamid>\n" +
                "  5. ccc wl remove <claimid/steamid> <steamid>\n" +
                "  6. ccc p1\n" +
                "  7. ccc p2 <claimid/steamid> <accessLevel> [<type>]\n" +
                "  8. ccc radius <radius> <steamId/entityId/Name> <claimid/steamid> <accessLevel> [<type>]\n" +
                "1. Add a claim. Use steamid of player in the name when using as playerclaim.\n" +
                "   For normal claims leave type empty. Possible types: reversed, leveled, timed, hostilefree, portal, openhours\n" +
                "2. Delete claim with name <claimid/steamid>\n" +
                "3. List all advanced claims\n" +
                "4. Add a steamid to the whitelist of claim <claimid/steamid>\n" +
                "5. Remove a steamid from the whitelist of claim <claimid/steamid>\n" +
                "6. Store your position for use with 7.\n" +
                "7. Add a claim with coordinates set by 6 and current position.\n" +
                "8. Add a claim with boundaries on <radius> distance from <steamId/entityId/Name> position.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count >= 1)
            {
                switch (_params[0].ToLower())
                {
                    case "add":
                        ExecuteAdd(_params);
                        break;
                    case "remove":
                        ExecuteRemove(_params);
                        break;
                    case "list":
                        ExecuteList();
                        break;
                    case "wl":
                        ExecuteWhiteList(_params);
                        break;
                    case "p1":
                        SavePosition(_senderInfo);
                        break;
                    case "p2":
                        ExecuteAddP2(_params, _senderInfo);
                        break;
                    case "radius":
                        ExecuteAddRadius(_params);
                        break;
                    default:
                        SdtdConsole.Instance.Output("ERR: Invalid sub command \"" + _params[0] + "\".");
                        return;
                }
            }
            else
            {
                SdtdConsole.Instance.Output("ERR: No sub command given.");
            }
        }

        private void ExecuteAddRadius(List<string> _params)
        {
            if (_params.Count != 5 && _params.Count != 6)
            {
                SdtdConsole.Instance.Output("ERR: Incorrect number of parameter. Excpected 5 or 6.");
                return;
            }

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[2]);
            if (ci == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get player position.");
                return;

            }
            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
            if (ep == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get player position");
                return;

            }

            Vector3i pos = ep.GetBlockPosition();
            bool isResetClaim = false;

            string type = "";
            if (_params.Count == 6)
            {
                if (_params[5].Trim().ToLower().Contains("timed:"))
                {
                    string sTime = _params[5].Trim().ToLower().Split(new string[] { "timed:" }, StringSplitOptions.None)[1].Trim();

                    if (!int.TryParse(sTime, out int hours))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The value for time to live for claim is not a valid integer. Parameter: {0}", _params[5]));
                        return;
                    }

                    type = string.Format("timed:{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now.AddHours(hours));
                }
                else if (_params[5].Trim().ToLower().Equals("reset"))
                {
                    isResetClaim = true;
                    type = "reset";

                }
                else
                    type = _params[5].Trim();
            }

            if (!int.TryParse(_params[1], out int r))
            {
                SdtdConsole.Instance.Output(string.Format("ERR: The value for radius is not a valid integer. Parameter: {0}", _params[1]));
                return;
            }

            int W;
            int E;
            int N;
            int S;

            if (isResetClaim)
            {
                W = pos.x - r;
                E = pos.x + r;
                N = pos.z + r;
                S = pos.z - r;

                int q = Math.DivRem(W, 16, out int remainder);
                if (remainder != 0)
                {
                    if (W > 0)
                        q += 1;

                    W = (q * 16) - 16;
                }
                q = Math.DivRem(E, 16, out remainder);
                if (remainder != 0)
                {
                    if (E < 0)
                        q -= 1;
                    E = (q * 16) + 16;
                }
                q = Math.DivRem(N, 16, out remainder);
                if (remainder != 0)
                {
                    if (N < 0)
                        q -= 1;
                    N = (q * 16) + 16;
                }
                q = Math.DivRem(S, 16, out remainder);
                if (remainder != 0)
                {
                    if (S > 0)
                        q += 1;

                    S = (q * 16) - 16;
                }
            }
            else
            {
                W = pos.x - r;
                E = pos.x + r;
                N = pos.z + r;
                S = pos.z - r;
            }

            if (type == "")
            {
                //check if normal claim and if trying to place an overlap on existing normal claim
                List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

                foreach (DbClaim activeClaim in lstClaims)
                {
                    if (activeClaim == null) continue;

                    if (activeClaim.Type == "")
                    {
                        bool abort = false;

                        if (W >= activeClaim.W_bound && W <= activeClaim.E_bound && S >= activeClaim.S_bound && S <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (W >= activeClaim.W_bound && W <= activeClaim.E_bound && N >= activeClaim.S_bound && N <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (E >= activeClaim.W_bound && E <= activeClaim.E_bound && N >= activeClaim.S_bound && N <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (E >= activeClaim.W_bound && E <= activeClaim.E_bound && S >= activeClaim.S_bound && S <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= W && activeClaim.W_bound <= E && activeClaim.S_bound >= S && activeClaim.S_bound <= N)
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= W && activeClaim.W_bound <= E && activeClaim.N_bound >= S && activeClaim.N_bound <= N)
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= W && activeClaim.E_bound <= E && activeClaim.N_bound >= S && activeClaim.N_bound <= N)
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= W && activeClaim.E_bound <= E && activeClaim.S_bound >= S && activeClaim.S_bound <= N)
                        {
                            abort = true;
                        }

                        if (abort)
                        {
                            SdtdConsole.Instance.Output("ERR: Claim intersects with an existing claim. Placement not allowed!");
                            return;
                        }
                    }
                }
            }

            Database.Instance.DeleteDbClaim(_params[3]);

            var dbclaim = new DbClaim
            {
                Id = _params[3],
                W_bound = W,
                E_bound = E,
                N_bound = N,
                S_bound = S,
                AccessLevel = int.Parse(_params[4]),
                Type = type,
                Whitelist = ""
            };

            Database.Instance.SaveDbClaim(dbclaim);

            SdtdConsole.Instance.Output(string.Format("Claim for claimid/steamid={0} added with boundaries: W:{1} E:{2} N:{3} S:{4}. Accesslevel={5} Type={6}", _params[3], W, E, N, S, _params[4], type));
        }

        private void ExecuteAddP2(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count != 3 && _params.Count != 4)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 4, found " + _params.Count + ".");
                return;
            }

            ClientInfo ci = _senderInfo.RemoteClientInfo;
            if (ci == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get your position");
                return;

            }
            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
            if (ep == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get your position");
                return;

            }
            if (!location.ContainsKey(ci.entityId))
            {
                SdtdConsole.Instance.Output("ERR: There isnt any stored location. Use p1 to store a position.");
                SdtdConsole.Instance.Output(GetHelp());
                return;
            }

            bool isResetClaim = false;
            Vector3i storedPos;
            location.TryGetValue(ci.entityId, out storedPos);

            string type = "";
            if (_params.Count == 4)
            {
                if (_params[3].Trim().ToLower().Contains("timed:"))
                {
                    string sTime = _params[3].Trim().ToLower().Split(new string[] { "timed:" }, StringSplitOptions.None)[1].Trim();

                    if (!int.TryParse(sTime, out int hours))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The value for time to live for claim is not a valid integer. Parameter: {0}", _params[3]));
                        return;
                    }

                    type = string.Format("timed:{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now.AddHours(hours));
                }
                else if (_params[3].Trim().ToLower().Equals("reset"))
                {
                    isResetClaim = true;
                    type = "reset";
                }
                else
                    type = _params[3].Trim();
            }

            //use current and stored pos to make bouundaries
            Vector3i pos = ep.GetBlockPosition();

            int x1;
            int y1;
            int z1;

            int x2;
            int y2;
            int z2;

            if (isResetClaim)
            {
                x1 = storedPos.x;
                y1 = storedPos.y;
                z1 = storedPos.z;

                x2 = pos.x;
                y2 = pos.y;
                z2 = pos.z;

                if (x2 < x1)
                {
                    int val = x1;
                    x1 = x2;
                    x2 = val;
                }

                if (y2 < y1)
                {
                    int val = y1;
                    y1 = y2;
                    y2 = val;
                }

                if (z2 < z1)
                {
                    int val = z1;
                    z1 = z2;
                    z2 = val;
                }

                int q = Math.DivRem(x1, 16, out int remainder);
                if (remainder != 0)
                {
                    if (x1 > 0)
                        q += 1;

                    x1 = (q * 16) - 16;
                }
                q = Math.DivRem(x2, 16, out remainder);
                if (remainder != 0)
                {
                    if (x2 < 0)
                        q -= 1;
                    x2 = (q * 16) + 16;
                }
                q = Math.DivRem(z2, 16, out remainder);
                if (remainder != 0)
                {
                    if (z2 < 0)
                        q -= 1;
                    z2 = (q * 16) + 16;
                }
                q = Math.DivRem(z1, 16, out remainder);
                if (remainder != 0)
                {
                    if (z1 > 0)
                        q += 1;

                    z1 = (q * 16) - 16;
                }
            }
            else
            {
                x1 = storedPos.x;
                y1 = storedPos.y;
                z1 = storedPos.z;

                x2 = pos.x;
                y2 = pos.y;
                z2 = pos.z;

                if (x2 < x1)
                {
                    int val = x1;
                    x1 = x2;
                    x2 = val;
                }

                if (y2 < y1)
                {
                    int val = y1;
                    y1 = y2;
                    y2 = val;
                }

                if (z2 < z1)
                {
                    int val = z1;
                    z1 = z2;
                    z2 = val;
                }
            }

            if (type == "")
            {
                //check if normal claim and if trying to place an overlap on existing normal claim
                List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

                foreach (DbClaim activeClaim in lstClaims)
                {
                    if (activeClaim == null) continue;

                    if (activeClaim.Type == "")
                    {
                        bool abort = false;

                        if (x1 >= activeClaim.W_bound && x1 <= activeClaim.E_bound && z1 >= activeClaim.S_bound && z1 <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (x1 >= activeClaim.W_bound && x1 <= activeClaim.E_bound && z2 >= activeClaim.S_bound && z2 <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (x2 >= activeClaim.W_bound && x2 <= activeClaim.E_bound && z2 >= activeClaim.S_bound && z2 <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (x2 >= activeClaim.W_bound && x2 <= activeClaim.E_bound && z1 >= activeClaim.S_bound && z1 <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= x1 && activeClaim.W_bound <= x2 && activeClaim.S_bound >= z1 && activeClaim.S_bound <= z2)
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= x1 && activeClaim.W_bound <= x2 && activeClaim.N_bound >= z1 && activeClaim.N_bound <= z2)
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= x1 && activeClaim.E_bound <= x2 && activeClaim.N_bound >= z1 && activeClaim.N_bound <= z2)
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= x1 && activeClaim.E_bound <= x2 && activeClaim.S_bound >= z1 && activeClaim.S_bound <= z2)
                        {
                            abort = true;
                        }

                        if (abort)
                        {
                            SdtdConsole.Instance.Output("ERR: Claim intersects with an existing claim. Placement not allowed!");
                            return;
                        }
                    }
                }
            }

            Database.Instance.DeleteDbClaim(_params[1]);

            var dbclaim = new DbClaim
            {
                Id = _params[1],
                W_bound = x1,
                E_bound = x2,
                N_bound = z2,
                S_bound = z1,
                AccessLevel = int.Parse(_params[2]),
                Type = type,
                Whitelist = ""
            };

            Database.Instance.SaveDbClaim(dbclaim);

            SdtdConsole.Instance.Output(string.Format("Claim for claimid/steamid={0} added with boundaries: W:{1} E:{2} N:{3} S:{4}. Accesslevel={5} Type={6}", _params[1], x1, x2, z2, z1, _params[2], type));
            return;
        }

        private void SavePosition(CommandSenderInfo _senderInfo)
        {
            ClientInfo ci = _senderInfo.RemoteClientInfo;
            if (ci == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get your position. P1 can only be used in game.");
                return;

            }
            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];
            if (ep == null)
            {
                SdtdConsole.Instance.Output("ERR: Unable to get your position");
                return;

            }
            if (location.ContainsKey(ci.entityId))
            {
                location.Remove(ci.entityId);
            }
            Vector3i pos = ep.GetBlockPosition();
            location.Add(ci.entityId, pos);
            SdtdConsole.Instance.Output("Stored position: " + pos.x + " " + pos.y + " " + pos.z);
            return;
        }

        private void ExecuteAdd(List<string> _params)
        {
            if (_params.Count < 7 || _params.Count > 8)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 7 or 8, found " + _params.Count + ".");
                return;
            }

            if (string.IsNullOrEmpty(_params[1]))
            {
                SdtdConsole.Instance.Output("ERR: Argument 'steamid' is empty.");
                return;
            }

            string type = "";
            if (_params.Count == 8)
            {
                if (_params[7].Trim().ToLower().Contains("timed:"))
                {
                    string sTime = _params[7].Trim().ToLower().Split(new string[] { "timed:" }, StringSplitOptions.None)[1].Trim();

                    if (!int.TryParse(sTime, out int hours))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The value for time to live for claim is not a valid integer. Parameter: {0}", _params[7]));
                        return;
                    }

                    type = string.Format("timed:{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now.AddHours(hours));
                }
                else if (_params[7].Trim().ToLower().Equals("reset"))
                {
                    //make parameters WENS snap to chunkborders
                    //WENS = 2,3,4,5
                    int.TryParse(_params[2], out int W);
                    int.TryParse(_params[3], out int E);
                    int.TryParse(_params[4], out int N);
                    int.TryParse(_params[5], out int S);

                    int q = Math.DivRem(W, 16, out int remainder);
                    if (remainder != 0)
                    {
                        if (W > 0)
                            q += 1;

                        _params[2] = ((q * 16) - 16).ToString();
                    }
                    q = Math.DivRem(E, 16, out remainder);
                    if (remainder != 0)
                    {
                        if (E < 0)
                            q -= 1;
                        _params[3] = ((q * 16) + 16).ToString();
                    }
                    q = Math.DivRem(N, 16, out remainder);
                    if (remainder != 0)
                    {
                        if (N < 0)
                            q -= 1;
                        _params[4] = ((q * 16) + 16).ToString();
                    }
                    q = Math.DivRem(S, 16, out remainder);
                    if (remainder != 0)
                    {
                        if (S > 0)
                            q += 1;

                        _params[5] = ((q * 16) - 16).ToString();
                    }

                    type = "reset";

                }
                else
                    type = _params[7].Trim();
            }

            if (type == "")
            {
                //check if normal claim and if trying to place an overlap on existing normal claim
                List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();

                foreach (DbClaim activeClaim in lstClaims)
                {
                    if (activeClaim == null) continue;

                    if (activeClaim.Type == "")
                    {
                        bool abort = false;

                        if (int.Parse(_params[2]) >= activeClaim.W_bound && int.Parse(_params[2]) <= activeClaim.E_bound && int.Parse(_params[5]) >= activeClaim.S_bound && int.Parse(_params[5]) <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (int.Parse(_params[2]) >= activeClaim.W_bound && int.Parse(_params[2]) <= activeClaim.E_bound && int.Parse(_params[4]) >= activeClaim.S_bound && int.Parse(_params[4]) <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (int.Parse(_params[3]) >= activeClaim.W_bound && int.Parse(_params[3]) <= activeClaim.E_bound && int.Parse(_params[4]) >= activeClaim.S_bound && int.Parse(_params[4]) <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (int.Parse(_params[3]) >= activeClaim.W_bound && int.Parse(_params[3]) <= activeClaim.E_bound && int.Parse(_params[5]) >= activeClaim.S_bound && int.Parse(_params[5]) <= activeClaim.N_bound)
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= int.Parse(_params[2]) && activeClaim.W_bound <= int.Parse(_params[3]) && activeClaim.S_bound >= int.Parse(_params[5]) && activeClaim.S_bound <= int.Parse(_params[4]))
                        {
                            abort = true;
                        }
                        else if (activeClaim.W_bound >= int.Parse(_params[2]) && activeClaim.W_bound <= int.Parse(_params[3]) && activeClaim.N_bound >= int.Parse(_params[5]) && activeClaim.N_bound <= int.Parse(_params[4]))
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= int.Parse(_params[2]) && activeClaim.E_bound <= int.Parse(_params[3]) && activeClaim.N_bound >= int.Parse(_params[5]) && activeClaim.N_bound <= int.Parse(_params[4]))
                        {
                            abort = true;
                        }
                        else if (activeClaim.E_bound >= int.Parse(_params[2]) && activeClaim.E_bound <= int.Parse(_params[3]) && activeClaim.S_bound >= int.Parse(_params[5]) && activeClaim.S_bound <= int.Parse(_params[4]))
                        {
                            abort = true;
                        }

                        if (abort)
                        {
                            SdtdConsole.Instance.Output("ERR: Claim intersects with an existing claim. Placement not allowed!");
                            return;
                        }
                    }
                }
            }

            Database.Instance.DeleteDbClaim(_params[1]);

            var dbclaim = new DbClaim
            {
                Id = _params[1],
                W_bound = int.Parse(_params[2]),
                E_bound = int.Parse(_params[3]),
                N_bound = int.Parse(_params[4]),
                S_bound = int.Parse(_params[5]),
                AccessLevel = int.Parse(_params[6]),
                Type = type,
                Whitelist = ""
            };

            Database.Instance.SaveDbClaim(dbclaim);

            SdtdConsole.Instance.Output(string.Format("Claim for steamid={0} added with boundaries:  W:{1} E:{2} N:{3} S:{4}. Type={5}", _params[1], _params[2], _params[3], _params[4], _params[5], type));
            return;
        }

        private void ExecuteRemove(List<string> _params)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2, found " + _params.Count + ".");
                return;
            }

            if (string.IsNullOrEmpty(_params[1]))
            {
                SdtdConsole.Instance.Output("ERR: Argument 'steamid' is empty.");
                return;
            }
            bool removed = Database.Instance.DeleteDbClaim(_params[1]);
            if (removed)
            {
                SdtdConsole.Instance.Output(string.Format("Claim for steamid={0} has been removed from claims.", _params[1]));

            }
            else SdtdConsole.Instance.Output(string.Format("ERR: Claim for steamid={0} could not be removed from claims.", _params[1]));
        }

        private void ExecuteList()
        {
            SdtdConsole.Instance.Output("Defined claims:");
            SdtdConsole.Instance.Output("  SteamID: (Boundaries) (AccessLevel) (Type) (Whitelist)");

            List<DbClaim> lstClaims = Database.Instance.GetAllDbClaims();
            if (lstClaims == null) return;

            foreach (DbClaim activeClaim in lstClaims)
            {
                string boundaries = string.Format("(W={0} E={1} N={2} S={3})", activeClaim.W_bound, activeClaim.E_bound, activeClaim.N_bound, activeClaim.S_bound);
                SdtdConsole.Instance.Output(string.Format("  {0}: {1} ({2}) ({3}) ({4})", activeClaim.Id, boundaries, activeClaim.AccessLevel, activeClaim.Type, activeClaim.Whitelist));
            }
        }

        private void ExecuteWhiteList(List<string> _params)
        {
            if (_params.Count != 4)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 4, found " + _params.Count + ".");
                return;
            }

            if (string.IsNullOrEmpty(_params[2]))
            {
                SdtdConsole.Instance.Output("ERR: Argument 'claimid' is empty.");
                return;
            }

            DbClaim activeClaim = Database.Instance.GetDbClaim(_params[2]);
            if (activeClaim == null)
            {
                //no claim to add whitelist to
                SdtdConsole.Instance.Output("ERR: Claim not found!");
                return;

            }

            string steamID = "";
            string playerName = "";

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[3]);
            if (ci == null)
            {
                //check if steamid for offline use

                DbPlayer player = Database.Instance.GetDbPlayer(_params[3]);
                if (player == null)
                {
                    //check if portal type and player = public
                    if (activeClaim.Type.ToLower().Contains("portal:") && _params[3].ToLower() == "public")
                    {
                        steamID = "public";
                        playerName = "public";
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("Player not found!");
                        return;
                    }
                }
                else
                {
                    steamID = player.Id;
                    playerName = player.Name;
                }
            }
            else
            {
                steamID = ci.PlatformId.ToString();
                playerName = ci.playerName;
            }

            //check if add or remove parameter
            if (_params[1].ToLower().Equals("add"))
            {
                if (activeClaim.Whitelist.Contains(steamID))
                {
                    SdtdConsole.Instance.Output(playerName + " is already whitelisted for your claim.");
                    return;
                }
                else
                {
                    string currentWhitelist = activeClaim.Whitelist;
                    currentWhitelist += playerName + "(" + steamID + ")";
                    //set whitelist 
                    Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);

                    SdtdConsole.Instance.Output(playerName + " has been whitelisted for your claim.");
                    return;

                }

            }
            else if (_params[1].ToLower().Equals("remove"))
            {
                if (!activeClaim.Whitelist.Contains(steamID))
                {
                    SdtdConsole.Instance.Output(playerName + " is not in your whitelist.");
                    return;
                }
                else
                {
                    string currentWhitelist = activeClaim.Whitelist;
                    currentWhitelist = currentWhitelist.Replace(playerName + "(" + steamID + ")", "");
                    Database.Instance.SetDbClaimWhitelist(activeClaim.Id, currentWhitelist);

                    SdtdConsole.Instance.Output(playerName + " has been removed from the whitelist.");
                    return;

                }
            }
        }
    }
}
