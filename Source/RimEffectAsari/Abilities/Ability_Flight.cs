namespace RimEffectAsari
{
    using System.Linq;
    using RimEffect;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Flight : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            this.CastEffects(targets);
            LongEventHandler.QueueLongEvent(() =>
                                            {
                                                Map     map         = this.pawn.Map;

                                                AbilityPawnFlyer flyer = (AbilityPawnFlyer) PawnFlyer.MakeFlyer(REA_DefOf.RE_AbilityFlyer_Flight, this.pawn, targets.First().Cell, null, null);
                                                flyer.ability = this;
                                                GenSpawn.Spawn(flyer, targets.First().Cell, map);
                                            }, "flightAbility", false, null);
        }

        public override bool CanHitTarget(LocalTargetInfo target, bool sightCheck) => base.CanHitTarget(target, false);

        public override bool CanHitTarget(LocalTargetInfo target) => base.CanHitTarget(target) && target.Cell.Walkable(this.Caster.Map);
    }
}
