using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class HediffFromBuilding : HediffWithComps
    {
        public CompMapWideHediff Source;

        public override void Tick()
        {
            base.Tick();
            if (Source?.parent != null && Source.parent.Spawned && !Source.parent.Destroyed)
                Severity += Source.Props.severityPerTick *
                            (Source.Props.relevantStat == null ? 1 : pawn.GetStatValue(Source.Props.relevantStat));
            else if (Source != null)
                if (Source.Props.vanishesInstantly) pawn.health.RemoveHediff(this);
                else
                    Severity -= Source.Props.severityPerTick *
                                (Source.Props.relevantStat == null ? 1 : pawn.GetStatValue(Source.Props.relevantStat));
            else Severity -= 0.0001f;
        }
    }
}