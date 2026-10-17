using RimWorld;
using Verse;

namespace Tunnel_Raids
{
    [DefOf]
    public static class TR_ThingDefOf
    {
        public static ThingDef TR_TunnelRaidSpawner;

        static TR_ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TR_ThingDefOf));
        }
    }
}
