using HarmonyLib;
using Verse;
using Verse.AI.Group;

namespace RimEffectReapers
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public static HarmonyLib.Harmony Harm;

        static HarmonyInit()
        {
            Harm = new HarmonyLib.Harmony("OskarPotocki.RimEffectReapers");
            Harm.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Trigger_UrgentlyHungry), nameof(Trigger_UrgentlyHungry.ActivateOn))]
    public class Trigger_UrgentlyHungry_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Lord lord, ref bool __result)
        {
            if (lord.faction.def != RER_DefOf.RE_Reapers) return true;
            __result = false;
            return false;
        }
    }
}