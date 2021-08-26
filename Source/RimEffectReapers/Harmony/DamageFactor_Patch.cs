using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectReapers.Harmony
{
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.GetDamageFactorFor), typeof(Tool), typeof(Pawn), typeof(HediffComp_VerbGiver))]
    public static class DamageFactor_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn attacker, ref float __result)
        {
            if (attacker.def == RER_DefOf.RE_Husk_Brute)
                __result *= Mathf.Lerp(2f, 1f, attacker.health.summaryHealth.SummaryHealthPercent);
        }
    }
}