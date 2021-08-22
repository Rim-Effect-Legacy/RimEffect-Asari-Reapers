namespace RimEffectAsari
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimEffect;
    using RimWorld;
    using Verse;
    using Verse.AI;

    [HarmonyPatch]
    public static class LovinJobDriver_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            FieldInfo lovinInfo = AccessTools.Field(typeof(ThoughtDefOf), nameof(ThoughtDefOf.GotSomeLovin));

            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(JobDriver_Lovin)))
            {
                IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(method);
                if (instructions.Any(i => i.Value is FieldInfo fi && fi == lovinInfo))
                    return method;
            }

            return null;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo lovinInfo = AccessTools.Field(typeof(ThoughtDefOf), nameof(ThoughtDefOf.GotSomeLovin));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsField(lovinInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn)));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver_Lovin), "Partner"));
                    yield return new CodeInstruction(OpCodes.Call,  AccessTools.Method(typeof(LovinJobDriver_Patch), nameof(GetLovinThought)));
                } else
                    yield return instruction;
            }
        }

        public static ThoughtDef GetLovinThought(Pawn pawn, Pawn partner)
        {
            if (pawn.def == REA_DefOf.RE_Asari || partner.def == REA_DefOf.RE_Asari)
                return REA_DefOf.RE_EmbracingEternity;
            return ThoughtDefOf.GotSomeLovin;
        }

        [HarmonyPostfix]
        public static void Postfix(JobDriver_Lovin __instance, int ___ticksLeft)
        {
            if(__instance.pawn.story.traits.HasTrait(REA_DefOf.RE_ArdatYakshi) && ___ticksLeft <= GenTicks.TicksPerRealSecond)
            {
                Hediff hediff  = __instance.pawn.health.hediffSet.GetFirstHediffOfDef(REA_DefOf.RE_ArdatYakshi_Power) ?? __instance.pawn.health.AddHediff(REA_DefOf.RE_ArdatYakshi_Power);
                hediff.Severity += 1f;
                
                Pawn   partner = ((Pawn)__instance.job.GetTarget(TargetIndex.A));
                partner.health.AddHediff(REA_DefOf.RE_ArdatYakshi_Partner, partner.health.hediffSet.GetBrain()).Severity = 1f;
            }
        }
    }

    [HarmonyPatch]
    public static class LovinJobDriverFleck_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            FieldInfo heartInfo = AccessTools.Field(typeof(FleckDefOf), nameof(FleckDefOf.Heart));

            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(JobDriver_Lovin)))
            {
                IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(method);
                if (instructions.Any(i => i.Value is FieldInfo fi && fi == heartInfo))
                    return method;
            }

            return null;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo  heartInfo = AccessTools.Field(typeof(FleckDefOf), nameof(FleckDefOf.Heart));
            MethodInfo posInfo   = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Position));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(posInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver_Lovin), "Partner"));
                    yield return CodeInstruction.Call(typeof(LovinJobDriverFleck_Patch), nameof(GetBioticPos));
                }
                else if (instruction.LoadsField(heartInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LovinJobDriverFleck_Patch), nameof(GetLovinFleck)));
                }
                else
                    yield return instruction;
            }
        }

        public static IntVec3 GetBioticPos(Pawn pawn, Pawn partner) => 
            pawn.def == REA_DefOf.RE_Asari ? pawn.Position + ((partner.Position.ToVector3() - pawn.Position.ToVector3())/2).ToIntVec3() : pawn.Position;

        public static FleckDef GetLovinFleck(Pawn pawn)
        {
            if (pawn.def == REA_DefOf.RE_Asari) 
                return REA_DefOf.RE_Fleck_EmbracingEternity;

            return FleckDefOf.Heart;
        }
    }

    [HarmonyPatch(typeof(JobDriver), "ReportStringProcessed")]
    public static class JobDriverReportString_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(JobDriver __instance, ref string str)
        {
            if(__instance.job.def == JobDefOf.Lovin)
                if (__instance.pawn.def == REA_DefOf.RE_Asari || ((Pawn) __instance.job.GetTarget(TargetIndex.A)).def == REA_DefOf.RE_Asari)
                    str = "RE_AsariLovinReport".Translate();
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
    public static class SecondaryLovinChanceFactor_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            FieldInfo bisexualInfo = AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Bisexual));

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;

                if (i > 5 && instructionList[i - 2].LoadsField(bisexualInfo))
                {
                    yield return new CodeInstruction(instructionList[i - 6]);
                    yield return new CodeInstruction(instructionList[i - 5]);
                    yield return CodeInstruction.LoadField(typeof(Pawn), nameof(Pawn.def));
                    yield return CodeInstruction.LoadField(typeof(REA_DefOf), nameof(REA_DefOf.RE_Asari));
                    yield return new CodeInstruction(OpCodes.Beq, instruction.operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.LoadField(typeof(Pawn),      nameof(Pawn.def));
                    yield return CodeInstruction.LoadField(typeof(REA_DefOf), nameof(REA_DefOf.RE_Asari));
                    yield return new CodeInstruction(OpCodes.Beq, instruction.operand);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            if ((___pawn.def == REA_DefOf.RE_Asari || otherPawn.def == REA_DefOf.RE_Asari) && ___pawn.def != otherPawn.def)
                __result *= 2f;

            if (___pawn.story.traits.HasTrait(REA_DefOf.RE_ArdatYakshi))
                __result *= 10f;
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith))]
    public static class CompatibilityWith_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            if ((___pawn.def == REA_DefOf.RE_Asari || otherPawn.def == REA_DefOf.RE_Asari) && ___pawn.def != otherPawn.def)
                __result += 1f;

            if (___pawn.story.traits.HasTrait(REA_DefOf.RE_ArdatYakshi))
                __result += 10f;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Lovin), "GenerateRandomMinTicksToNextLovin")]
    public static class TicksToNextLovin_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref int __result) => 
            __result *= 3;
    }

    [HarmonyPatch]
    public static class LovinJobDriverInit_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            MethodInfo info = AccessTools.Method(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob));

            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(JobDriver_Lovin)))
            {
                IEnumerable<KeyValuePair<OpCode, object>> instructions = PatchProcessor.ReadMethodBody(method);
                if (instructions.Any(i => i.Value is MethodInfo mi && mi == info))
                    return method;
            }

            return null;
        }
        
        [HarmonyPostfix]
        public static void Postfix(JobDriver __instance)
        {
            if (__instance.pawn.story.traits.HasTrait(REA_DefOf.RE_ArdatYakshi))
            {
                Pawn partner = ((Pawn)__instance.job.GetTarget(TargetIndex.A));
                Messages.Message("RE_ArdatYakshiDeathIncoming".Translate(__instance.pawn.Named("ARDATYAKSHI"), partner.Named("VICTIM")), __instance.pawn, MessageTypeDefOf.ThreatSmall);
            }
        }
    }
}
