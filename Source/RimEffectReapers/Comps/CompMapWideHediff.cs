using System.Linq;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class CompProperties_Hediff : CompProperties
    {
        public bool affectNonReapers;
        public bool affectReapers;
        public HediffDef hediff;
        public StatDef relevantStat;
        public float severityPerTick;
        public bool vanishesInstantly;
    }

    public class CompMapWideHediff : ThingComp
    {
        public CompProperties_Hediff Props => props as CompProperties_Hediff;

        public override void CompTickRare()
        {
            base.CompTickRare();
            var pawns = parent.Map.mapPawns.AllPawnsSpawned.Where(p => p.health != null);
            if (!Props.affectReapers) pawns = pawns.Where(p => p.RaceProps.FleshType != RER_DefOf.RE_Husk);
            if (!Props.affectNonReapers) pawns = pawns.Where(p => p.RaceProps.FleshType == RER_DefOf.RE_Husk);
            foreach (var pawn in pawns)
                if (!pawn.health.hediffSet.HasHediff(Props.hediff))
                    if (pawn.health.AddHediff(Props.hediff) is HediffFromBuilding hdb)
                        hdb.Source = this;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (Props.vanishesInstantly)
            {
                var pawns = parent.Map.mapPawns.AllPawnsSpawned.Where(p => p.health != null);
                if (!Props.affectReapers) pawns = pawns.Where(p => p.RaceProps.FleshType != RER_DefOf.RE_Husk);
                if (!Props.affectNonReapers) pawns = pawns.Where(p => p.RaceProps.FleshType == RER_DefOf.RE_Husk);
                foreach (var pawn in pawns)
                foreach (var hediff in pawn.health.hediffSet.hediffs.Where(hd => hd.def == Props.hediff).ToList())
                    pawn.health.RemoveHediff(hediff);
            }

            base.PostDestroy(mode, previousMap);
        }
    }
}