using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimEffectReapers
{
    public static class ReaperUtils
    {
        public static void CreateOrAddToAssaultLord(Pawn pawn, Lord lord = null, bool canKidnap = false,
            bool canTimeoutOrFlee = false, bool sappers = false,
            bool useAvoidGridSmart = false, bool canSteal = false)
        {
            if (lord == null && pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any(p => p != pawn))
                lord = ((Pawn) GenClosest.ClosestThing_Global(pawn.Position,
                    pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f,
                    p => p != pawn && ((Pawn) p).GetLord() != null)).GetLord();

            if (lord == null)
            {
                var lordJob = new LordJob_AssaultColony(pawn.Faction, canKidnap, canTimeoutOrFlee, sappers,
                    useAvoidGridSmart, canSteal);
                lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map);
            }

            lord.AddPawn(pawn);
        }

        public static int ReaperPresence() => 
            (int)Find.World.worldObjects.Settlements.Sum(s => s.def.GetModExtension<ReaperBaseExtension>()?.raisesPresence ?? 0f) + 1;

        public static bool HasAnyOtherBase(Settlement defeatedFactionBase)
        {
            var settlements = Find.WorldObjects.Settlements;
            for (var i = 0; i < settlements.Count; i++)
            {
                var settlement = settlements[i];
                if (settlement.Faction == defeatedFactionBase.Faction && settlement != defeatedFactionBase) return true;
            }

            return false;
        }
    }
}