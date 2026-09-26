using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class PrefabCheckBlock : ConsoleCmdAbstract
    {

        public override string getDescription()
        {
            return "Checks the type of block by coordinates or under your feet.";
        }

        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. bcheck <x> <y> <z>\n" +
                "  2. bcheck\n" +
                "1. check the block at x, y, z.\n" +
                "2. check the block under your feet.";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-bcheck", "bcheck" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {

            try
            {

                if (_params.Count != 0 && _params.Count != 3)
                {
                    SdtdConsole.Instance.Output("ERR: Wrong number of arguments, expected 0 or 3, found " + _params.Count + ".");
                    SdtdConsole.Instance.Output(GetHelp());
                    return;
                }
                else
                {
                    int x = int.MinValue;
                    int y = int.MinValue;
                    int z = int.MinValue;

                    if (_params.Count == 0)
                    {
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
                        x = ep.GetBlockPosition().x;
                        y = ep.GetBlockPosition().y;
                        z = ep.GetBlockPosition().z;

                        BlockValue currentBlock = GameManager.Instance.World.GetBlock(new Vector3i(x, y - 1, z));
                        SdtdConsole.Instance.Output("checking at " + x + " " + (y - 1) + " " + z + " BlockID: " + currentBlock.type);

                        foreach (Block block in Block.list)
                        {
                            if (block != null && block.GetBlockName() != null)
                            {
                                if (Block.GetBlockValue(block.GetBlockName()).type == currentBlock.type)
                                {
                                    SdtdConsole.Instance.Output("block at " + x + " " + (y - 1) + " " + z + " is " + block.GetBlockName());
                                }
                            }
                        }
                    }
                    else if (_params.Count == 3)
                    {
                        int.TryParse(_params[0], out x);
                        int.TryParse(_params[1], out y);
                        int.TryParse(_params[2], out z);

                        BlockValue currentBlock = GameManager.Instance.World.GetBlock(new Vector3i(x, y, z));
                        SdtdConsole.Instance.Output("checking at " + x + " " + y + " " + z + " BlockID: " + currentBlock.type);

                        foreach (Block block in Block.list)
                        {
                            if (block != null && block.GetBlockName() != null)
                            {
                                if (Block.GetBlockValue(block.GetBlockName()).type == currentBlock.type)
                                {
                                    SdtdConsole.Instance.Output("block at " + x + " " + y + " " + z + " is " + block.GetBlockName());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in PrefabFCheckBlock.Run: " + e);
            }
        }
    }
}
