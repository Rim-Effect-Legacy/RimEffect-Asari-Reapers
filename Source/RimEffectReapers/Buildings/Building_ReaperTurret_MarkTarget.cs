using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimEffectReapers
{
    public class Building_ReaperTurret_MarkTarget : Building_ReaperTurret
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (!TargetCurrentlyAimingAt.IsValid ||
                TargetCurrentlyAimingAt.HasThing && !TargetCurrentlyAimingAt.Thing.Spawned) return;

            var b = TargetCurrentlyAimingAt.HasThing
                ? TargetCurrentlyAimingAt.Thing.TrueCenter()
                : TargetCurrentlyAimingAt.Cell.ToVector3Shifted();
            var a = this.TrueCenter();
            a.y = b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
        }

        public override void Tick()
        {
            base.Tick();
            if (Mathf.Approximately(5f, burstWarmupTicksLeft.TicksToSeconds()))
                RER_DefOf.RE_Targeting_ReaperOrbitalBeam.PlayOneShot(SoundInfo.InMap(this));
        }
    }
}