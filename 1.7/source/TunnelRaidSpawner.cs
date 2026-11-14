using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tunnel_Raids
{
    public class TunnelRaidSpawner : GroundSpawner, IThingHolder
    {
        private ThingOwner<Pawn> innerContainer;

        protected override IntRange ResultSpawnDelay => GetScaledSpawnDelay(def.building.groundSpawnerSpawnDelay, ModSettings.tunnelEmergenceDelayPercent);

        public static IntRange GetScaledSpawnDelay(IntRange baseline, int percent)
        {
            percent = ModSettings.NormalizeTunnelEmergenceDelayPercent(percent);
            if (percent == 100)
            {
                return baseline;
            }

            float multiplier = percent / 100f;
            int first = Mathf.Max(1, Mathf.RoundToInt(baseline.min * multiplier));
            int second = Mathf.Max(1, Mathf.RoundToInt(baseline.max * multiplier));
            return new IntRange(Mathf.Min(first, second), Mathf.Max(first, second));
        }

        public TunnelRaidSpawner()
        {
            innerContainer = new ThingOwner<Pawn>(this, oneStackOnly: false);
        }

        public bool TryAddPawn(Pawn pawn)
        {
            return innerContainer.TryAdd(pawn);
        }

        protected override void Spawn(Map map, IntVec3 loc)
        {
            innerContainer.TryDropAll(loc, map, ThingPlaceMode.Near, null, null, playDropSound: false);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}
