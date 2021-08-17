using KCSG;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class Reaper_Skyfaller : KCSG_Skyfaller
    {
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
    }
}