namespace RimEffectAsari
{
    using HarmonyLib;
    using Verse;

    [HarmonyPatch(typeof(HediffSet), "CalculateBleedRate")]
    public static class CalculateBleedRate_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__instance.HasHediff(REA_DefOf.RE_Biotic_StasisHediff))
                __result = 0f;
        }
    }
}