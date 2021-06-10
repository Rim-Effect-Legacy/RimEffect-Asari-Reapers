using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class Building_Reaper_UnitStorage : Building_Reaper
    {
        private bool released;

        public bool CanRelease => !released && Map != null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref released, "released");
        }

        public void Release()
        {
            var pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
            {
                tile = Map.Tile,
                faction = Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers),
                points = 2000f,
                groupKind = PawnGroupKindDefOf.Combat
            });
            foreach (var pawn in pawns)
            {
                GenPlace.TryPlaceThing(pawn, Position, Map, ThingPlaceMode.Near);
                ReaperUtils.CreateOrAddToAssaultLord(pawn);
            }

            released = true;
        }
    }
}