using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tunnel_Raids
{
    public static class TunnelRaidOverride
    {
        public static readonly Stack<IncidentParms> contexts = new Stack<IncidentParms>();

        public static int generationDepth = 0;

        public static bool TryGetForcedXenotype(PawnKindDef kind, Faction faction, FactionDef factionDef, out XenotypeDef xenotype)
        {
            xenotype = null;
            if (contexts.Count == 0 || kind == null || !kind.RaceProps.Humanlike)
            {
                return false;
            }
            IncidentParms parms = contexts.Peek();
            Faction raidFaction = parms.faction;
            if (raidFaction == null || !raidFaction.def.humanlikeFaction || parms.raidArrivalMode != TR_PawnsArrivalModeDefOf.TR_TunnelEmerge)
            {
                return false;
            }
            if (faction != null ? faction != raidFaction : factionDef != raidFaction.def)
            {
                return false;
            }
            if (!TunnelRaidEligibility.TryGetOverrideXenotype(out xenotype))
            {
                return false;
            }
            if (kind.xenotypeSet != null && kind.xenotypeSet.Count > 0 && !kind.xenotypeSet.Contains(xenotype))
            {
                Logger.WarningOnce("fixedKind:" + kind.defName, "Pawn kind " + kind.defName + " restricts its own xenotypes, so the tunnel raider xenotype override is not applied to it");
                xenotype = null;
                return false;
            }
            return true;
        }
    }
}
