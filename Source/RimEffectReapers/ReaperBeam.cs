using UnityEngine;
using Verse;

namespace RimEffectReapers
{
    internal class ReaperBeam : Projectile_Explosive
    {
        public override void Draw()
        {
        }

        protected override void Impact(Thing hitThing)
        {
            Log.Message(
                $"Hit {hitThing} while aiming at {intendedTarget} and really hit {usedTarget} while at {Position}");
            if (hitThing != null) Position = hitThing.Position;
            base.Impact(hitThing);
            var graphic = (ReaperBeamDraw) ThingMaker.MakeThing(RER_DefOf.RER_ReaperBeamGraphic);
            graphic.Setup(launcher, origin, destination);
            GenSpawn.Spawn(graphic, ExactPosition.ToIntVec3(), launcher.Map);
        }
    }

    [StaticConstructorOnStartup]
    internal class ReaperBeamDraw : ThingWithComps
    {
        private const int LIFETIME = 60 * 2;

        private static readonly Material BeamMat =
            MaterialPool.MatFrom("Things/Projectiles/ReaperBeam", ShaderDatabase.TransparentPostLight);

        private Vector3 a;
        private Vector3 b;
        private Matrix4x4 drawMatrix;
        private int ticksRemaining;

        public void Setup(Thing launcher, Vector3 origin, Vector3 destination)
        {
            a = origin;
            b = destination;
            ticksRemaining = LIFETIME;
            drawMatrix.SetTRS((a + b) / 2 + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(),
                Quaternion.LookRotation(b - a), new Vector3(5f, 1f, (b - a).magnitude));
            GetComp<CompAffectsSky>()?.StartFadeInHoldFadeOut(10, LIFETIME / 2 - 10, LIFETIME / 2);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
                drawMatrix.SetTRS((a + b) / 2 + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(),
                    Quaternion.LookRotation(b - a), new Vector3(10f, 1f, (b - a).magnitude));
        }

        public override void Tick()
        {
            ticksRemaining--;
            if (ticksRemaining <= 0) Destroy();
        }

        public override void Draw()
        {
            Graphics.DrawMesh(MeshPool.plane10, drawMatrix,
                FadedMaterialPool.FadedVersionOf(BeamMat, Mathf.Sin((float) ticksRemaining / LIFETIME * Mathf.PI)), 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
            Scribe_Values.Look(ref a, "a");
            Scribe_Values.Look(ref b, "b");
        }
    }
}