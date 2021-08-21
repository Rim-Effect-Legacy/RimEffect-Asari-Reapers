using System.Collections.Generic;

namespace RimEffectReapers
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;

    public class ReaperWorldComponent : WorldComponent
    {
        private int lastReaperSpawn;
        private int nextReaperSpawn;

        public ReaperWorldComponent(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (!ReaperMod.settings.reaperInvasionIsDisabled && Find.TickManager.TicksGame >= this.nextReaperSpawn)
            {
                this.lastReaperSpawn = Find.TickManager.TicksGame;

                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, this.world);

                if (ReaperMod.settings.reaperShipIncidentChances.TryRandomElementByWeight(kvp => kvp.Value, out KeyValuePair<string, float> incident))
                {
                    IncidentDef def = DefDatabase<IncidentDef>.GetNamed(incident.Key, false);
                    if (def is null)
                    {
                        ReaperMod.settings.reaperShipIncidentChances.Remove(incident.Key);
                    }
                    else if (def.Worker.CanFireNow(parms))
                    {
                        IncidentDef.Named(incident.Key).Worker.TryExecute(parms);
                    }
                }

                int ticksTillNextInvasion = ReaperMod.settings.reaperTimeInterval.RandomInRange * (1000 * 1 / ReaperUtils.ReaperPresence());
                this.nextReaperSpawn = Find.TickManager.TicksGame + ticksTillNextInvasion;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.lastReaperSpawn, "RE_" + nameof(this.lastReaperSpawn));
            Scribe_Values.Look(ref this.nextReaperSpawn, "RE_" + nameof(this.nextReaperSpawn), 0);
        }
    }
}
