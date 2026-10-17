using HarmonyLib;
using UnityEngine;
using Verse;

namespace Tunnel_Raids
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "TunnelRaids.LoadingLabel", doAsynchronously: true, null);
        }

        private void Init()
        {
            GetSettings<ModSettings>();
            if (ModSettings.RemoveMissingDefs())
            {
                LongEventHandler.ExecuteWhenFinished(WriteSettings);
            }
            new Harmony("sk.tunnelraids").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "TunnelRaids.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            ModSettingsWindow.overrideDisabledNotice = false;
            base.WriteSettings();
        }
    }
}
