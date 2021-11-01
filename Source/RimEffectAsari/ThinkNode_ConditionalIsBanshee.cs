using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectAsari
{
    using Verse;
    using Verse.AI;

    public class ThinkNode_ConditionalIsBanshee : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn) => 
            pawn.def == REA_DefOf.RE_Husk_Banshee;
    }
}
