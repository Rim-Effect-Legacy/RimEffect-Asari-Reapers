using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimEffectReapers
{
    public class IncidentWorker_ReaperLanding : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (ReaperMod.settings.reaperInvasionIsDisabled)
            {
                return false;
            }
            ReaperIncidentExtension incidentExtension = this.def.GetModExtension<ReaperIncidentExtension>();
            if (incidentExtension != null)
            {
                return (incidentExtension.minimumColonistCount <= 0 || PawnsFinder.AllMaps_FreeColonistsSpawned.Count >= incidentExtension.minimumColonistCount) &&
                       (incidentExtension.minimumWealth        <= 0 || Find.World.PlayerWealthForStoryteller          >= incidentExtension.minimumWealth);
            }

            return base.CanFireNowSub(parms);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            ReaperIncidentExtension incidentExtension = this.def.GetModExtension<ReaperIncidentExtension>();
            WorldObjectDef objectDef = incidentExtension.baseToPlace;

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(objectDef);
            Faction faction = Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers);
            settlement.SetFaction(faction);

            try
            {
                if (TileFinder.TryFindPassableTileWithTraversalDistance(Find.AnyPlayerHomeMap.Tile, incidentExtension.minDistance, incidentExtension.maxDistance, out int tile,
                                                                        i => TileFinder.IsValidTileForNewSettlement(i)))
                    settlement.Tile = tile;
            }
            catch
            {
                // ignored
            }

            if (settlement.Tile < 0)
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction);

            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, objectDef.GetModExtension<ReaperBaseExtension>().nameMaker);
            Find.WorldObjects.Add(settlement);

            string letterLabel = this.def.letterLabel;
            string letterText = this.def.letterText;

            letterText += "\n\n" + objectDef.description;


            this.SendStandardLetter(letterLabel, letterText, def.letterDef, parms, settlement, faction.Name);
            
            /*
            int raisesPresence = (int)objectDef.GetModExtension<ReaperBaseExtension>().raisesPresence;
            int presence       = ReaperUtils.ReaperPresence();

            
            foreach (MechUpgradeWarningDef warningDef in DefDatabase<MechUpgradeWarningDef>.AllDefsListForReading)
            {
                if (presence >= warningDef.presenceRequired && (presence - raisesPresence) < warningDef.presenceRequired)
                {
                    Messages.Message(warningDef.description, MessageTypeDefOf.CautionInput);
                    if (warningDef.sendLetter)
                        Find.LetterStack.ReceiveLetter(warningDef.label, warningDef.description, LetterDefOf.NegativeEvent);
                }
            }
            */

            return true;
        }
    }
}