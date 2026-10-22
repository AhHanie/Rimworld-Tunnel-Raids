using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tunnel_Raids
{
    public enum MessageSeverity
    {
        Error,
        Warning,
        Info
    }

    public struct SettingsMessage
    {
        public MessageSeverity severity;

        public string text;
    }

    public static class ModSettingsWindow
    {
        public const float RowHeight = 26f;

        public const float HeaderHeight = 32f;

        public const float MessagesHeight = 150f;

        public const float Gap = 12f;

        public static Vector2 factionScrollPosition = Vector2.zero;

        public static Vector2 xenotypeScrollPosition = Vector2.zero;

        public static Vector2 messageScrollPosition = Vector2.zero;

        public static List<FactionDef> sortedFactions;

        public static List<XenotypeDef> sortedXenotypes;

        public static List<SettingsMessage> messages = new List<SettingsMessage>();

        public static bool messagesDirty = true;

        public static int messagesStateKey;

        public static bool overrideDisabledNotice = false;

        public static void Draw(Rect inRect)
        {
            EnsureSortedLists();
            RefreshMessagesIfNeeded();

            float columnsTop = DrawTunnelOptions(inRect);

            Rect messagesRect = new Rect(inRect.x, inRect.yMax - MessagesHeight, inRect.width, MessagesHeight);
            Rect topRect = new Rect(inRect.x, columnsTop, inRect.width, Mathf.Max(0f, messagesRect.y - Gap - columnsTop));
            float columnWidth = (topRect.width - Gap) / 2f;
            Rect leftRect = new Rect(topRect.x, topRect.y, columnWidth, topRect.height);
            Rect rightRect = new Rect(leftRect.xMax + Gap, topRect.y, columnWidth, topRect.height);

            DrawFactionColumn(leftRect);
            DrawXenotypeColumn(rightRect);
            DrawMessages(messagesRect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private static void EnsureSortedLists()
        {
            if (sortedFactions == null || sortedFactions.Count != DefDatabase<FactionDef>.DefCount)
            {
                sortedFactions = DefDatabase<FactionDef>.AllDefsListForReading
                    .OrderBy(DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(def => def.defName, StringComparer.Ordinal)
                    .ToList();
                messagesDirty = true;
            }
            if (sortedXenotypes == null || sortedXenotypes.Count != DefDatabase<XenotypeDef>.DefCount)
            {
                sortedXenotypes = DefDatabase<XenotypeDef>.AllDefsListForReading
                    .OrderBy(DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ThenBy(def => def.defName, StringComparer.Ordinal)
                    .ToList();
                messagesDirty = true;
            }
        }

        private static string DisplayName(Def def)
        {
            return def.label.NullOrEmpty() ? def.defName : def.LabelCap.Resolve();
        }

        private static float DrawTunnelOptions(Rect inRect)
        {
            float y = DrawMechanoidSection(inRect);
            y = DrawTerrainCheckbox(inRect, y, ref ModSettings.blockMuddyTunnelSites, "TunnelRaids.Settings.BlockMuddyLabel", "TunnelRaids.Settings.BlockMuddyTooltip");
            y = DrawTerrainCheckbox(inRect, y, ref ModSettings.blockRockyTunnelSites, "TunnelRaids.Settings.BlockRockyLabel", "TunnelRaids.Settings.BlockRockyTooltip");
            return y + Gap;
        }

        private static float DrawTerrainCheckbox(Rect inRect, float y, ref bool setting, string labelKey, string tooltipKey)
        {
            Rect rect = new Rect(inRect.x, y, inRect.width, RowHeight);
            Widgets.CheckboxLabeled(rect, labelKey.Translate(), ref setting);
            TooltipHandler.TipRegion(rect, tooltipKey.Translate());
            return rect.yMax + 4f;
        }

        private static float DrawMechanoidSection(Rect inRect)
        {
            bool biotech = ModsConfig.BiotechActive;
            Rect checkboxRect = new Rect(inRect.x, inRect.y, inRect.width, RowHeight);
            bool value = ModSettings.enableMechanoidTunnelRaids;
            Widgets.CheckboxLabeled(checkboxRect, "TunnelRaids.Settings.MechanoidLabel".Translate(), ref value, disabled: !biotech);
            TooltipHandler.TipRegion(checkboxRect, "TunnelRaids.Settings.MechanoidTooltip".Translate());
            if (biotech)
            {
                ModSettings.enableMechanoidTunnelRaids = value;
            }

            float y = checkboxRect.yMax + 4f;
            if (!biotech)
            {
                y += DrawHint(new Rect(inRect.x, y, inRect.width, 0f), "TunnelRaids.Settings.MechanoidBiotechRequired".Translate());
            }
            return y;
        }

        private static void DrawFactionColumn(Rect rect)
        {
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, "TunnelRaids.Settings.FactionsHeading".Translate());
            Text.Font = GameFont.Small;

            float y = rect.y + HeaderHeight;
            y += DrawHint(new Rect(rect.x, y, rect.width, 0f), "TunnelRaids.Settings.FactionsHint".Translate());

            Rect listRect = new Rect(rect.x, y, rect.width, rect.yMax - y);
            DrawList(listRect, sortedFactions, ModSettings.selectedFactionDefNames, ref factionScrollPosition,
                def => TunnelRaidEligibility.GetFactionDefIssue((FactionDef)def), OnFactionSelectionChanged);
        }

        private static void DrawXenotypeColumn(Rect rect)
        {
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, "TunnelRaids.Settings.XenotypesHeading".Translate());
            Text.Font = GameFont.Small;

            float y = rect.y + HeaderHeight;
            if (!ModsConfig.BiotechActive)
            {
                DrawHint(new Rect(rect.x, y, rect.width, 0f), "TunnelRaids.Settings.BiotechRequired".Translate());
                return;
            }

            bool overrideOn = TunnelRaidEligibility.TryGetOverrideXenotype(out _);
            string hintKey = overrideOn ? "TunnelRaids.Settings.XenotypesOverrideHint" : "TunnelRaids.Settings.XenotypesHint";
            y += DrawHint(new Rect(rect.x, y, rect.width, 0f), hintKey.Translate());

            DrawOverrideCheckbox(new Rect(rect.x, y, rect.width, RowHeight));
            y += RowHeight + 4f;

            Rect listRect = new Rect(rect.x, y, rect.width, rect.yMax - y);
            DrawList(listRect, sortedXenotypes, ModSettings.selectedXenotypeDefNames, ref xenotypeScrollPosition,
                def => ((XenotypeDef)def).canGenerateAsCombatant ? null : "TunnelRaids.XenotypeIssue.NotCombatant", OnXenotypeSelectionChanged);
        }

        private static float DrawHint(Rect rect, string text)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            rect.height = Text.CalcHeight(text, rect.width);
            Widgets.Label(rect, text);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            return rect.height + 4f;
        }

        private static void DrawOverrideCheckbox(Rect rect)
        {
            bool canOverride = TunnelRaidEligibility.TryGetSingleCombatXenotype(out _);
            if (!canOverride && ModSettings.overrideTunnelXenotype)
            {
                ModSettings.overrideTunnelXenotype = false;
                messagesDirty = true;
            }

            bool value = ModSettings.overrideTunnelXenotype;
            bool before = value;
            Widgets.CheckboxLabeled(rect, "TunnelRaids.Settings.OverrideLabel".Translate(), ref value, disabled: !canOverride);
            TooltipHandler.TipRegion(rect, (canOverride ? "TunnelRaids.Settings.OverrideTooltip" : "TunnelRaids.Settings.OverrideDisabledTooltip").Translate());
            if (value != before)
            {
                ModSettings.overrideTunnelXenotype = value;
                overrideDisabledNotice = false;
                messagesDirty = true;
            }
        }

        private static void OnFactionSelectionChanged()
        {
            messagesDirty = true;
        }

        private static void OnXenotypeSelectionChanged()
        {
            messagesDirty = true;
            if (ModSettings.selectedXenotypeDefNames.Count == 1)
            {
                overrideDisabledNotice = false;
            }
            else if (ModSettings.overrideTunnelXenotype)
            {
                ModSettings.overrideTunnelXenotype = false;
                overrideDisabledNotice = true;
            }
        }

        private static void DrawList<T>(Rect rect, List<T> defs, HashSet<string> selected, ref Vector2 scrollPosition, Func<Def, string> issueOf, Action onChanged) where T : Def
        {
            Widgets.DrawMenuSection(rect);
            Rect outRect = rect.ContractedBy(4f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, defs.Count * RowHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            int first = Mathf.Max(0, (int)(scrollPosition.y / RowHeight));
            int last = Mathf.Min(defs.Count, (int)((scrollPosition.y + outRect.height) / RowHeight) + 1);
            for (int i = first; i < last; i++)
            {
                DrawRow(new Rect(0f, i * RowHeight, viewRect.width, RowHeight), defs[i], selected, issueOf, onChanged);
            }

            Widgets.EndScrollView();
        }

        private static void DrawRow(Rect row, Def def, HashSet<string> selected, Func<Def, string> issueOf, Action onChanged)
        {
            bool isSelected = selected.Contains(def.defName);
            string issue = issueOf(def);

            Widgets.DrawHighlightIfMouseover(row);
            Widgets.CheckboxDraw(row.x + 2f, row.y + 1f, isSelected, false);

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;

            Rect labelRect = new Rect(row.x + 32f, row.y, row.width * 0.6f - 32f, row.height);
            GUI.color = issue == null ? Color.white : Color.gray;
            Widgets.Label(labelRect, DisplayName(def).Truncate(labelRect.width));

            Rect defNameRect = new Rect(labelRect.xMax, row.y, row.xMax - labelRect.xMax, row.height);
            GUI.color = new Color(1f, 1f, 1f, 0.45f);
            Text.Font = GameFont.Tiny;
            Widgets.Label(defNameRect, def.defName.Truncate(defNameRect.width));

            Text.Font = GameFont.Small;
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            TooltipHandler.TipRegion(row, issue == null ? def.defName : def.defName + "\n\n" + issue.Translate());
            if (Widgets.ButtonInvisible(row))
            {
                if (isSelected)
                {
                    selected.Remove(def.defName);
                }
                else
                {
                    selected.Add(def.defName);
                }
                onChanged();
            }
        }

        private static void RefreshMessagesIfNeeded()
        {
            int key = HashCode.Combine(
                (int)Current.ProgramState,
                ModsConfig.BiotechActive,
                DefDatabase<FactionDef>.DefCount,
                DefDatabase<XenotypeDef>.DefCount,
                Current.ProgramState == ProgramState.Playing && Find.FactionManager != null ? Find.FactionManager.AllFactionsListForReading.Count : 0);
            if (!messagesDirty && key == messagesStateKey)
            {
                return;
            }
            messagesStateKey = key;
            messagesDirty = false;
            RebuildMessages();
        }

        private static void RebuildMessages()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var infos = new List<string>();

            bool playing = Current.ProgramState == ProgramState.Playing;
            bool xenotypeFilter = TunnelRaidEligibility.XenotypeFilterActive;
            bool overrideOn = TunnelRaidEligibility.TryGetOverrideXenotype(out _);
            bool naturalMatching = xenotypeFilter && !overrideOn;

            if (ModsConfig.BiotechActive && ModSettings.selectedXenotypeDefNames.Count == 1)
            {
                foreach (string name in ModSettings.selectedXenotypeDefNames)
                {
                    XenotypeDef xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(name);
                    if (xenotype != null && !xenotype.canGenerateAsCombatant)
                    {
                        errors.Add("TunnelRaids.Message.OverrideNotCombatant".Translate(DisplayName(xenotype)));
                    }
                }
            }

            foreach (FactionDef def in sortedFactions.Where(def => ModSettings.selectedFactionDefNames.Contains(def.defName)))
            {
                string issue = TunnelRaidEligibility.GetFactionDefIssue(def);
                if (issue != null)
                {
                    warnings.Add("TunnelRaids.Message.FactionUnusable".Translate(DisplayName(def), issue.Translate()));
                    continue;
                }
                Faction live = TunnelRaidEligibility.LiveFactionOf(def);
                if (playing && live == null)
                {
                    warnings.Add("TunnelRaids.Message.FactionNotInWorld".Translate(DisplayName(def)));
                }
                if (naturalMatching && !TunnelRaidEligibility.NaturallyGeneratesAny(def, live, ModSettings.selectedXenotypeDefNames))
                {
                    errors.Add("TunnelRaids.Message.FactionNoNaturalMatch".Translate(DisplayName(def)));
                }
            }

            if (naturalMatching && ModSettings.selectedFactionDefNames.Count == 0 && !sortedFactions.Any(def => CanTunnelNaturally(def, playing)))
            {
                errors.Add("TunnelRaids.Message.NoFactionMatchesXenotypes".Translate());
            }

            if (overrideDisabledNotice)
            {
                warnings.Add("TunnelRaids.Message.OverrideDisabledMultiple".Translate());
            }

            if (naturalMatching)
            {
                infos.Add("TunnelRaids.Message.XenotypeScopeNotice".Translate());
                if (!playing)
                {
                    infos.Add("TunnelRaids.Message.EstimateNotice".Translate());
                }
            }

            messages.Clear();
            AddMessages(errors, MessageSeverity.Error);
            AddMessages(warnings, MessageSeverity.Warning);
            AddMessages(infos, MessageSeverity.Info);
        }

        private static bool CanTunnelNaturally(FactionDef def, bool playing)
        {
            if (TunnelRaidEligibility.GetFactionDefIssue(def) != null)
            {
                return false;
            }
            Faction live = TunnelRaidEligibility.LiveFactionOf(def);
            if (playing && live == null)
            {
                return false;
            }
            return TunnelRaidEligibility.NaturallyGeneratesAny(def, live, ModSettings.selectedXenotypeDefNames);
        }

        private static void AddMessages(List<string> texts, MessageSeverity severity)
        {
            foreach (string text in texts)
            {
                messages.Add(new SettingsMessage { severity = severity, text = ("TunnelRaids.Severity." + severity).Translate() + ": " + text });
            }
        }

        private static void DrawMessages(Rect rect)
        {
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 24f);
            Widgets.Label(headerRect, "TunnelRaids.Settings.MessagesHeading".Translate());

            Rect bodyRect = new Rect(rect.x, headerRect.yMax, rect.width, rect.height - headerRect.height);
            Widgets.DrawMenuSection(bodyRect);
            Rect outRect = bodyRect.ContractedBy(4f);
            float width = outRect.width - 16f;

            if (messages.Count == 0)
            {
                GUI.color = Color.gray;
                Widgets.Label(outRect, "TunnelRaids.Settings.NoMessages".Translate());
                GUI.color = Color.white;
                return;
            }

            float totalHeight = 0f;
            foreach (SettingsMessage message in messages)
            {
                totalHeight += Text.CalcHeight(message.text, width) + 2f;
            }

            Rect viewRect = new Rect(0f, 0f, width, totalHeight);
            Widgets.BeginScrollView(outRect, ref messageScrollPosition, viewRect);
            float y = 0f;
            foreach (SettingsMessage message in messages)
            {
                float height = Text.CalcHeight(message.text, width);
                GUI.color = ColorOf(message.severity);
                Widgets.Label(new Rect(0f, y, width, height), message.text);
                y += height + 2f;
            }
            GUI.color = Color.white;
            Widgets.EndScrollView();
        }

        private static Color ColorOf(MessageSeverity severity)
        {
            switch (severity)
            {
                case MessageSeverity.Error:
                    return ColorLibrary.RedReadable;
                case MessageSeverity.Warning:
                    return ColorLibrary.Yellow;
                default:
                    return Color.gray;
            }
        }
    }
}
