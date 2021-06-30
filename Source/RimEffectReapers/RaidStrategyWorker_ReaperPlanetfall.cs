using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimEffectReapers
{
    public class RaidStrategyWorker_ReaperPlanetfall : RaidStrategyWorker_Siege
    {
        public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            return parms.points >= MinimumPoints(parms.faction, groupKind) &&
                   parms.faction.def == RER_DefOf.RE_Reapers && ReaperUtils.ReaperPresence() > 0 &&
                   ReaperUtils.FindRect((Map) parms.target, 50) != IntVec3.Invalid;
        }

        public override List<Pawn> SpawnThreats(IncidentParms parms)
        {
            StructureLayoutDef structureLayoutDef;
            if (ModLister.RoyaltyInstalled)
                structureLayoutDef = def.modContentPack.AllDefs.OfType<StructureLayoutDef>()
                    .RandomElement();
            else
                structureLayoutDef = def.modContentPack.AllDefs.OfType<StructureLayoutDef>()
                    .Where(s => !s.defName.Contains("DLC")).RandomElement();
            ReaperUtils.HeightWidthFromLayout(structureLayoutDef, out var h, out var w);
            var cellRect = CellRect.CenteredOn(parms.spawnCenter, w, h);

            var allSymbList = new List<string>();
            var map = parms.target as Map;

            foreach (var str in structureLayoutDef.layouts[0])
            {
                var symbSplitFromLine = str.Split(',').ToList();
                symbSplitFromLine.ForEach(s => allSymbList.Add(s));
            }

            var pairsSymbolLabel = ReaperUtils.FillpairsSymbolLabel();
            var l = 0;
            foreach (var cell in cellRect.Cells)
            {
                if (l < allSymbList.Count && allSymbList[l] != ".")
                {
                    SymbolDef temp;
                    pairsSymbolLabel.TryGetValue(allSymbList[l], out temp);
                    Thing thing;

                    if (temp?.thingDef != null &&
                        (temp.defName.Contains("RE") || temp.modContentPack == def.modContentPack))
                    {
                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);

                        if (thing.def.rotatable) thing.Rotation = temp.rotation;
                        var label = structureLayoutDef.defName.Remove(0, 5).Replace("DLC", "");

                        var faller = new ThingDef
                        {
                            category = ThingCategory.Ethereal,
                            useHitPoints = false,
                            thingClass = typeof(Reaper_Skyfaller),
                            drawOffscreen = true,
                            tickerType = TickerType.Normal,
                            altitudeLayer = AltitudeLayer.Skyfaller,
                            drawerType = DrawerType.RealtimeOnly,
                            defName = temp.thingDef.defName + "_Incoming",
                            skyfaller = new SkyfallerProperties
                            {
                                shadowSize = new Vector2(thing.def.size.x + 1, thing.def.size.z + 1),
                                ticksToImpactRange = new IntRange(600, 600),
                                movementType = SkyfallerMovementType.Decelerate
                            },
                            size = new IntVec2(thing.def.size.x, thing.def.size.z),
                            label = label.Remove(label.Length - 1, 1) + " (incoming)"
                        };

                        var skyfaller2 = SkyfallerMaker.MakeSkyfaller(faller);
                        if (!skyfaller2.innerContainer.TryAdd(thing))
                        {
                            Log.Error("Could not add " + thing.ToStringSafe() + " to a Reaper_Skyfaller.");
                            thing.Destroy();
                        }

                        GenSpawn.Spawn(skyfaller2, cell, map, thing.Rotation);
                    }
                    else if (temp?.thingDef != null)
                    {
                        thing = ThingMaker.MakeThing(temp.thingDef, temp.stuffDef);
                        thing.SetFactionDirect(parms.faction);

                        var activeDropPodInfo = new ActiveDropPodInfo();
                        activeDropPodInfo.innerContainer.TryAdd(thing);
                        activeDropPodInfo.openDelay = 40;
                        activeDropPodInfo.leaveSlag = false;
                        DropPodUtility.MakeDropPodAt(cell, map, activeDropPodInfo);
                    }
                }

                l++;
            }

            var parms1 = parms;
            RCellFinder.TryFindRandomCellNearWith(parms.spawnCenter, i => i.Walkable(map), map, out parms1.spawnCenter,
                33, 40);

            return base.SpawnThreats(parms1);
        }

        protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
        {
            var originCell = parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld;
            if (parms.faction.HostileTo(Faction.OfPlayer)) return new LordJob_AssaultColony(parms.faction);

            IntVec3 fallbackLocation;
            RCellFinder.TryFindRandomSpotJustOutsideColony(originCell, map, out fallbackLocation);
            return new LordJob_AssistColony(parms.faction, fallbackLocation);
        }
    }
}