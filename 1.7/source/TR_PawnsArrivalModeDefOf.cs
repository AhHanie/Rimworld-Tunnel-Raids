using RimWorld;

namespace Tunnel_Raids
{
    [DefOf]
    public static class TR_PawnsArrivalModeDefOf
    {
        public static PawnsArrivalModeDef TR_TunnelEmerge;

        static TR_PawnsArrivalModeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TR_PawnsArrivalModeDefOf));
        }
    }
}
