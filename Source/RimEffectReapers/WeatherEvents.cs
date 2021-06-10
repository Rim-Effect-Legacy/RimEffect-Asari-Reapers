using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimEffectReapers
{
    public class WeatherEvent_ReaperLightningFlash : WeatherEvent_LightningFlash
    {
        public WeatherEvent_ReaperLightningFlash(Map map) : base(map)
        {
        }

        public override SkyTarget SkyTarget =>
            new SkyTarget(1f,
                new SkyColorSet(new Color(1.0f, 0.95f, 1f), new Color(1.0f, 0.8235294f, 0.847058833f),
                    new Color(1.0f, 0.95f, 1f), 1.15f), 1f, 1f);
    }

    [StaticConstructorOnStartup]
    public class WeatherEvent_ReaperLightningStrike : WeatherEvent_ReaperLightningFlash
    {
        private static readonly Material LIGHTNING_MAT =
            MaterialPool.MatFrom("Things/Projectiles/ReaperLightningBolt", ShaderDatabase.MoteGlow);

        private Mesh boltMesh;

        private IntVec3 strikeLoc = IntVec3.Invalid;

        public WeatherEvent_ReaperLightningStrike(Map map) : base(map)
        {
        }

        public WeatherEvent_ReaperLightningStrike(Map map, IntVec3 forcedStrikeLoc) : base(map)
        {
            strikeLoc = forcedStrikeLoc;
        }

        public override void FireEvent()
        {
            base.FireEvent();
            if (!strikeLoc.IsValid)
                strikeLoc =
                    CellFinderLoose.RandomCellWith(
                        sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);

            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (!strikeLoc.Fogged(map))
            {
                GenExplosion.DoExplosion(strikeLoc, map, 1.9f, DamageDefOf.Flame, null, -1, -1f, null, null,
                    null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null);
                var loc = strikeLoc.ToVector3Shifted();
                for (var i = 0; i < 4; i++)
                {
                    MoteMaker.ThrowSmoke(loc, map, 1.5f);
                    MoteMaker.ThrowMicroSparks(loc, map);
                    MoteMaker.ThrowLightningGlow(loc, map, 1.5f);
                }
            }

            var info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        public override void WeatherEventDraw()
        {
            Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather),
                Quaternion.identity,
                FadedMaterialPool.FadedVersionOf(LIGHTNING_MAT, LightningBrightness),
                0);
        }
    }
}