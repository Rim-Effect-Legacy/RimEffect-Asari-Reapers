using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    using System.Text;

    public class Building_Reaper : Building
    {
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            foreach (var building in Map.listerThings.ThingsOfDef(RER_DefOf.RE_ReaperUnitStorage)
                .Cast<Building_Reaper_UnitStorage>().Where(b => b.CanRelease))
                building.Release();
        }

        public override bool ClaimableBy(Faction by, StringBuilder reason = null)
        {
            return false;
        }
    }
}