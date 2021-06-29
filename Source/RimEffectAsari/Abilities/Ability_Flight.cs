﻿namespace RimEffectAsari
{
    using RimEffect;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Ability = RimEffect.Ability;

    public class Ability_Flight : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            LongEventHandler.QueueLongEvent(() =>
                                            {
                                                Map     map         = this.pawn.Map;

                                                AbilityPawnFlyer flyer = (AbilityPawnFlyer) PawnFlyer.MakeFlyer(RE_DefOf.RE_AbilityFlyer, this.pawn, target.Cell);
                                                flyer.ability = this;
                                                flyer.target  = target.Cell.ToVector3() + new Vector3(0, 1, 0);
                                                GenSpawn.Spawn(flyer, target.Cell, map);
                                            }, "flightAbility", false, null);
        }

        public override bool CanHitTarget(LocalTargetInfo target, bool sightCheck) => base.CanHitTarget(target, false);

        public override bool CanHitTarget(LocalTargetInfo target) => base.CanHitTarget(target) && target.Cell.Walkable(this.Caster.Map);
    }
}