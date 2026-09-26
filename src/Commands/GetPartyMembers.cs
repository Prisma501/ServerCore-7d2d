using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class GetParty : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-getparty", "getparty" };
        }

        public override string getDescription()
        {
            return "Returns all online party members for a specific player";
        }

        public override string getHelp()
        {
            return "Returns all online party members for a specific player\n" +
                "Usage:\n" +
                "   getparty <entityId/platformId>";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count > 1)
            {
                SdtdConsole.Instance.Output("Usage: getparty <entityId|platformId>");
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
                    EntityPlayer ep = GameManager.Instance.World.Players.dict[clientInfo.entityId];
                    List<EntityPlayer> list = new List<EntityPlayer>();
                    if (ep.Party != null)
                    {
                        list.AddRange(ep.Party.MemberList);
                    }
                    else return;

                    ClientInfo cInfo;
                    string partyMembers = null;

                    foreach (EntityPlayer EP in list)
                    {
                        cInfo = ConsoleHelper.ParseParamIdOrName(EP.PlayerDisplayName, true, false);
                        if (cInfo != null)
                        {
                            partyMembers += $"{cInfo.playerName}({cInfo.PlatformId}),";
                        }
                    }

                    if(partyMembers.EndsWith(","))
                    {
                        partyMembers = partyMembers.TrimEnd(',');
                    }
                    SdtdConsole.Instance.Output($"{partyMembers}");
                    //Log.Out($"[PrismaCore] {EP.PlayerDisplayName}({cInfo.PlatformId})");
                }
            }
            else if (_params.Count == 0)
            {
                SdtdConsole.Instance.Output("Usage: getparty <entityId|platformId>");
            }

        }
    }
}
