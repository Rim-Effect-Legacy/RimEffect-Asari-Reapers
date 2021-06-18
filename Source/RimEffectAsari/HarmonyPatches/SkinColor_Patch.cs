namespace RimEffectAsari
{
    using AlienRace;
    using HarmonyLib;
    using UnityEngine;
    using Verse;

    [HarmonyPatch(typeof(AlienPartGenerator), nameof(AlienPartGenerator.SkinColor))]
    public static class SkinColor_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref Color __result, Pawn alien)
        {
            if (alien.health.hediffSet.HasHediff(REA_DefOf.RE_Biotic_StasisHediff))
                __result = Color.white;
        }
    }
}