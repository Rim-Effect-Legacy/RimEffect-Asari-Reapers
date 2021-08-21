namespace RimEffectAsari
{
    using HarmonyLib;
    using RimWorld;
    using Verse;

    /*
    [HarmonyPatch(typeof(RecruitUtility), nameof(RecruitUtility.RecruitChanceFinalByPawn))]
    public static class RecruitChanceFinal_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn recruitee, Pawn recruiter, ref float __result)
        {
            if (recruiter.health?.hediffSet.HasHediff(REA_DefOf.RE_Biotic_CharmHediff) ?? false) 
                __result = 1f;
        }
    }*/

    [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.Interacted))]
    public static class RecruitInteractionWorker_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn initiator, Pawn recipient)
        {
            if (initiator.health?.hediffSet.HasHediff(REA_DefOf.RE_Biotic_CharmHediff) ?? false)
                recipient.guest.resistance = 0f;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            Hediff def = initiator.health.hediffSet.GetFirstHediffOfDef(REA_DefOf.RE_Biotic_CharmHediff);
            if (def != null)
            {
                initiator.health.hediffSet.hediffs.Remove(def);
                recipient.health?.AddHediff(REA_DefOf.RE_Biotic_CharmedHediff);
                recipient.needs.mood?.thoughts.memories.TryGainMemory(REA_DefOf.RE_CharmThought, initiator);
            }
        }
    }
}
