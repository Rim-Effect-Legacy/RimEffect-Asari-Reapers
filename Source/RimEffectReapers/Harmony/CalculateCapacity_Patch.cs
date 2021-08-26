using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectReapers.Harmony
{
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [HarmonyPatch]
    public static class CalculateCapacity_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(PawnCapacityWorker_Consciousness), nameof(PawnCapacityWorker_Consciousness.CalculateCapacityLevel));
            yield return AccessTools.Method(typeof(PawnCapacityWorker_Manipulation),  nameof(PawnCapacityWorker_Manipulation.CalculateCapacityLevel));
            yield return AccessTools.Method(typeof(PawnCapacityWorker_Moving),        nameof(PawnCapacityWorker_Moving.CalculateCapacityLevel));
        }

        [HarmonyPostfix]
        public static void Postfix(HediffSet diffSet, ref float __result)
        {
            if (__result > 0 && diffSet.pawn.def == RER_DefOf.RE_Husk_Brute)
                __result = Mathf.Lerp(2f, 1f, diffSet.pawn.health.summaryHealth.SummaryHealthPercent);
        }
    }
}
