using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class Tooltip : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-tooltip", "tooltip" };
        }

        public override string getDescription()
        {
            return "Show a tooltip on a specific connected client";
        }

        public override string getHelp()
        {
            return "Show a tooltip on a specific connected client\n" +
                "Usage:\n" +
                "   tooltip <Name/EntityId/SteamId/all> <tooltipName>\n";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 2, found " + _params.Count + ".");
                return;
            }

            //int.TryParse(_params[0], out int id);
            if (_params[0].ToLower() == "all")
            {
                World w = GameManager.Instance.World;
                foreach (KeyValuePair<int, EntityPlayer> player in w.Players.dict)
                {
                    if (BuffManager.Buffs.ContainsKey(_params[1]))
                    {
                        try
                        {
                            if (player.Value != null)
                                player.Value.Buffs.AddBuff(_params[1], -1, true, false);
                        }
                        catch { continue; }
                    }
                    else
                    {
                        SdtdConsole.Instance.Output("[PrismaCore] ERR: Tooltip can not be found.");
                        return;
                    }
                }

                return;
            }

            ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);

            if (ci == null)
            {
                SdtdConsole.Instance.Output("[PrismaCore] ERR: Player cannot be found.");
                return;
            }

            EntityPlayer ep = GameManager.Instance.World.Players.dict[ci.entityId];

            if (ep == null)
            {
                SdtdConsole.Instance.Output("[PrismaCore] ERR: Player cannot be found.");
                return;
            }

            if (BuffManager.Buffs.ContainsKey(_params[1]))
            {
                ep.Buffs.AddBuff(_params[1], -1, true, false);
            }
            else
            {
                SdtdConsole.Instance.Output("[PrismaCore] ERR: Tooltip can not be found.");
            }
        }
    }
}
