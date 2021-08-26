using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectReapers.Harmony
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.InPainShock), MethodType.Getter)]
    public static class HealthTrackerPainShock_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn.def == RER_DefOf.RE_Husk_Brute)
                __result = false;
        }
    }
}
