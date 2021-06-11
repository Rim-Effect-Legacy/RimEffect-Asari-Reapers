using Verse;

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
}