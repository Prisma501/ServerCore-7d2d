using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class PrefabFillBlockOne : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "place one block at a time without the need of chunkreloading (RPC)";
        }

        public override string getHelp()
        {
            return "Usage:" +
                   "  1. fblock1 <blockname> <x> <y> <z>\n" +
                   "1. Place one block on position x,y,z";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-fblock1", "fblock1" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 4)
                {
                    SdtdConsole.Instance.Output("Invalid number of parameters. Expected 4.");
                    return;
                }

                PlaceOnPosition(_params);

            }
            catch (Exception e)
            {
                Log.Out("Error in PrefabFillBlockOne.Run: " + e);
            }
        }

        private void PlaceOnPosition(List<string> _params)
        {
            try
            {
                string blockName = _params[0].Trim();
                int x = int.MinValue;
                int.TryParse(_params[1], out x);
                int y = int.MinValue;
                int.TryParse(_params[2], out y);
                int z = int.MinValue;
                int.TryParse(_params[3], out z);

                if (x == int.MinValue || y == int.MinValue || z == int.MinValue)
                {
                    SdtdConsole.Instance.Output("At least one of the given coordinates is not a valid integer");
                    return;
                }

                Vector3i v = new Vector3i(x, y, z);

                if (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(v.x), World.toChunkXZ(v.z)) == null)
                {
                    SdtdConsole.Instance.Output("The chunk at given position is not loaded. Aborting.");
                    return;
                }
                else
                {
                    bool blockFound = false;
                    int blockId = -999;
                    bool hasBlockId = int.TryParse(blockName, out blockId);
                    BlockValue blockV = BlockValue.Air;
                    foreach (Block block in Block.list)
                    {
                        if (block != null && block.GetBlockName() != null)
                        {
                            if (block.GetBlockName().EqualsCaseInsensitive(blockName) || (hasBlockId && block.blockID == blockId))
                            {
                                blockFound = true;
                                blockV = Block.GetBlockValue(block.GetBlockName());
                                break;
                            }
                        }
                    }

                    if (!blockFound)
                    {
                        SdtdConsole.Instance.Output("Invalid block name or ID " + blockName + "");
                        return;
                    }

                    BlockChangeInfo bci = new BlockChangeInfo(v, blockV, true, false);

                    List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
                    changes.Add(bci);

                    try
                    {
                        GameManager.Instance.SetBlocksRPC(changes);
                    }
                    catch { GameManager.Instance.SetBlocksRPC(changes); }

                    SdtdConsole.Instance.Output("Block placed at: " + v.ToString());
                }
            }
            catch { }
        }
    }
}
