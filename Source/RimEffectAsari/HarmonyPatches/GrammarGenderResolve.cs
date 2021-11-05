namespace RimEffectAsari
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using RimWorld;
    using Verse;
    using Verse.Grammar;

    [HarmonyPatch]
    public static class GrammarGenderResolve
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GrammarResolverSimple), "TryResolveSymbol");
            yield return AccessTools.Method(typeof(GrammarUtility), nameof(GrammarUtility.RulesForPawn), new Type[] { typeof(string), typeof(Pawn), typeof(Dictionary<string, string>), typeof(bool), typeof(bool) });
            foreach (MethodInfo mi in AccessTools.GetDeclaredMethods(typeof(GrammarResolver)).Where(predicate: mi => mi.GetParameters().Any(predicate: pi => pi.ParameterType == typeof(Pawn))))
                yield return mi;
            yield return AccessTools.Method(typeof(TraitDegreeData), nameof(TraitDegreeData.GetLabelFor), new Type[] { typeof(Pawn) });
            yield return AccessTools.Method(typeof(RoyalTitleDef),   nameof(RoyalTitleDef.GetLabelFor),   new Type[] { typeof(Pawn)});
            foreach (MethodInfo mi in AccessTools.GetDeclaredMethods(typeof(PawnRelationDef)).Where(predicate: mi => mi.GetParameters().Any(predicate: pi => pi.ParameterType == typeof(Pawn))))
                yield return mi;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            FieldInfo genderInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

            List<CodeInstruction> instructionList = instructions.ToList();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsField(genderInfo))
                {
                    //pawn.def == REA_DefOf.RE_Asari ? Gender.Female : pawn.gender;

                    Label label  = ilg.DefineLabel();
                    Label label2 = ilg.DefineLabel();

                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return CodeInstruction.LoadField(typeof(Pawn), nameof(Pawn.def));
                    yield return CodeInstruction.LoadField(typeof(REA_DefOf), nameof(REA_DefOf.RE_Asari));
                    yield return new CodeInstruction(OpCodes.Beq, label);
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Br,       label2);
                    yield return new CodeInstruction(OpCodes.Pop).WithLabels(label);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(label2);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}