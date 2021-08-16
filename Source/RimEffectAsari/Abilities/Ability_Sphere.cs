using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectAsari
{
    using HarmonyLib;
    using RimEffect;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;
    using Ability = VFECore.Abilities.Ability;


    public class Ability_Sphere : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            BioticSphere bioticSphere = (BioticSphere) GenSpawn.Spawn(REA_DefOf.RE_Biotic_SphereShield, this.Caster.Position, this.pawn.Map);
            bioticSphere.caster    = this.pawn;
            bioticSphere.bioticAmp = this.Hediff as Hediff_BioticAmp;
            bioticSphere.radius    = this.GetRadiusForPawn();
            bioticSphere.damage    = this.GetPowerForPawn();

            if (this.def.targetFlecks.Any())
                bioticSphere.fleck = this.def.targetFlecks.First();
        }

        public override void CheckCastEffects(LocalTargetInfo targetInfo, out bool cast, out bool target, out bool hediffApply)
        {
            base.CheckCastEffects(targetInfo, out cast, out _, out hediffApply);
            target = false;
        }
    }

    public class BioticSphere : Thing
    {
        public delegate void ProjectileDelegate(Projectile proj);

        public static readonly ProjectileDelegate projectileImpactSomethingDelegate = AccessTools.MethodDelegate<ProjectileDelegate>(AccessTools.Method(typeof(Projectile), "ImpactSomething"));

        public Pawn             caster;
        public FleckDef         fleck;
        public Hediff_BioticAmp bioticAmp;

        public float radius;
        public float damage;

        [Unsaved]
        public Graphic graphic;

        public float curRotation = 0f;

        public static float rotSpeed = 5f;

        private       Vector3 impactAngleVect;
        private       int     lastAbsorbDamageTick;

        public IntVec3          cachedPos   = IntVec3.Invalid;
        public HashSet<IntVec3> cachedCells = new HashSet<IntVec3>();

        public override Graphic Graphic
        {
            get
            {
                if (this.graphic == null || Math.Abs(this.graphic.drawSize.x - (this.radius * 2)) > float.Epsilon)
                    this.graphic = GraphicDatabase.Get(this.def.graphicData.graphicClass, this.def.graphicData.texPath, this.def.graphicData.shaderType.Shader,
                                                       new Vector2(this.radius * 2, this.radius * 2), this.def.graphicData.color, this.def.graphicData.colorTwo, this.def.graphicData,
                                                       this.def.graphicData.shaderParameters);
                return this.graphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            FleckMaker.Static(this.DrawPos, this.Map, this.fleck);
        }

        public override void Tick()
        {
            if (this.damage <= 0 || this.caster == null || !this.bioticAmp.SufficientEnergyPresent(1))
            {
                this.Destroy();
                return;
            }

            this.curRotation += rotSpeed % 360f;

            this.bioticAmp.UseEnergy(0.5f);

            /*
            if (this.moteThing is null)
                this.moteThing = MoteMaker.MakeStaticMote(this.DrawPos, this.Map, this.fleck);
            else
                this.moteThing.Maintain();
            */

            if (this.IsHashIntervalTick(GenTicks.TickRareInterval / 2) && this.caster.Position.DistanceToSquared(this.cachedPos) > 2f)
            {
                this.cachedPos   = this.caster.Position;
                this.cachedCells = new HashSet<IntVec3>(GenRadial.RadialCellsAround(this.caster.Position, this.radius+1, true));
            }

            if (this.IsHashIntervalTick(2))
            {
                HashSet<Thing> thingsWithinRadius = new HashSet<Thing>();
                foreach (IntVec3 cell in this.cachedCells)
                {
                    List<Thing> thingList = cell.GetThingList(this.MapHeld);
                    for (int i = 0; i < thingList.Count; i++)
                        thingsWithinRadius.Add(thingList[i]);
                }

                foreach (Thing thing in thingsWithinRadius)
                    if (thing is Projectile proj)
                        if (proj.Launcher is Thing launcher && !thingsWithinRadius.Contains(launcher))
                        {
                            this.AbsorbDamage(proj.DamageAmount, proj.def.projectile.damageDef, proj.ExactRotation.eulerAngles.y);
                            proj.Position   += Rot4.FromAngleFlat((this.Position - proj.Position).AngleFlat).Opposite.FacingCell;
                            proj.usedTarget =  new LocalTargetInfo(proj.Position);
                            projectileImpactSomethingDelegate(proj);
                        }
            }
        }

        

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.caster != null)
            {
                this.caster.health.hediffSet.hediffs.Remove(this.caster.health.hediffSet.GetFirstHediffOfDef(REA_DefOf.RE_Biotic_SphereHediff));
            }

            base.Destroy(mode);
        }

        public void AbsorbDamage(float amount, DamageDef def, float angle)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(this.Position, this.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(angle);
            Vector3 loc       = this.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * (this.radius / 2);
            float   flashSize = Mathf.Min(10f, 2f + amount / 10f);
            FleckMaker.Static(loc, this.Map, FleckDefOf.ExplosionFlash, flashSize);
            int dustCount = (int)flashSize;
            for (int i = 0; i < dustCount; i++)
            {
                FleckMaker.ThrowDustPuff(loc, this.Map, Rand.Range(0.8f, 1.2f));
            }
            float energyLoss = amount;
            this.damage -= energyLoss;

            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
        }

        public override Vector3 DrawPos => this.caster.DrawPos;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float   size = this.radius * 2 * Mathf.Lerp(0.9f, 1.1f, 1);
            Vector3 pos  = this.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            int ticksSinceAbsorbDamage = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
            if (ticksSinceAbsorbDamage < 8)
            {
                float sizeMod = (8 - ticksSinceAbsorbDamage) / 8f * 0.05f;
                pos  += this.impactAngleVect * sizeMod;
                size -= sizeMod;
            }
            
            float     angle  = this.curRotation; // Rand.Range(0, 45);
            Vector3   s      = new Vector3(size, 1f, size);
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.Graphic.MatAt(this.Rotation,this), 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.caster, nameof(this.caster));
            Scribe_References.Look(ref this.bioticAmp, nameof(this.bioticAmp));
            Scribe_Values.Look(ref this.radius, nameof(this.radius));
            Scribe_Values.Look(ref this.damage, nameof(this.damage));

            Scribe_Values.Look(ref this.curRotation, nameof(this.curRotation));
        }
    }
}