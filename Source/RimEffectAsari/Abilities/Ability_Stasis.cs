namespace RimEffectAsari
{
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Stasis : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            target.Pawn.stances.stunner.StunFor(GenTicks.TicksPerRealSecond * 20, this.Caster);
        }
    }
}