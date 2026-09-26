using JetBrains.Annotations;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class RepairTrees : ConsoleCmdAbstract
    {
        private List<BlockChangeInfo> _blockChangeInfos = new List<BlockChangeInfo>();

        public override string[] getCommands()
        {
            return new[] { "pc-rt", "rt", "repairtrees" };
        }

        public override string getDescription()
        {
            return "Repair indestructable trees (only with damageindicator)";
        }

        public override string getHelp()
        {
            return "Repair indestructable trees (only with damageindicator)\n" +
                "Usage:\n" +
                "   rt  (list bugged trees)\n" +
                "   rt repair  (repair bugged trees)";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
            foreach (var chunk in chunkArrayCopySync)
                RepairChunk(chunk, _params);

            if (_params.Count == 1 && _params[0].ToLower().Equals("repair"))
                GameManager.Instance.SetBlocksRPC(_blockChangeInfos);
        }

        private void RepairChunk([NotNull] Chunk chunk, List<string> _params)
        {
            var problemsTree = 0;
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    for (int y = 0; y < byte.MaxValue; ++y)
                    {
                        var blockValue = chunk.GetBlockNoDamage(x, y, z);
                        var pos = new Vector3i(x, y, z);
                        problemsTree += RepairBlockTree(blockValue, chunk, pos, _params);
                    }
                }
            }

        }

        private int RepairBlockTree(BlockValue blockValue, Chunk chunk, Vector3i posInChunk, List<string> _params)
        {
            var block = blockValue.Block;

            if (!(block is BlockModelTree))
                return 0;

            blockValue.damage = chunk.GetDamage(posInChunk.x, posInChunk.y, posInChunk.z);

            if (blockValue.damage < block.MaxDamage)
                return 0;

            block.EnablePassThroughDamage = false;

            if (_params.Count == 1 && _params[0].ToLower().Equals("repair"))
            {
                blockValue.damage = 0;
                _blockChangeInfos.Add(new BlockChangeInfo(chunk.ToWorldPos(posInChunk), blockValue, false, true));

                Log.Out($"Bugged tree repaired at {chunk.ToWorldPos(posInChunk).ToString()}");
            }
            else
                Log.Out($"Bugged tree found at {chunk.ToWorldPos(posInChunk).ToString()}");

            return 1;
        }

    }
}
