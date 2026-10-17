using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tunnel_Raids
{
    public class TunnelRaidSpawner : GroundSpawner, IThingHolder
    {
        private ThingOwner<Pawn> innerContainer;

        protected override IntRange ResultSpawnDelay => def.building.groundSpawnerSpawnDelay;

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
