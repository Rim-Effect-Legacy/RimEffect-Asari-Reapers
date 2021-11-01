namespace RimEffectAsari
{
    using RimEffect;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Flight : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            this.CastEffects(target);
            LongEventHandler.QueueLongEvent(() =>
                                            {
                                                Map     map         = this.pawn.Map;

                                                AbilityPawnFlyer flyer = (AbilityPawnFlyer) PawnFlyer.MakeFlyer(REA_DefOf.RE_AbilityFlyer_Flight, this.pawn, target.Cell);
                                                flyer.ability = this;
                                                flyer.target  = target.Cell.ToVector3() + new Vector3(0, 0, 0);
                                                GenSpawn.Spawn(flyer, target.Cell, map);
                                            }, "flightAbility", false, null);
        }

        public override bool CanHitTarget(LocalTargetInfo target, bool sightCheck) => base.CanHitTarget(target, false);

        public override bool CanHitTarget(LocalTargetInfo target) => base.CanHitTarget(target) && target.Cell.Walkable(this.Caster.Map);
    }
}
