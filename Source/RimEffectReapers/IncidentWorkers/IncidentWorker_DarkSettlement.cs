using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    using System.Collections.Generic;
    using RimWorld.Planet;

    public class IncidentWorker_DarkSettlement : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && ReaperUtils.ReaperPresence() > 1;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            IEnumerable<IGrouping<Faction, Settlement>> validBases = Find.WorldObjects.Settlements.GroupBy(s => s.Faction).Where(g => g.Key.def != RER_DefOf.RE_Reapers && !g.Key.IsPlayer);

            IEnumerable<IGrouping<Faction, Settlement>> validBasesFromValidFactions = validBases.Where(g => g.Count() > 1);
            Settlement settlement = (validBasesFromValidFactions.Any() ? validBasesFromValidFactions : validBases).RandomElement().RandomElement();

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

            settlement.Destroy();
            Find.WorldObjects.Add(darkSettlement);

            var letterText = def.letterText;

            if (!ReaperUtils.HasAnyOtherBase(settlement))
            {
                settlement.Faction.defeated = true;
                letterText +=
                    $"\n\n\n{"LetterFactionBaseDefeated_FactionDestroyed".Translate(settlement.Faction.Name)}";
            }

            SendStandardLetter(def.letterLabel + ": " + settlement.Name,
                letterText, def.letterDef, parms, darkSettlement);
            return true;
        }
    }
}