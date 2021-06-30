using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class IncidentWorker_DarkSettlement : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && ReaperUtils.ReaperPresence() > 0;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var settlement = Find.WorldObjects.Settlements
                .Where(s => s.Faction.def != RER_DefOf.RE_Reapers && !s.Faction.IsPlayer)
                .RandomElement();

            if (settlement == null) return false;

            var darkSettlement = new DarkSettlement
            {
                def = settlement.def,
                ID = Find.UniqueIDsManager.GetNextWorldObjectID(),
                creationGameTicks = Find.TickManager.TicksGame,
                Tile = settlement.Tile,
                Name = settlement.Name
            };

            var reapers = Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers);

            darkSettlement.SetFaction(reapers);
            darkSettlement.OldFaction = settlement.Faction;

            darkSettlement.PostMake();

            Find.WorldObjects.Remove(settlement);
            Find.WorldObjects.Add(darkSettlement);

            SendStandardLetter(def.letterLabel + ": " + settlement.Name,
                def.letterText, def.letterDef, parms, darkSettlement);
            return true;
        }
    }
}