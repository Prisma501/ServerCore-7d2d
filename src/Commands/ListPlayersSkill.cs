using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
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
                            //SdtdConsole.Instance.Output("ERR: Playername or entity id not found.");
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
            //Dictionary<string, int> attrs = new Dictionary<string, int>();
            //Dictionary<string, int> skills = new Dictionary<string, int>();
            //Dictionary<string, int> perks = new Dictionary<string, int>();

            SdtdConsole.Instance.Output($"Attributes/Skills/Perks for player (Name : Level):  {ci.playerName}");
            foreach (KeyValuePair<int, ProgressionValue> c in player.Progression.GetDict())
            {
                //if (c.Value.ProgressionClass.IsAttribute)
                //{
                //    attrs.Add(c.Value.ProgressionClass.NameTag.ToString(), c.Value.Level);
                //}
                //if (c.Value.ProgressionClass.IsSkill)
                //{
                //    skills.Add(c.Value.ProgressionClass.NameTag.ToString(), c.Value.Level);
                //}
                //if (c.Value.ProgressionClass.IsPerk)
                //{
                //    perks.Add(c.Value.ProgressionClass.NameTag.ToString(), c.Value.Level);
                //}
                SdtdConsole.Instance.Output($"{c.Value.Name} : {c.Value.Level}");

            }
        }
    }
}
