using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class GameCondition_ReaperWeather : GameCondition
    {
        public override WeatherDef ForcedWeather()
        {
            return RER_DefOf.RER_ReaperLightningStorm;
        }
    }
}