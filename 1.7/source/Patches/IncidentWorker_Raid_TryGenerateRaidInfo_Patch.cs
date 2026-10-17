using HarmonyLib;
using RimWorld;

namespace Tunnel_Raids.Patches
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
    internal static class IncidentWorker_Raid_TryGenerateRaidInfo_Patch
    {
        private static void Prefix(IncidentParms parms, out bool __state)
        {
            TunnelRaidOverride.contexts.Push(parms);
            __state = true;
        }

        private static void Finalizer(bool __state)
        {
            if (__state && TunnelRaidOverride.contexts.Count > 0)
            {
                TunnelRaidOverride.contexts.Pop();
            }
        }
    }
}
