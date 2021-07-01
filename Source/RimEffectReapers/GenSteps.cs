using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RimEffectReapers
{
    public class GenStep_Reaperify : GenStep
    {
        public override int SeedPart => 8476135;

        public override void Generate(Map map, GenStepParams parms)
        {
            var reapers = Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers);
            foreach (var pawn in map.mapPawns.AllPawnsSpawned.ToList())
                if (pawn.RaceProps.FleshType != RER_DefOf.RE_Husk)
                    try
                    {
                        GenSpawn.Spawn(PawnGenerator.GeneratePawn(reapers.RandomPawnKind(), reapers), pawn.Position,
                            map, pawn.Rotation);
                    }
                    catch (NullReferenceException e)
                    {
                    }
                    finally
                    {
                        pawn.Destroy();
                    }

            var lord = map.mapPawns.AllPawnsSpawned.Select(pawn => pawn.GetLord())
                .First(l => l?.LordJob is LordJob_DefendBase);
            if (lord.LordJob is LordJob_DefendBase db)
            {
                var newLord = LordMaker.MakeNewLord(reapers,
                    new LordJob_ReaperDefendBase(reapers, Traverse.Create(db).Field("baseCenter").GetValue<IntVec3>()),
                    map);
                foreach (var pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.GetLord() != null) pawn.GetLord().Notify_PawnLost(pawn, PawnLostCondition.Undefined);
                    newLord.AddPawn(pawn);
                }
            }

            foreach (var thing in map.listerThings.AllThings.Where(thing =>
                thing.Faction != null && thing.Faction.def != RER_DefOf.RE_Reapers))
                thing.SetFaction(Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers));

            foreach (var thing in map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget)
                .Where(t => !ThingRequestGroup.Pawn.Includes(t.def)).ToList())
                thing.Destroy();
        }
    }

    public class GenStep_DragonsTeeth : GenStep_Scatterer
    {
        public override int SeedPart => 98762345;

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.MineableGold), loc, map, ThingPlaceMode.Direct);
        }

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return base.CanScatterAt(loc, map);
        }
    }
}