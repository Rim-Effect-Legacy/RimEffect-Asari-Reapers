using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class Building_ReaperTurret : Building_TurretGun
    {
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            foreach (var building in Map.listerThings.ThingsOfDef(RER_DefOf.RE_ReaperUnitStorage)
                .Cast<Building_Reaper_UnitStorage>().Where(b => b.CanRelease))
                building.Release();
        }

        public override bool ClaimableBy(Faction by)
        {
            return false;
        }
    }
}