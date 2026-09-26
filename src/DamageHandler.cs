using System.Collections.Generic;
using System.Threading;

namespace ServerCore
{
    public class DamageHandler
    {
        public static void StartThreadParameterizedCL(ClientInfo _cInfo)
        {
            var t = new Thread(() => CloseLootParameterized(_cInfo))
            {
                IsBackground = true
            };
            t.Start();
        }

        private static void CloseLootParameterized(ClientInfo _cInfo)
        {
           _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close looting", true));

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(200);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui close looting", true));
            }
        }

        public static void HandleFallingBlock(World w, Vector3i block)
        {
            BlockValue bv = w.GetBlock(block);

            if (bv.ischild || bv.Block.StabilityIgnore || bv.isair)
            {
                return;
            }

            List<BlockChangeInfo> changes = new List<BlockChangeInfo>();

            BlockChangeInfo bci = new BlockChangeInfo(block, new BlockValue(0), true, false);
            changes.Add(bci);


            if (changes.Count != 0)
            {
                GameManager.Instance.SetBlocksRPC(changes);
            }
        }

        public static void HandleFallingBlocks(World w, IList<Vector3i> _list)
        {
            List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
            Vector3i colPos = new Vector3i();
            colPos = _list[0];

            foreach (var bl in _list)
            {
                BlockValue bv = w.GetBlock(bl);

                if (bv.ischild || bv.Block.StabilityIgnore || bv.isair)
                {
                    continue;
                }

                BlockChangeInfo bci = new BlockChangeInfo(bl, new BlockValue(0), true, false);
                changes.Add(bci);

            }

            if (changes.Count != 0)
            {
                GameManager.Instance.SetBlocksRPC(changes);
                if (changes.Count > ServerCoreSettings.Instance.PreventFallingBlocks)
                {
                    Log.Out($"[PrismaCore] {changes.Count} falling blocks prevented! @ {colPos.ToString()}");
                }
            }
        }

        public static void HandleDamagePlayer(DamageProperties damProps, bool died)
        {
            if (!died) return;

            if (string.IsNullOrEmpty(damProps.VictimName) || string.IsNullOrEmpty(damProps.OffenderName)) return;

            //log all kills, pve or not
            Log.Out($"[PrismaCore]playerKilledByPlayer: {damProps.OffenderName} (offenderSteamId={damProps.OffenderSteamId}) @ {damProps.OffenderPosition.x} {damProps.OffenderPosition.y} {damProps.OffenderPosition.z} killed {damProps.VictimName} (victimSteamId={damProps.VictimSteamId}) @ {damProps.VictimPosition.x} {damProps.VictimPosition.y} {damProps.VictimPosition.z} with {weaponUsed(damProps.OffenderEntityId)}");
        }

        public static void HandleDamageOther(DamageProperties damProps, bool died)
        {
            if (!died) return;

            if (string.IsNullOrEmpty(damProps.VictimName) || string.IsNullOrEmpty(damProps.OffenderName)) return;

            Log.Out($"[PrismaCore]playerKilledByEntity: {damProps.OffenderName} ({damProps.OffenderPosition.x},{damProps.OffenderPosition.y},{damProps.OffenderPosition.z}) killed {damProps.VictimName} ({damProps.VictimSteamId}) @ {damProps.VictimPosition.x} {damProps.VictimPosition.y} {damProps.VictimPosition.z}");

            //(?<= playerKilledByPlayer : )([\w\s] +)
        }

        public static void LogDamageDetection(string name, string steamId, int strength)
        {
            Log.Out($"[PrismaCore]damageDetection(Entity): Player {name} ({steamId}) triggered damage detection! Damage done: {strength}");
        }

        public static string weaponUsed(int? entityId)
        {
            if (entityId == null)
            {
                return "unknown";
            }

            EntityPlayer pl = GameManager.Instance.World.Players.dict[(int)entityId];
            if (pl != null && pl.IsSpawned())
            {
                string hi = pl.inventory.holdingItem.Name;
                if (!string.IsNullOrEmpty(pl.inventory.holdingItem.Name))
                {
                    ItemValue _itemValue = ItemClass.GetItem(hi, true);
                    if (_itemValue.type != ItemValue.None.type)
                    {
                        hi = _itemValue.ItemClass.GetLocalizedItemName() ?? _itemValue.ItemClass.GetItemName();
                        return hi;
                    }
                }
                else
                {
                    return "unknown";
                }
            }

            return "unknown";
        }
    }

    public class DamageProperties
    {
        public Vector3i VictimPosition;
        public Vector3i OffenderPosition;
        public int VictimEntityId;
        public string VictimName;
        public string VictimSteamId;
        public int? OffenderEntityId;
        public string OffenderName;
        public string OffenderSteamId;
    }
}
