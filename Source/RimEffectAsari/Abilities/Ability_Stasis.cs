namespace RimEffectAsari
{
    using RimWorld.Planet;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    public class Ability_Stasis : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (GlobalTargetInfo target in targets)
            {
                ((Pawn)target.Thing).stances.stunner.StunFor(GenTicks.TicksPerRealSecond * 20, this.Caster);
            }
        }
    }
}