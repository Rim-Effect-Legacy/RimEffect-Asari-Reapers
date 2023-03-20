using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimEffectReapers
{
    using HarmonyLib;

    public class Building_Reaper_LongRangeMissile : Building_Reaper, IAttackTargetSearcher
    {
        private static readonly int COOLDOWN_TICKS = 30f.SecondsToTicks();
        private static readonly FloatRange INCOMING_DELAY_RANGE = new FloatRange(1f, 2f);
        private int cooldownTicksLeft;
        private List<int> incoming = new List<int>();

        public Thing Thing => this;
        public Verb CurrentEffectiveVerb => null;
        public LocalTargetInfo LastAttackedTarget { get; private set; }
        public int LastAttackTargetTick { get; private set; }

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
                if (Map.attackTargetsCache.TargetsHostileToFaction(Faction).Any()) Fire();
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
            var missile = (LongRangeMissile) SkyfallerMaker.SpawnSkyfaller(RER_DefOf.RER_ReaperLongRangeMissile_Leaving, Position, Map);
            missile.Init(this);
            RER_DefOf.RE_Launch_ReaperLongRangeMissile.PlayOneShot(SoundInfo.InMap(missile, MaintenanceType.PerTick));
        }

        public void Incoming()
        {
            LastAttackTargetTick = Find.TickManager.TicksGame;
            if (!Map.attackTargetsCache.GetPotentialTargetsFor(this).TryRandomElement(out IAttackTarget targ))
            {
                incoming.Add(Find.TickManager.TicksGame + (INCOMING_DELAY_RANGE.RandomInRange * 5f).SecondsToTicks());
                return;
            }
            Thing target = targ.Thing;
            LastAttackedTarget = target;
            var missile = (LongRangeMissile) SkyfallerMaker.SpawnSkyfaller(RER_DefOf.RER_ReaperLongRangeMissile_Arriving, target.Position, Map);
            missile.Init(this);
            missile.angle = 0; //new Vector3(Map.Size.ToVector3().x, target.DrawPos.y, DrawPos.z).AngleToFlat(target.DrawPos);
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

        public override string GetInspectString() =>
            base.GetInspectString() +
            "RE_LongRangeMissile_Inspect".Translate(cooldownTicksLeft.ToStringSecondsFromTicks());
    }

    public class LongRangeMissile : Skyfaller
    {
        private int                              betterTicksToImpact;
        private int                              betterTicksToImpactMax;
        private Building_Reaper_LongRangeMissile launcher;
        private int                              ticksToImpactMaxPrivate;
        

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.angle = 0;
            if (!respawningAfterLoad && def.skyfaller.reversed)
            {
                betterTicksToImpactMax = betterTicksToImpact = def.skyfaller.ticksToImpactRange.RandomInRange;
            }

            this.ticksToImpactMaxPrivate = (int)AccessTools.Field(typeof(Skyfaller), "ticksToImpactMax").GetValue(this);
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
                if (Find.TickManager.TicksGame % 3 == 0)
                    FleckMaker.ThrowSmoke(DrawPos - new Vector3(0, 0, Graphic.drawSize.y / 2), Map, 1.5f);
            }
            Vector3 drawPos = this.GetDrawPosForMotes();

            if (this.Map == null || !drawPos.InBounds(this.Map))
                return;



            FleckMaker.ThrowSmoke(drawPos, this.Map, 2f);

            FleckCreationData data = FleckMaker.GetDataStatic(drawPos, this.Map, RER_DefOf.RER_ReaperGlow, Rand.Range(4f, 6f) * 2);
            data.rotationRate  = Rand.Range(-3f, 3f);
            data.velocityAngle = Rand.Range(0,   360);
            data.velocitySpeed = 0.12f;
            this.Map.flecks.CreateFleck(data);

        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Graphic.Draw(drawLoc, flip ? this.Rotation.Opposite : this.Rotation, this, this.def.skyfaller.movementType == SkyfallerMovementType.Decelerate ? 0 : 180);
        }

        private Vector3 GetDrawPosForMotes()
        {
            float currentSpeed            = 1f;
            float timeInAnim              = 1;
            int   ticksToImpactPrediction = 220;

            switch (this.def.skyfaller.movementType)
            {
                case SkyfallerMovementType.Accelerate:
                    ticksToImpactPrediction = this.ticksToImpact - GenTicks.TicksPerRealSecond / 2;
                    timeInAnim              = 1                  - ticksToImpactPrediction     / this.ticksToImpactMaxPrivate;
                    currentSpeed            = (this.def.skyfaller.speedCurve?.Evaluate(timeInAnim) ?? 1) * this.def.skyfaller.speed;
                    return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                case SkyfallerMovementType.ConstantSpeed:
                    return SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                case SkyfallerMovementType.Decelerate:
                    ticksToImpactPrediction = this.ticksToImpact + GenTicks.TicksPerRealSecond / 2;
                    timeInAnim              = (float)ticksToImpactPrediction                     / 220f;
                    currentSpeed            = this.def.skyfaller.speedCurve.Evaluate(timeInAnim) * this.def.skyfaller.speed;
                    return SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
                default:
                    Log.ErrorOnce("SkyfallerMovementType not handled: " + this.def.skyfaller.movementType, this.thingIDNumber ^ 0x7424EBC7);
                    return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksToImpactPrediction, this.angle, currentSpeed);
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