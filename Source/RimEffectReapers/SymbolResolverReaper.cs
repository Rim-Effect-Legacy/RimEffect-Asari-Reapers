using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace RimEffectReapers
{
    internal class SymbolResolverReaper : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();

            var singlePawnLord = rp.singlePawnLord ??
                                 LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell),
                                     map);
            var traverseParms = TraverseParms.For(TraverseMode.PassDoors);
            var resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            resolveParams.singlePawnSpawnCellExtraPredicate = rp.singlePawnSpawnCellExtraPredicate ??
                                                              (x => map.reachability.CanReachMapEdge(x, traverseParms));
            if (resolveParams.pawnGroupMakerParams == null &&
                faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms
                {
                    tile = map.Tile,
                    faction = faction,
                    points =
                        rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange,
                    inhabitants = true,
                    seed = rp.settlementPawnGroupSeed
                };
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
                BaseGen.symbolStack.Push("pawnGroup", resolveParams);

            foreach (var c in rp.rect.Where(c => map.fogGrid.IsFogged(c)))
                map.fogGrid.Unfog(c);

            var rp2 = rp;
            rp2.faction = faction;
            BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", rp2);

            foreach (var c in rp.rect)
                c.GetThingList(map).ToList()
                    .FindAll(t1 => t1.def.category == ThingCategory.Filth || t1.def.category == ThingCategory.Item)
                    .ForEach(t => t.DeSpawn());
        }
    }
}