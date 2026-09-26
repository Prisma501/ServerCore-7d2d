using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class ListPlayersBuff : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "List players active buffs";
        }

        public override string getHelp()
        {
            return "List players active buffs\n" +
            "Usage:\n" +
            "1.   lpbuffs <name / entity id> \n" +
            "2.   lpbuffs ";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-listplayerbuffs", "listplayerbuffs", "lpbuffs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }

                if (_params.Count == 1)
                {
                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("Playername or entity id not found.");
                        return;
                    }

                    EntityPlayer p1 = GameManager.Instance.World.Players.dict[ci.entityId];
                    if (p1 == null)
                    {
                        SdtdConsole.Instance.Output("Playername or entity id not found.");
                        return;
                    }
                    printPlayerBuffs(p1, ci.playerName);
                }
                else
                {
                    List<EntityPlayer> players = GameManager.Instance.World.Players.list;
                    foreach (EntityPlayer player in players)
                    {
                        ClientInfo cli = ConsoleHelper.ParseParamIdOrName(player.entityId.ToString());
                        printPlayerBuffs(player, cli.playerName);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in ListPlayersBuff.Execute: " + e);
            }
        }

        private void printPlayerBuffs(EntityPlayer player, string name)
        {
            SdtdConsole.Instance.Output($"Active Buffs for player {name}:");

            foreach (BuffValue bv in player.Buffs.ActiveBuffs)
            {
                try
                {
                    SdtdConsole.Instance.Output($"Buff Name: {bv.BuffClass.LocalizedName}");
                    SdtdConsole.Instance.Output($"Technical Buff Name: {bv.BuffName}");
                    if (!string.IsNullOrEmpty(bv.BuffClass.Description))
                    {
                        SdtdConsole.Instance.Output($"Buff Description: {bv.BuffClass.Description}");
                    }

                    SdtdConsole.Instance.Output(string.Empty);
                }
                catch (Exception e)
                {
                    SdtdConsole.Instance.Output($"error on printplayerbuffs: {e.ToString()}");
                    continue;
                }
            }
        }
    }
}
