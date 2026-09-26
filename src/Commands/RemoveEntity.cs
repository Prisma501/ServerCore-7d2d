using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class RemoveEntity : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "remove entity from game";
        }

        public override string getHelp()
        {
            return "Removes an entity from the game.\n" +
                "Usage:\n" +
                "   1. entityremove entityID\n" +
                "1. Remove an entity from game\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-entityremove", "entityremove" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 1, found " + _params.Count + ".");
                    return;
                }
                int entityID = int.MinValue;
                int.TryParse(_params[0], out entityID);

                if (entityID == int.MinValue)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid entity ID");
                    return;
                }
                GameManager.Instance.World.RemoveEntity(entityID, EnumRemoveEntityReason.Despawned);

                SdtdConsole.Instance.Output("Removed entity_id" + entityID);
            }
            catch (Exception e)
            {
                Log.Out("Error in RemoveEntity.Run: " + e);
            }
        }
    }
}
