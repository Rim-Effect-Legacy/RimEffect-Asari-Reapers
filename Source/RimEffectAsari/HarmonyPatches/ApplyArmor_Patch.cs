namespace RimEffectAsari
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    public static class ApplyArmor_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float armorRating, Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(REA_DefOf.RE_Biotic_WarpHediff))
                armorRating = 0f;
        }
    }
}
