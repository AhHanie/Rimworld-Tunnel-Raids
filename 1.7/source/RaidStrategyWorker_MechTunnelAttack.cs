using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tunnel_Raids
{
    public class MechTunnelRaidException : Exception
    {
        public MechTunnelRaidException(string message) : base(message)
        {
        }
    }

    public class RaidStrategyWorker_MechTunnelAttack : RaidStrategyWorker_ImmediateAttack
    {
        // Raw points. At 500 the strategy curve leaves about 312, enough for the tunneler (250) to be funded.
        public const float MinRawPoints = 500f;

        private const string TunnelerDefName = "Mech_Tunneler";

        private static PawnKindDef TunnelerKind =>
            ModsConfig.BiotechActive ? DefDatabase<PawnKindDef>.GetNamedSilentFail(TunnelerDefName) : null;

        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (!ModSettings.enableMechanoidTunnelRaids || TunnelerKind == null)
            {
                return false;
            }
            if (parms.faction == null || parms.faction.def != FactionDefOf.Mechanoid)
            {
                return false;
            }
            if ((groupKind != null && groupKind != PawnGroupKindDefOf.Combat) || (parms.pawnGroupKind != null && parms.pawnGroupKind != PawnGroupKindDefOf.Combat))
            {
                return false;
            }
            if (parms.points < MinRawPoints || parms.pawnKind != null)
            {
                return false;
            }
            if (parms.raidArrivalMode != null && parms.raidArrivalMode != TR_PawnsArrivalModeDefOf.TR_MechTunnelEmerge)
            {
                return false;
            }
            if (!(parms.target is Map map) || !map.IsPlayerHome)
            {
                return false;
            }
            return base.CanUseWith(parms, groupKind);
        }

        // Keeps vanilla's minimum-points clamp from ever leaving a budget below the tunneler's cost.
        public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
        {
            PawnKindDef tunneler = TunnelerKind;
            return Mathf.Max(base.MinimumPoints(faction, groupKind), tunneler != null ? tunneler.combatPower : 0f);
        }

        // Returning null would make TryGenerateRaidInfo build an ordinary group with no tunneler, so every
        // failure throws MechTunnelRaidException instead, which the TryGenerateRaidInfo patch turns into a failed raid.
        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            PawnKindDef tunnelerKind = TunnelerKind;
            if (tunnelerKind == null || parms.faction == null || parms.faction.def != FactionDefOf.Mechanoid)
            {
                throw new MechTunnelRaidException("Mechanoid tunnel raid needs Biotech's " + TunnelerDefName + " and the mechanoid faction, but got faction " + parms.faction?.def.defName);
            }

            PawnGenerationRequest request = new PawnGenerationRequest(tunnelerKind, parms.faction, PawnGenerationContext.NonPlayer, parms.target.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, biocodeWeaponChance: parms.biocodeWeaponsChance, biocodeApparelChance: parms.biocodeApparelChance, allowFood: def.pawnsCanBringFood);
            Pawn tunneler = PawnGenerator.GeneratePawn(request);
            if (tunneler == null)
            {
                throw new MechTunnelRaidException("Could not generate a " + TunnelerDefName);
            }

            List<Pawn> pawns = new List<Pawn> { tunneler };
            PawnGroupMakerParms groupParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, parms);
            groupParms.points = Mathf.Max(0f, parms.points - tunnelerKind.combatPower);
            if (groupParms.points > 0f && PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(groupParms, out _))
            {
                pawns.AddRange(PawnGroupMakerUtility.GeneratePawns(groupParms));
            }

            parms.raidArrivalMode.Worker.Arrive(pawns, parms);
            return pawns;
        }
    }
}
