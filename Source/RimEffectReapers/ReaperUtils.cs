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

        public static int ReaperPresence()
        {
            return Find.WorldObjects.Settlements.Count(s =>
                s.Faction.def == RER_DefOf.RE_Reapers && s.def == RER_DefOf.RE_Reaper) * 2000;
        }

        public static void HeightWidthFromLayout(StructureLayoutDef structureLayoutDef, out int height, out int width)
        {
            if (structureLayoutDef == null || structureLayoutDef.layouts.Count == 0)
            {
                Log.Warning("StructureLayoutDef was null or empty. Throwing 10 10 size");
                height = 10;
                width = 10;
                return;
            }

            height = structureLayoutDef.layouts[0].Count;
            width = structureLayoutDef.layouts[0][0].Split(',').ToList().Count;
        }

        public static Dictionary<string, SymbolDef> FillpairsSymbolLabel()
        {
            var pairsSymbolLabel = new Dictionary<string, SymbolDef>();
            var symbolDefs = DefDatabase<SymbolDef>.AllDefsListForReading;
            foreach (var s in symbolDefs) pairsSymbolLabel.Add(s.symbol, s);

            return pairsSymbolLabel;
        }

        public static IntVec3 FindRect(Map map, int maxTries)
        {
            var c = 0;
            while (true)
            {
                var rect = CellRect.CenteredOn(CellFinder.RandomNotEdgeCell(33, map), 33, 33);
                if (!rect.Cells.ToList().Any(i =>
                    !i.Walkable(map) || !i.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Medium)))
                    return rect.CenterCell;

                if (c > maxTries) return IntVec3.Invalid;
                c++;
            }
        }

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