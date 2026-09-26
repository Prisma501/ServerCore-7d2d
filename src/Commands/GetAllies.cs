using Epic.OnlineServices.Presence;
using Steamworks;
using System;
using System.Collections.Generic;
using static vp_ComponentPreset;

namespace PrismaCore.CustomCommands
{
    public class GetAllies : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-getallies", "getallies" };
        }

        public override string getDescription()
        {
            return "Returns allies for a specific player";
        }

        public override string getHelp()
        {
            return "Returns allies for a specific player\n" +
                "Usage:\n" +
                "   getallies <entityId/platformId>";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count > 1)
            {
                SdtdConsole.Instance.Output("Usage: getallies <entityId|platformId>");
            }
            else if (_params.Count == 1)
            {
                ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);
                if (clientInfo == null)
                {
                    SdtdConsole.Instance.Output("ERR: entityId/platformId id not found.");
                }
                else
                {
                    //EntityPlayer ep = GameManager.Instance.World.Players.dict[clientInfo.entityId];
                    //List<EntityPlayer> list = new List<EntityPlayer>();
                    //if (ep.Party != null)
                    //{

                    //}

                    //PersistentPlayerData data = GameManager.Instance.persistentPlayers.GetPlayerData(clientInfo.CrossplatformId);
                    string allies = string.Empty;
                    ClientInfo cInfo;

                    foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in GameManager.Instance.persistentPlayers.Allies.EnumerateAllies(clientInfo.CrossplatformId))
                    {
                        //PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(platformUserIdentifierAbs);
                        //if (playerData != null)
                        //{
                            if (platformUserIdentifierAbs != null)
                            {
                                cInfo = ConsoleHelper.ParseParamIdOrName(platformUserIdentifierAbs.CombinedString, true, false);
                                if (cInfo != null)
                                {
                                    allies += $"{cInfo.playerName}({cInfo.PlatformId}),";
                                }
                                else
                                {
                                    allies += $"offline({platformUserIdentifierAbs.CombinedString}),";
                                }
                            }
                        //}
                    }

                    if (allies.EndsWith(","))
                    {
                        allies = allies.TrimEnd(',');
                    }
                                       
                    SdtdConsole.Instance.Output($"{allies}");
                }
            }
            else if (_params.Count == 0)
            {
                SdtdConsole.Instance.Output("Usage: getallies <entityId|platformId>");
            }

        }
    }
}
