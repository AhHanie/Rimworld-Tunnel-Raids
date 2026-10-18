using RimWorld;

namespace Tunnel_Raids
{
    [DefOf]
    public static class TR_PawnsArrivalModeDefOf
    {
        public static PawnsArrivalModeDef TR_TunnelEmerge;

        [MayRequireBiotech]
        public static PawnsArrivalModeDef TR_MechTunnelEmerge;

        static TR_PawnsArrivalModeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TR_PawnsArrivalModeDefOf));
        }
    }
}
