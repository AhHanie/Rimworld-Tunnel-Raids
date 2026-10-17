using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Tunnel_Raids
{
    public static class TunnelRaidEligibility
    {
        public const string IssuePlayerFaction = "TunnelRaids.FactionIssue.PlayerFaction";

        public const string IssueNotHumanlike = "TunnelRaids.FactionIssue.NotHumanlike";

        public const string IssueRaidsForbidden = "TunnelRaids.FactionIssue.RaidsForbidden";

        public const string IssueNoCombatGroups = "TunnelRaids.FactionIssue.NoCombatGroups";

        public static bool XenotypeFilterActive =>
            ModsConfig.BiotechActive && ModSettings.selectedXenotypeDefNames.Count > 0;

        public static bool TryGetSingleCombatXenotype(out XenotypeDef xenotype)
        {
            xenotype = null;
            if (!ModsConfig.BiotechActive || ModSettings.selectedXenotypeDefNames.Count != 1)
            {
                return false;
            }
            foreach (string name in ModSettings.selectedXenotypeDefNames)
            {
                xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(name);
            }
            return xenotype != null && xenotype.canGenerateAsCombatant;
        }

        public static bool TryGetOverrideXenotype(out XenotypeDef xenotype)
        {
            xenotype = null;
            return ModSettings.overrideTunnelXenotype && TryGetSingleCombatXenotype(out xenotype);
        }

        public static bool IsFactionAllowed(Faction faction)
        {
            return faction == null || IsFactionDefAllowed(faction.def, faction);
        }

        public static bool IsFactionDefAllowed(FactionDef def, Faction liveFaction)
        {
            if (ModSettings.selectedFactionDefNames.Count > 0 && !ModSettings.selectedFactionDefNames.Contains(def.defName))
            {
                return false;
            }
            if (TryGetOverrideXenotype(out _))
            {
                return true;
            }
            if (!XenotypeFilterActive)
            {
                return true;
            }
            return NaturallyGeneratesAny(def, liveFaction, ModSettings.selectedXenotypeDefNames);
        }

        public static bool NaturallyGeneratesAny(FactionDef def, Faction liveFaction, HashSet<string> xenotypeDefNames)
        {
            if (!ModsConfig.BiotechActive || def.pawnGroupMakers == null)
            {
                return false;
            }
            for (int i = 0; i < def.pawnGroupMakers.Count; i++)
            {
                PawnGroupMaker maker = def.pawnGroupMakers[i];
                if (maker.kindDef != PawnGroupKindDefOf.Combat || maker.options == null)
                {
                    continue;
                }
                for (int j = 0; j < maker.options.Count; j++)
                {
                    PawnGenOption option = maker.options[j];
                    if (option.kind == null || option.selectionWeight <= 0f || !option.kind.RaceProps.Humanlike)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<XenotypeDef, float> pair in PawnGenerator.XenotypesAvailableFor(option.kind, def, liveFaction))
                    {
                        if (pair.Value > 0f && pair.Key.canGenerateAsCombatant && xenotypeDefNames.Contains(pair.Key.defName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static string GetFactionDefIssue(FactionDef def)
        {
            if (def.isPlayer)
            {
                return IssuePlayerFaction;
            }
            if (!def.humanlikeFaction)
            {
                return IssueNotHumanlike;
            }
            if (def.raidsForbidden)
            {
                return IssueRaidsForbidden;
            }
            if (def.pawnGroupMakers == null || !def.pawnGroupMakers.Exists(maker => maker.kindDef == PawnGroupKindDefOf.Combat))
            {
                return IssueNoCombatGroups;
            }
            return null;
        }

        public static Faction LiveFactionOf(FactionDef def)
        {
            if (Current.ProgramState != ProgramState.Playing || Find.FactionManager == null)
            {
                return null;
            }
            return Find.FactionManager.FirstFactionOfDef(def);
        }
    }
}
