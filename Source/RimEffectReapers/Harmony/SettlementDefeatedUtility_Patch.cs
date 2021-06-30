using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimEffectReapers.Harmony
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated")]
    [HarmonyAfter("vanillaexpanded.achievements")]
    internal class SettlementDefeatedUtility_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Map map, Faction faction, ref bool __result)
        {
            if (map.mapPawns.SpawnedPawnsInFaction(faction).Any(p =>
                p.Faction?.def == RER_DefOf.RE_Reapers && GenHostility.IsActiveThreatToPlayer(p)))
                __result = false;
            else if (map.listerBuildings.allBuildingsNonColonist.Any(b =>
                b.Faction?.def == RER_DefOf.RE_Reapers && b is Building_ReaperTurret ||
                b is Building_Reaper_LongRangeMissile)) __result = false;
        }
    }
}