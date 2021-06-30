using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace RimEffectReapers.Harmony
{
    internal class Raid_Patches
    {
        [HarmonyPatch]
        private static class FactionForCombatGroup_Patch
        {
            [HarmonyTargetMethod]
            public static MethodBase TargetMethod()
            {
                return typeof(PawnGroupMakerUtility).GetNestedTypes(AccessTools.all)
                    .First(t => t.GetMethods(AccessTools.all).Any(mi =>
                        mi.Name.Contains(nameof(PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup)) &&
                        mi.GetParameters()[0].ParameterType == typeof(Faction))).GetMethods(AccessTools.all)
                    .First(mi => mi.ReturnType == typeof(bool));
            }

            [HarmonyPrefix]
            public static bool Prefix(Faction f, ref bool __result)
            {
                if (f.def != RER_DefOf.RE_Reapers || ReaperUtils.ReaperPresence() > 0) return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
        private static class RaidEnemyResolveFaction_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(ref IncidentParms parms)
            {
                if (parms.faction is null)
                    return;
                if (parms.faction.def == RER_DefOf.RE_Reapers && ReaperUtils.ReaperPresence() <= 0)
                    parms.faction = null;
            }

            [HarmonyPostfix]
            public static void Postfix(ref IncidentParms parms)
            {
                if (parms.faction?.def == RER_DefOf.RE_Reapers)
                    parms.points = ReaperUtils.ReaperPresence();
            }
        }
    }
}