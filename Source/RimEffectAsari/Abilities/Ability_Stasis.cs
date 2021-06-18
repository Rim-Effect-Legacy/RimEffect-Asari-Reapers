namespace RimEffectAsari
{
    using RimEffect;
    using Verse;

    public class Ability_Stasis : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            base.Cast(target);
            target.Pawn.stances.stunner.StunFor_NewTmp(GenTicks.TicksPerRealSecond * 20, this.Caster);
        }
    }
}