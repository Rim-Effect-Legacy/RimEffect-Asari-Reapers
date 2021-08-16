using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimEffectReapers
{
    public class Building_Reaper_LongRangeMissile : Building_Reaper
    {
        private static readonly int COOLDOWN_TICKS = 30f.SecondsToTicks();
        private static readonly FloatRange INCOMING_DELAY_RANGE = new FloatRange(1f, 2f);
        private int cooldownTicksLeft;
        private List<int> incoming = new List<int>();

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (cooldownTicksLeft > 0)
                GenDraw.DrawAimPieRaw(DrawPos -
                    Quaternion.AngleAxis(0f, Vector3.up) * Vector3.forward * 0.8f + Vector3.up * 5f, 0f,
                    cooldownTicksLeft / COOLDOWN_TICKS * 360);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad) cooldownTicksLeft = COOLDOWN_TICKS;
        }

        public override void Tick()
        {
            base.Tick();
            if (!Spawned) return;
            if (cooldownTicksLeft > 0) cooldownTicksLeft--;
            if (cooldownTicksLeft <= 0)
            {
                Fire();
                cooldownTicksLeft = COOLDOWN_TICKS;
            }

            foreach (var time in incoming.Where(i => i <= Find.TickManager.TicksGame).ToList())
            {
                Incoming();
                incoming.Remove(time);
            }
        }

        public void Fire()
        {
            var missile =
                (LongRangeMissile) SkyfallerMaker.SpawnSkyfaller(RER_DefOf.RER_ReaperLongRangeMissile_Leaving, Position,
                    Map);
            missile.Init(this);
            RER_DefOf.RE_Launch_ReaperLongRangeMissile.PlayOneShot(SoundInfo.InMap(missile, MaintenanceType.PerTick));
        }

        public void Incoming()
        {
            var target = Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget)
                .Where(t => t.Faction.HostileTo(Faction))
                .Concat(Map.mapPawns.AllPawnsSpawned.Where(p => p.Faction.HostileTo(Faction))).RandomElement();
            if (target == null)
            {
                incoming.Add(
                    Find.TickManager.TicksGame + (INCOMING_DELAY_RANGE.RandomInRange * 5f).SecondsToTicks());
                return;
            }

            var missile =
                (LongRangeMissile) SkyfallerMaker.SpawnSkyfaller(RER_DefOf.RER_ReaperLongRangeMissile_Arriving,
                    target.Position, Map);
            missile.Init(this);
            missile.angle =
                new Vector3(Map.Size.ToVector3().x, target.DrawPos.y, DrawPos.z).AngleToFlat(target.DrawPos);
            RER_DefOf.RE_Incoming_ReaperLongRangeMissile.PlayOneShot(SoundInfo.InMap(missile, MaintenanceType.PerTick));
        }

        public void QueueIncoming()
        {
            incoming.Add(Find.TickManager.TicksGame + INCOMING_DELAY_RANGE.RandomInRange.SecondsToTicks());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicksLeft, "cooldown");
            Scribe_Collections.Look(ref incoming, "incoming");
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() +
                   "RE_LongRangeMissile_Inspect".Translate(cooldownTicksLeft.ToStringSecondsFromTicks());
        }
    }

    public class LongRangeMissile : Skyfaller
    {
        private int betterTicksToImpact;
        private int betterTicksToImpactMax;
        private Building_Reaper_LongRangeMissile launcher;

        public override Vector3 DrawPos
        {
            get
            {
                if (def.skyfaller.reversed)
                    return this.TrueCenter() + new Vector3(0, 0,
                        Mathf.Pow(betterTicksToImpactMax - betterTicksToImpact, def.skyfaller.speed) * 0.05f +
                        (betterTicksToImpactMax - betterTicksToImpact) * 0.1f);
                return base.DrawPos;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && def.skyfaller.reversed)
            {
                betterTicksToImpactMax = betterTicksToImpact = def.skyfaller.ticksToImpactRange.RandomInRange;
                ticksToImpact = 0;
            }
        }

        public void Init(Building_Reaper_LongRangeMissile l)
        {
            launcher = l;
        }

        protected override void LeaveMap()
        {
            Log.Message($"Leaving map after {betterTicksToImpactMax} ticks");
            if (launcher != null && launcher.Spawned)
                launcher.QueueIncoming();
            base.LeaveMap();
        }

        public override void Tick()
        {
            base.Tick();
            if (def.skyfaller.reversed)
            {
                if (betterTicksToImpact > 0)
                {
                    ticksToImpact = 0;
                    betterTicksToImpact--;
                }

                if (betterTicksToImpact <= 0) ticksToImpact = 219;
                if (Find.TickManager.TicksGame % 3 == 0)
                    FleckMaker.ThrowSmoke(DrawPos - new Vector3(0, 0, Graphic.drawSize.y / 2), Map, 1.5f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref launcher, "launcher");
            Scribe_Values.Look(ref betterTicksToImpact, "betterTicksToImpact");
            Scribe_Values.Look(ref betterTicksToImpactMax, "betterTicksToImpactMax");
        }
    }
}