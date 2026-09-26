using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class RenderMap : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Render the current map to maptiles for ClaimCreator";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-rendermap" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            MapRendering.MapRendering.Instance.RenderFullMap();

            SdtdConsole.Instance.Output("Render map done.");
        }
    }
}