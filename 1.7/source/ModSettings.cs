using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tunnel_Raids
{
    public class ModSettings : Verse.ModSettings
    {
        public static HashSet<string> selectedFactionDefNames = new HashSet<string>();

        public static HashSet<string> selectedXenotypeDefNames = new HashSet<string>();

        public static bool overrideTunnelXenotype = false;

        public static bool enableMechanoidTunnelRaids = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableMechanoidTunnelRaids, "enableMechanoidTunnelRaids", true);
            Scribe_Collections.Look(ref selectedFactionDefNames, "selectedFactionDefNames", LookMode.Value);
            Scribe_Collections.Look(ref selectedXenotypeDefNames, "selectedXenotypeDefNames", LookMode.Value);
            Scribe_Values.Look(ref overrideTunnelXenotype, "overrideTunnelXenotype", false);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

            if (selectedFactionDefNames == null) selectedFactionDefNames = new HashSet<string>();
            if (selectedXenotypeDefNames == null) selectedXenotypeDefNames = new HashSet<string>();
            selectedFactionDefNames.RemoveWhere(string.IsNullOrWhiteSpace);
            selectedXenotypeDefNames.RemoveWhere(string.IsNullOrWhiteSpace);
        }

        public static bool RemoveMissingDefs()
        {
            int removed = selectedFactionDefNames.RemoveWhere(name => DefDatabase<FactionDef>.GetNamedSilentFail(name) == null);
            removed += selectedXenotypeDefNames.RemoveWhere(name => DefDatabase<XenotypeDef>.GetNamedSilentFail(name) == null);
            if (overrideTunnelXenotype && !TunnelRaidEligibility.TryGetOverrideXenotype(out _))
            {
                overrideTunnelXenotype = false;
                removed++;
            }
            if (removed > 0)
            {
                Logger.Message("Removed " + removed + " saved setting(s) that referenced missing defs");
            }
            return removed > 0;
        }
    }
}
