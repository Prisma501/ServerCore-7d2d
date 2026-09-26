using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class TraderList : ConsoleCmdAbstract
    {

        public override string getDescription()
        {
            return "List Trader Areas";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. traderlist";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-traderlist", "traderlist" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                List<TraderArea> list = GameManager.Instance.World.TraderAreas;
                int index = 0;
                foreach (TraderArea trader in list)
                {
                    index++;
                    Vector3i pos = trader.Position;
                    Vector3i size = trader.PrefabSize;
                    Vector3i protection = trader.ProtectSize;

                    SdtdConsole.Instance.Output("traderlist: [" + index + "] Position, pos_x=" + pos.x + ", pos_y=" + pos.y + ", pos_z=" + pos.z + ", closed=" + trader.IsClosed);
                    SdtdConsole.Instance.Output("traderlist: [" + index + "] Size, size_x=" + size.x + ", size_y=" + size.y + ", size_z=" + size.z + ", closed=" + trader.IsClosed);
                    SdtdConsole.Instance.Output("traderlist: [" + index + "] Protection, protection_x=" + protection.x + ", protection_y=" + protection.y + ", protection_z=" + protection.z + ", closed=" + trader.IsClosed);
                }

            }
            catch (Exception e)
            {
                Log.Out("Error in TraderList.Run: " + e);
            }
        }

    }
}
