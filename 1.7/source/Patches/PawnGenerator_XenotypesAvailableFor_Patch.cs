using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Tunnel_Raids.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.XenotypesAvailableFor))]
    internal static class PawnGenerator_XenotypesAvailableFor_Patch
    {
        private static void Postfix(PawnKindDef kind, FactionDef factionDef, Faction faction, Dictionary<XenotypeDef, float> __result)
        {
            if (!TunnelRaidOverride.TryGetForcedXenotype(kind, faction, factionDef, out XenotypeDef xenotype))
            {
                return;
            }
            __result.Clear();
            __result.Add(xenotype, 1f);
        }
    }
}
