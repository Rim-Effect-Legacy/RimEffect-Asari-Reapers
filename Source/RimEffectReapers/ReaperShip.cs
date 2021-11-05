using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimEffectReapers
{
    public class ReaperShip : Settlement
    {
        public ReaperShip()
        {
            previouslyGeneratedInhabitants = new List<Pawn>();
            trader = null;
        }

        public override string GetInspectString()
        {
            var text = base.GetInspectString();
            if (!text.NullOrEmpty()) text += "\n";

            text += def.description;

            return text;
        }
    }
}