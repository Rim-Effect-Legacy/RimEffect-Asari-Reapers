using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectAsari
{
    using RimWorld;
    using Verse;
    using Verse.AI;

    public class JobGiver_BansheeTeleport : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Thing target = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, maxDist: 20f);

            if (target == null)
                return null;

            return new Job(REA_DefOf.RE_BansheeTeleport, target);
        }
    }
}
