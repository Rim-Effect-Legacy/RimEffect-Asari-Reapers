namespace RimEffectAsari
{
    using HarmonyLib;
    using Verse;

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit() => 
            new Harmony("OskarPotocki.RimEffectAsari").PatchAll();
    }
}
