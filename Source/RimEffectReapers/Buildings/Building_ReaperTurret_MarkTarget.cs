using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class Building_ReaperTurret_MarkTarget : Building_ReaperTurret
    {
        public override void Draw()
        {
            base.Draw();
            if (!TargetCurrentlyAimingAt.IsValid ||
                TargetCurrentlyAimingAt.HasThing && !TargetCurrentlyAimingAt.Thing.Spawned) return;

            var b = TargetCurrentlyAimingAt.HasThing
                ? TargetCurrentlyAimingAt.Thing.TrueCenter()
                : TargetCurrentlyAimingAt.Cell.ToVector3Shifted();
            var a = this.TrueCenter();
            a.y = b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
        }
    }
}