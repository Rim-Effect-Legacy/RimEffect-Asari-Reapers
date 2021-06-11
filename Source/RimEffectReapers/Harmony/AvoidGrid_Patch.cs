using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace RimEffectReapers.Harmony
{
    [StaticConstructorOnStartup]
    internal class AvoidGrid_Patch
    {
        static AvoidGrid_Patch()
        {
            if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Mechanoids"))
            {
                var type = AccessTools.TypeByName("VFEMech.AvoidGrid_Patch");
                HarmonyInit.Harm.Patch(AccessTools.Method(type, "AvoidCover"),
                    new HarmonyMethod(typeof(AvoidGrid_Patch), nameof(AvoidCover_Prefix)));
            }
            else
            {
                HarmonyInit.Harm.Patch(AccessTools.Method(typeof(CastPositionFinder), "CastPositionPreference"),
                    transpiler: new HarmonyMethod(typeof(AvoidGrid_Patch), nameof(Transpiler)));
            }
        }

        public static bool AvoidCover_Prefix(Pawn pawn, ref bool __result)
        {
            if (pawn.Faction?.def == RER_DefOf.RE_Reapers &&
                (pawn.health?.hediffSet?.HasHediff(RER_DefOf.RER_DirectControl) ?? false))
            {
                __result = false;
                return false;
            }

            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();

            var avoidCoverInfo = AccessTools.Field(typeof(PawnKindDef), nameof(PawnKindDef.aiAvoidCover));

            var done = false;

            for (var i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                yield return instruction;

                if (!done && instructionList[i + 2].OperandIs(avoidCoverInfo))
                {
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(AvoidGrid_Patch), nameof(AvoidCover)));
                    i += 2;
                    done = true;
                }
            }
        }

        public static bool AvoidCover(Pawn pawn)
        {
            if (pawn.Faction?.def == RER_DefOf.RE_Reapers &&
                (pawn.health?.hediffSet?.HasHediff(RER_DefOf.RER_DirectControl) ?? false))
                return false;

            return pawn.kindDef.aiAvoidCover;
        }
    }
}