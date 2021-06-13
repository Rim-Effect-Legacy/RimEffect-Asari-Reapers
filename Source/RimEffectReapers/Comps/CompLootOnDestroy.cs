using System.Linq;
using UnityEngine;
using Verse;

namespace RimEffectReapers
{
    public class CompProperties_LootOnDestroy : CompProperties
    {
        public int amount;
        public int maxValuePerItem;

        public CompProperties_LootOnDestroy()
        {
            compClass = typeof(CompLootOnDestroy);
        }
    }

    public class CompLootOnDestroy : ThingComp
    {
        public CompProperties_LootOnDestroy Props => props as CompProperties_LootOnDestroy;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            var numToPlace = Props.amount;
            while (numToPlace > 0)
            {
                var resource = ThingMaker.MakeThing(DefDatabase<ThingDef>.AllDefs.Where(def =>
                    def.IsStuff && def.BaseMarketValue <= Props.maxValuePerItem).RandomElement());
                numToPlace -= resource.stackCount = Mathf.Max(resource.def.stackLimit, numToPlace);
                GenPlace.TryPlaceThing(resource, parent.Position, previousMap, ThingPlaceMode.Near);
            }

            base.PostDestroy(mode, previousMap);
        }
    }
}