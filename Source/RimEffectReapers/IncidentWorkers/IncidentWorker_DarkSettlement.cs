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