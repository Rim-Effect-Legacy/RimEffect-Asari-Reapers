using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectAsari
{
    using RimWorld;
    using Verse;

    public class StatWorker_ArdatYakshiPower : StatWorker
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            float valueUnfinalized = base.GetValueUnfinalized(req, applyPostProcess);

            valueUnfinalized *= (req.Thing as Pawn)?.health.hediffSet.GetFirstHediffOfDef(REA_DefOf.RE_ArdatYakshi_Power).Severity ?? 1;

            return valueUnfinalized;
        }
    }
}