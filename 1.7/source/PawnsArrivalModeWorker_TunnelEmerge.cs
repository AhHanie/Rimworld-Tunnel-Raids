using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tunnel_Raids
{
    public class PawnsArrivalModeWorker_TunnelEmerge : PawnsArrivalModeWorker
    {
        private const int PawnsPerTunnel = 5;

        public override bool CanUseWith(IncidentParms parms)
        {
            if (!base.CanUseWith(parms))
            {
                return false;
            }
            Faction faction = parms.faction;
            return faction == null || (faction.def.humanlikeFaction && faction.HostileTo(Faction.OfPlayer) && TunnelRaidEligibility.IsFactionAllowed(faction));
        }

        public override bool CanUseOnMap(Map map)
        {
            return map.IsPlayerHome && base.CanUseOnMap(map);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            if (!TunnelRaidEligibility.IsFactionAllowed(parms.faction))
            {
                Logger.Warning("Faction " + parms.faction.def.defName + " is not allowed to use tunnels, falling back to EdgeWalkIn");
                parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
            }
            Map map = (Map)parms.target;
            if (parms.spawnCenter.IsValid || TunnelSiteFinder.TryFindRoot(map, parms, out parms.spawnCenter))
            {
                parms.spawnRotation = Rot4.Random;
                return true;
            }
            Logger.Warning("No tunnel site found, falling back to EdgeWalkIn");
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
        }

        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            Map map = (Map)parms.target;
            int wanted = Mathf.Clamp((pawns.Count + PawnsPerTunnel - 1) / PawnsPerTunnel, 1, TunnelSiteFinder.MaxTunnels);
            List<IntVec3> cells = TunnelSiteFinder.ChooseCells(map, parms.spawnCenter, wanted);
            int count = cells.Count;
            TunnelRaidSpawner[] spawners = new TunnelRaidSpawner[count];
            for (int i = 0; i < count; i++)
            {
                TunnelRaidSpawner spawner = (TunnelRaidSpawner)ThingMaker.MakeThing(TR_ThingDefOf.TR_TunnelRaidSpawner);
                GenSpawn.Spawn(spawner, cells[i], map);
                spawners[i] = spawner;
            }
            for (int i = 0; i < pawns.Count; i++)
            {
                spawners[i % count].TryAddPawn(pawns[i]);
            }
        }
    }
}
