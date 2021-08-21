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

    public class ReaperMod : Mod
    {
        public static ReaperSettings settings;
        public ReaperMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<ReaperSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RimEffect - Asari & Reapers";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsAlterer.DoDefsAlter();
        }
    }
    [StaticConstructorOnStartup]
    public static class DefsAlterer
    {
        static DefsAlterer()
        {
            Setup();
            DoDefsAlter();
        }

        public static void Setup()
        {
            ReaperMod.settings.incidents = DefDatabase<IncidentDef>.AllDefsListForReading.Where(x => x.defName.StartsWith("RE_ReaperLand")).ToList();

            if (ReaperMod.settings.reaperShipIncidentChances is null)
            {
                ReaperMod.settings.reaperShipIncidentChances = new Dictionary<string, float>();
            }
            ReaperMod.settings.reaperShipIncidentChances.RemoveAll(x => DefDatabase<IncidentDef>.GetNamed(x.Key, false) is null);

            if (ReaperMod.settings.reaperShipPresences is null)
            {
                ReaperMod.settings.reaperShipPresences = new Dictionary<string, int>();
            }

            if (ReaperMod.settings.reaperShipColonistCount is null)
            {
                ReaperMod.settings.reaperShipColonistCount = new Dictionary<string, int>();
            }

            if (ReaperMod.settings.reaperShipWealth is null)
            {
                ReaperMod.settings.reaperShipWealth = new Dictionary<string, int>();
            }

            if (ReaperMod.settings.reaperShipDistances is null)
            {
                ReaperMod.settings.reaperShipDistances = new Dictionary<string, int>();
            }

            foreach (var reaperShip in ReaperMod.settings.incidents)
            {
                if (!ReaperMod.settings.reaperShipIncidentChances.ContainsKey(reaperShip.defName))
                {
                    ReaperMod.settings.reaperShipIncidentChances[reaperShip.defName] = reaperShip.baseChance;
                }
                reaperShip.baseChance = 0;

                ReaperIncidentExtension incidentExtension = reaperShip.GetModExtension<ReaperIncidentExtension>();
                if (!ReaperMod.settings.reaperShipPresences.ContainsKey(incidentExtension.baseToPlace.defName))
                {
                    int presence = incidentExtension.baseToPlace.GetModExtension<ReaperBaseExtension>().raisesPresence;
                    ReaperMod.settings.reaperShipPresences[incidentExtension.baseToPlace.defName] = presence;
                }

                if (!ReaperMod.settings.reaperShipColonistCount.ContainsKey(reaperShip.defName))
                {
                    ReaperMod.settings.reaperShipColonistCount[reaperShip.defName] = incidentExtension.minimumColonistCount;
                }
                
                if (!ReaperMod.settings.reaperShipWealth.ContainsKey(reaperShip.defName))
                {
                    ReaperMod.settings.reaperShipWealth[reaperShip.defName] = incidentExtension.minimumWealth;
                }

                if (!ReaperMod.settings.reaperShipDistances.ContainsKey(reaperShip.defName))
                {
                    ReaperMod.settings.reaperShipDistances[reaperShip.defName] = incidentExtension.maxDistance;
                }
            }
        }

        public static void DoDefsAlter()
        {
            foreach (IncidentDef incidentDef in ReaperMod.settings.incidents)
            {
                ReaperIncidentExtension incidentExtension = incidentDef.GetModExtension<ReaperIncidentExtension>();

                incidentExtension.minimumColonistCount = ReaperMod.settings.reaperShipColonistCount[incidentDef.defName];
                incidentExtension.maxDistance = ReaperMod.settings.reaperShipDistances[incidentDef.defName];


            }

            foreach (KeyValuePair<string, int> reaperShipPresence in ReaperMod.settings.reaperShipPresences)
            {
                var defToAlter = DefDatabase<WorldObjectDef>.GetNamedSilentFail(reaperShipPresence.Key);
                if (defToAlter != null)
                {
                    defToAlter.GetModExtension<ReaperBaseExtension>().raisesPresence = reaperShipPresence.Value;
                }
            }
        }
    }
}
