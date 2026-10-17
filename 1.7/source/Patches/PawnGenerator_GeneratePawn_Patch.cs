using HarmonyLib;
using RimWorld;
using Verse;

namespace Tunnel_Raids.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    internal static class PawnGenerator_GeneratePawn_Patch
    {
        private static void Prefix(ref PawnGenerationRequest request, out bool __state)
        {
            __state = true;
            if (TunnelRaidOverride.generationDepth++ > 0)
            {
                return;
            }
            if (request.ForcedXenotype != null || request.ForcedCustomXenotype != null)
            {
                return;
            }
            if (TunnelRaidOverride.TryGetForcedXenotype(request.KindDef, request.Faction, null, out XenotypeDef xenotype))
            {
                request.ForcedXenotype = xenotype;
            }
        }

        private static void Finalizer(bool __state)
        {
            if (__state && TunnelRaidOverride.generationDepth > 0)
            {
                TunnelRaidOverride.generationDepth--;
            }
        }
    }
}
