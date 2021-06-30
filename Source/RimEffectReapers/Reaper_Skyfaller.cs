using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimEffectReapers
{
    public class Reaper_Skyfaller : Skyfaller
    {
        private static readonly IntRange range = new IntRange(-3, 3);
        private GameCondition caused;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (map.gameConditionManager.ConditionIsActive(RER_DefOf.RER_ReaperWeather)) return;
            caused = GameConditionMaker.MakeConditionPermanent(RER_DefOf.RER_ReaperWeather);
            caused.conditionCauser = this;
            map.GameConditionManager.RegisterCondition(caused);
            Map.weatherManager.TransitionTo(RER_DefOf.RER_ReaperLightningStorm);
            Map.weatherDecider.StartNextWeather();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (caused != null)
            {
                caused.End();
                Map.weatherManager.TransitionTo(WeatherDefOf.Clear);
                Map.weatherDecider.DisableRainFor(GenDate.TicksPerDay / 2);
                Map.weatherDecider.StartNextWeather();
            }

            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref caused, "caused");
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            innerContainer[0]?.DrawAt(drawLoc, flip);
        }

        protected override void SpawnThings()
        {
            foreach (var t in innerContainer)
                GenPlace.TryPlaceThing(t, Position, Map, ThingPlaceMode.Direct,
                    delegate(Thing thing, int count)
                    {
                        PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                        if (thing.def.Fillage == FillCategory.Full && def.skyfaller.CausesExplosion &&
                            def.skyfaller.explosionDamage.isExplosive &&
                            thing.Position.InHorDistOf(Position, def.skyfaller.explosionRadius))
                            Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
                    }, null, Rotation);

            foreach (var thing in innerContainer)
            foreach (var t in GenAdj.OccupiedRect(Position, Rotation, thing.def.size).SelectMany(
                cell =>
                    cell.GetThingList(Map)).Except(innerContainer))
                t.Destroy();
        }

        public override void Tick()
        {
            base.Tick();
            if (Rand.Chance(0.001f) && this.IsHashIntervalTick(60))
                Map.weatherManager.eventHandler.AddEvent(
                    new WeatherEvent_ReaperLightningStrike(Find.CurrentMap,
                        Position + new IntVec3(range.RandomInRange, 0, range.RandomInRange)));
        }
    }
}