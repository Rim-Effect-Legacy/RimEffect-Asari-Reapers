using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

using HarmonyLib;

namespace RimEffectReapers.Harmony
{

    [HarmonyPatch(typeof(ThingDef), "IsIngestible", MethodType.Getter)]
    public class ThingDefIsIngestible_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, ref ThingDef __instance)
        {
            if (__instance.race?.FleshType == RER_DefOf.RE_Husk)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
