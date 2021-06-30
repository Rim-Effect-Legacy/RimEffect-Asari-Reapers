using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimEffectReapers
{
    public class IncidentWorker_ReaperLanding : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var settlement = (Settlement) WorldObjectMaker.MakeWorldObject(RER_DefOf.RE_Reaper);
            var faction = Find.FactionManager.FirstFactionOfDef(RER_DefOf.RE_Reapers);
            settlement.SetFaction(faction);

            try
            {
                if (TileFinder.TryFindPassableTileWithTraversalDistance(Find.AnyPlayerHomeMap.Tile, 5, 90, out var tile,
                    i => TileFinder.IsValidTileForNewSettlement(i)))
                    settlement.Tile = tile;
            }
            catch
            {
                // ignored
            }

            if (settlement.Tile < 0)
                settlement.Tile = TileFinder.RandomSettlementTileFor(faction);

            settlement.Name = NameGenerator.GenerateName(RER_DefOf.RE_NameGenerate);
            Find.WorldObjects.Add(settlement);

            var letterLabel = def.letterLabel;
            var letterText = def.letterText;

            letterText += "\n\n" + RER_DefOf.RE_Reaper.description;


            SendStandardLetter(letterLabel, letterText, def.letterDef, parms, settlement, faction.Name);


            return true;
        }
    }
}