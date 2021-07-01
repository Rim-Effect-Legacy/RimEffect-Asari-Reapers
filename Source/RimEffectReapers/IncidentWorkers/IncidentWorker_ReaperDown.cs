using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    internal class IncidentWorker_ReaperDown : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return Find.FactionManager.AllFactions.Any(f =>
                       !f.defeated && !f.def.isPlayer && !f.Hidden && !f.temporary && f.def != RER_DefOf.RE_Reapers) &&
                   Find.WorldObjects.AllWorldObjects.Any(wo => wo is ReaperShip);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var faction = Find.FactionManager.AllFactions
                .Where(f => !f.defeated && !f.def.isPlayer && !f.Hidden && !f.temporary &&
                            f.def != RER_DefOf.RE_Reapers).RandomElementWithFallback();
            if (faction == null) return false;

            var reaper = Find.WorldObjects.AllWorldObjects.Where(wo => wo is ReaperShip).RandomElementWithFallback();

            if (reaper == null) return false;

            reaper.Destroy();

            SendStandardLetter(def.letterLabel + ": " + reaper.Label, def.letterText.Formatted(faction, reaper),
                def.letterDef, parms, null);

            return true;
        }
    }
}