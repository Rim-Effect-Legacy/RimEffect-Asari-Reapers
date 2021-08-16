using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimEffectReapers
{
    public class PawnsArrivalModeWorker_ReaperPlanetfall : PawnsArrivalModeWorker
    {
        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            PawnsArrivalModeWorkerUtility.DropInDropPodsNearSpawnCenter(parms, pawns);
        }

        public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
        {
            IntVec3 near;
            if (!DropCellFinder.TryFindRaidDropCenterClose(out near, map))
                near = DropCellFinder.FindRaidDropCenterDistant(map);

            TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            var map = (Map) parms.target;
            parms.spawnCenter = ReaperUtils.FindRect(map, 100);
            parms.spawnRotation = Rot4.Random;
            return true;
        }
    }
}