using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace RimEffectReapers
{
    public class DefModExt_DeathSpawnerProps : DefModExtension
    {
        public PawnKindDef kindDef;

        public IntRange quantityRange = new IntRange(1, 1);
    }
}
