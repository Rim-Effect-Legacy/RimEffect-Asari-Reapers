namespace RimEffectAsari
{
    using Verse;
    using VFECore.Abilities;
    using Ability = VFECore.Abilities.Ability;

    public class HediffComp_WarpTargetReact : HediffComp_AbilityTargetReact
    {
        private bool shouldRemove;

        public override bool CompShouldRemove => this.shouldRemove;

        public HediffCompProperties_WarpTargetReact Props => (HediffCompProperties_WarpTargetReact) this.props;

        public override void ReactTo(Ability ability)
        {
            base.ReactTo(ability);

            if (ability.def.defName == "RE_Biotic_Warp") 
                return;

            this.shouldRemove = true;
            GenExplosion.DoExplosion(this.parent.pawn.Position, this.parent.pawn.MapHeld, this.Props.radius, this.Props.damageDef, ability.Caster, this.Props.baseDamage, armorPenetration: float.MaxValue, explosionSound: SoundDef.Named("Explosion_Bomb"));
        }
    }

    public class HediffCompProperties_WarpTargetReact : HediffCompProperties
    {
        public float     radius;
        public DamageDef damageDef;
        public int     baseDamage;

        public HediffCompProperties_WarpTargetReact() => 
            this.compClass = typeof(HediffComp_WarpTargetReact);
    }
}
