using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimEffectReapers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class ReaperSettings : ModSettings
    {
        public List<IncidentDef> incidents = new List<IncidentDef>();

        public Dictionary<string, float> reaperShipIncidentChances = new Dictionary<string, float>();
        public Dictionary<string, int>   reaperShipPresences       = new Dictionary<string, int>();
        public Dictionary<string, int>   reaperShipColonistCount   = new Dictionary<string, int>();
        public Dictionary<string, int>   reaperShipWealth   = new Dictionary<string, int>();
        public Dictionary<string, int>   reaperShipDistances       = new Dictionary<string, int>();

        public IntRange reaperTimeInterval = new IntRange(GenDate.TicksPerQuadrum / 2, GenDate.TicksPerQuadrum);


        public bool reaperInvasionIsDisabled;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.reaperInvasionIsDisabled, "reaperInvasionIsDisabled");
            Scribe_Collections.Look(ref this.reaperShipIncidentChances, "reaperShipStates",        LookMode.Value, LookMode.Value, ref mechShipKeys,  ref floatValues);
            Scribe_Collections.Look(ref this.reaperShipPresences,       "reaperShipPresences",     LookMode.Value, LookMode.Value, ref mechShipKeys2, ref intValues2);
            Scribe_Collections.Look(ref this.reaperShipColonistCount,   "reaperShipColonistCount", LookMode.Value, LookMode.Value, ref mechShipKeys3, ref intValues3);
            Scribe_Collections.Look(ref this.reaperShipWealth,          "reaperShipWealth",        LookMode.Value, LookMode.Value, ref mechShipKeys3, ref intValues3);
            Scribe_Collections.Look(ref this.reaperShipDistances,       "reaperShipDistances",     LookMode.Value, LookMode.Value, ref mechShipKeys4, ref intValues4);

            int reaperTimeIntervalMax = this.reaperTimeInterval.max;
            int reaperTimeIntervalMin = this.reaperTimeInterval.min;
            Scribe_Values.Look(ref reaperTimeIntervalMax, "RE_" + nameof(reaperTimeIntervalMax), GenDate.TicksPerQuadrum);
            Scribe_Values.Look(ref reaperTimeIntervalMin, "RE_" + nameof(reaperTimeIntervalMin), GenDate.TicksPerQuadrum / 2);
            this.reaperTimeInterval = new IntRange(reaperTimeIntervalMin, reaperTimeIntervalMax);
        }

        private List<string> mechShipKeys;
        private List<float> floatValues;

        private List<string> mechShipKeys2;
        private List<int> intValues2;

        private List<string> mechShipKeys3;
        private List<int> intValues3;

        private List<string> mechShipKeys4;
        private List<int> intValues4;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var keys  = this.reaperShipIncidentChances.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys2 = this.reaperShipPresences.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys3 = this.reaperShipColonistCount.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys5 = this.reaperShipWealth.Keys.ToList().OrderByDescending(x => x).ToList();
            var keys4 = this.reaperShipDistances.Keys.ToList().OrderByDescending(x => x).ToList();

            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, (keys.Count * 30) + (keys2.Count * 30) + (keys3.Count * 30) + (keys5.Count * 30) + (keys4.Count * 30) + 450);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            listingStandard.CheckboxLabeled("RER.DisableReaperInvasionMechanic".Translate(), ref this.reaperInvasionIsDisabled);
            listingStandard.GapLine();
            listingStandard.Label("RER.AdjustIncidentTimeInterval".Translate());

            listingStandard.Label($"{this.reaperTimeInterval.min.ToStringTicksToPeriodVerbose(false)} - {this.reaperTimeInterval.max.ToStringTicksToPeriodVerbose(false)}");
            listingStandard.IntRange(ref this.reaperTimeInterval, GenDate.TicksPerDay, GenDate.TicksPerYear * 5);

            if (listingStandard.ButtonText("Reset".Translate()))
            {
                this.reaperTimeInterval = new IntRange(GenDate.TicksPerQuadrum / 2, GenDate.TicksPerQuadrum);
            }

            listingStandard.GapLine();
            listingStandard.Label("RER.AdjustIncidentChanceLabel".Translate());
            for (int num = keys.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys[num]);
                if (incidentDef != null)
                {
                    var incidentChance = this.reaperShipIncidentChances[keys[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref incidentChance, incidentChance.ToStringDecimalIfSmall(), 0f, 5f);
                    this.reaperShipIncidentChances[keys[num]] = incidentChance;
                }
            }

            if (listingStandard.ButtonText("Reset".Translate()))
            {
                this.reaperShipIncidentChances["RE_ReaperLanding"] = 0.1f;
            }

            listingStandard.GapLine();
            listingStandard.Label("RER.AdjustReaperPresenceLabel".Translate());
            for (int num = keys2.Count - 1; num >= 0; num--)
            {
                var worldObjectDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail(keys2[num]);
                if (worldObjectDef != null)
                {
                    var reaperPresence = this.reaperShipPresences[keys2[num]];
                    listingStandard.SliderLabeled(worldObjectDef.label, ref reaperPresence, reaperPresence.ToString(), 0, 5000);
                    this.reaperShipPresences[keys2[num]] = reaperPresence;
                }
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                this.reaperShipPresences["RE_Reaper"]   = 500;
                foreach (var data in this.reaperShipPresences)
                {
                    var def = DefDatabase<WorldObjectDef>.GetNamed(data.Key, false);
                    if (def != null)
                    {
                        def.GetModExtension<ReaperBaseExtension>().raisesPresence = data.Value;
                    }
                }
            }
            listingStandard.GapLine();
            listingStandard.Label("RER.AdjustMinimumColonistCountLabel".Translate());
            for (int num = keys3.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys3[num]);
                if (incidentDef != null)
                {
                    var minimumCount = this.reaperShipColonistCount[keys3[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref minimumCount, minimumCount.ToString(), 0, 100);
                    this.reaperShipColonistCount[keys3[num]] = minimumCount;
                }
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {

                this.reaperShipColonistCount["RE_ReaperLanding"]   = 3;
                foreach (var data in this.reaperShipColonistCount)
                {
                    var def = DefDatabase<IncidentDef>.GetNamed(data.Key, false);
                    if (def != null)
                    {
                        def.GetModExtension<ReaperIncidentExtension>().minimumColonistCount = data.Value;
                    }
                }
            }

            listingStandard.GapLine();
            listingStandard.Label("RER.AdjustMinimumWealthLabel".Translate());
            for (int num = keys5.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys5[num]);
                if (incidentDef != null)
                {
                    var minimumWealth = this.reaperShipWealth[keys5[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref minimumWealth, minimumWealth.ToString("G"), 0, 1000000);
                    this.reaperShipWealth[keys5[num]] = minimumWealth;
                }
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {

                this.reaperShipWealth["RE_ReaperLanding"]   = 100000;
                foreach (var data in this.reaperShipWealth)
                {
                    IncidentDef def = DefDatabase<IncidentDef>.GetNamed(data.Key, false);
                    if (def != null)
                        def.GetModExtension<ReaperIncidentExtension>().minimumWealth = data.Value;
                }
            }

            listingStandard.GapLine();

            listingStandard.Label("RER.AdjustMaximumDistanceForReapersLabel".Translate());
            for (int num = keys4.Count - 1; num >= 0; num--)
            {
                var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(keys4[num]);
                if (incidentDef != null)
                {
                    var maximumDistance = this.reaperShipDistances[keys4[num]];
                    listingStandard.SliderLabeled(incidentDef.label, ref maximumDistance, maximumDistance.ToString(), 0, 1000);
                    this.reaperShipDistances[keys4[num]] = maximumDistance;
                }
            }
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                this.reaperShipDistances["RE_ReaperLanding"]   = 90;
                foreach (var data in this.reaperShipDistances)
                {
                    var def = DefDatabase<IncidentDef>.GetNamed(data.Key, false);
                    if (def != null)
                    {
                        def.GetModExtension<ReaperIncidentExtension>().maxDistance = data.Value;
                    }
                }
            }
            listingStandard.GapLine();
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}
