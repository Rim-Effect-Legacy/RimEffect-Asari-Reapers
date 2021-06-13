using RimWorld;
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
    }
}