using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    [StaticConstructorOnStartup]
    public static class ReaperRotTweak
    {
        static ReaperRotTweak()
        {
            foreach(ThingDef thingdef in DefDatabase<ThingDef>.AllDefs)
            {
                ThingDef corpseDef = thingdef?.race?.corpseDef;
                if(corpseDef != null)
                {
                    if (thingdef?.race?.FleshType == RER_DefOf.RE_Husk)
                    {
                        corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_Rottable);
                        corpseDef.comps.RemoveAll(compProperties => compProperties is CompProperties_SpawnerFilth);
                    }
                }
            }
        }
    }
}
