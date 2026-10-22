using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Tunnel_Raids
{
    public static class TunnelSiteFinder
    {
        public const int MaxTunnels = 5;

        private const int AnchorAttempts = 8;
        private const int AnchorRadius = 8;
        private const int ChildRadius = 6;
        private const int MinAnchorDistSq = 9;
        private const int MinSpacingSq = 9;

        private static readonly HashSet<string> MuddyTerrainDefNames = new HashSet<string>
        {
            "MarshyTerrain",
            "MarshFlood"
        };

        private static readonly HashSet<string> RockyTerrainDefNames = new HashSet<string>
        {
            "TileSandstone", "TileGranite", "TileLimestone", "TileSlate", "TileMarble",
            "FlagstoneSandstone", "FlagstoneGranite", "FlagstoneLimestone", "FlagstoneSlate", "FlagstoneMarble"
        };

        private static readonly List<IntVec3> chosen = new List<IntVec3>(MaxTunnels);

        public static bool TryFindRoot(Map map, IncidentParms parms, out IntVec3 root)
        {
            root = IntVec3.Invalid;
            List<Thing> targets = parms.attackTargets;
            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            bool useTargets = targets != null && targets.Count > 0;
            if (!useTargets && colonists.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < AnchorAttempts; i++)
            {
                IntVec3 anchor = useTargets ? targets.RandomElement().PositionHeld : colonists.RandomElement().Position;
                if (CellFinder.TryFindRandomCellNear(anchor, map, AnchorRadius, c => IsValidRoot(c, anchor, map), out root))
                {
                    return true;
                }
            }
            root = IntVec3.Invalid;
            return false;
        }

        public static List<IntVec3> ChooseCells(Map map, IntVec3 root, int count)
        {
            chosen.Clear();
            chosen.Add(root);
            for (int i = 1; i < count; i++)
            {
                if (CellFinder.TryFindRandomCellNear(root, map, ChildRadius, c => IsValidChild(c, map), out IntVec3 cell))
                {
                    chosen.Add(cell);
                }
            }
            return chosen;
        }

        public static bool IsValidSpawnCenter(Map map, IntVec3 c)
        {
            return c.IsValid
                && c.InBounds(map)
                && IsOpenGround(c, map)
                && map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors));
        }

        public static bool IsTerrainAllowed(TerrainDef terrain)
        {
            if (ModSettings.blockMuddyTunnelSites && IsMuddy(terrain))
            {
                return false;
            }
            return !(ModSettings.blockRockyTunnelSites && IsRocky(terrain));
        }

        private static bool IsMuddy(TerrainDef terrain)
        {
            return terrain.HasTag("Mud")
                || terrain.HasTag("WaterMarshy")
                || MuddyTerrainDefNames.Contains(terrain.defName);
        }

        private static bool IsRocky(TerrainDef terrain)
        {
            return terrain.IsRock
                || terrain.categoryType == TerrainDef.TerrainCategoryType.Stone
                || RockyTerrainDefNames.Contains(terrain.defName);
        }

        private static bool IsValidRoot(IntVec3 c, IntVec3 anchor, Map map)
        {
            return c.DistanceToSquared(anchor) >= MinAnchorDistSq && IsValidSpawnCenter(map, c);
        }

        private static bool IsValidChild(IntVec3 c, Map map)
        {
            if (!IsOpenGround(c, map))
            {
                return false;
            }
            for (int i = 0; i < chosen.Count; i++)
            {
                if (c.DistanceToSquared(chosen[i]) < MinSpacingSq)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsOpenGround(IntVec3 c, Map map)
        {
            if (!c.Standable(map) || c.Fogged(map) || c.GetEdifice(map) != null)
            {
                return false;
            }
            TerrainDef terrain = c.GetTerrain(map);
            return !terrain.IsWater
                && IsTerrainAllowed(terrain)
                && c.GetFirstThing(map, TR_ThingDefOf.TR_TunnelRaidSpawner) == null;
        }
    }
}
