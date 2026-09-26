using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class ListPlayersSkill : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "list players Skills";
        }

        public override string getHelp()
        {
            return "list players Skills\n" +
            "Usage:\n" +
            "1.   lps <name / entity id> \n" +
            "2.   lps ";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-listplayerskill", "listplayerskill", "lps" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 0 && _params.Count != 1)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 0 or 1, found " + _params.Count + ".");
                    return;
                }

                if (_params.Count == 1)
                {
                    ClientInfo ci = ConsoleHelper.ParseParamIdOrName(_params[0]);
                    if (ci == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                        return;
                    }

                    EntityPlayer p1 = GameManager.Instance.World.Players.dict[ci.entityId];
                    if (p1 == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
                        return;
                    }
                    printPlayerSkill(p1, ci);
                }
                else
                {
                    List<EntityPlayer> players = GameManager.Instance.World.Players.list;
                    foreach (EntityPlayer player in players)
                    {
                        ClientInfo ci = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
                        if (ci == null)
                        {
                            continue;
                        }
                        printPlayerSkill(player, ci);

                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in ListPlayersSkill.Execute: " + e);
            }
        }

        private void printPlayerSkill(EntityPlayer player, ClientInfo ci)
        {
            SdtdConsole.Instance.Output($"Attributes/Skills/Perks for player (Name : Level):  {ci.playerName}");
            foreach (KeyValuePair<int, ProgressionValue> c in player.Progression.GetDict())
            {
                SdtdConsole.Instance.Output($"{c.Value.Name} : {c.Value.Level}");

            }
        }
    }
}
