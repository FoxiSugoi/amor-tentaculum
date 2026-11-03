using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AmorTentaculum
{

    [StaticConstructorOnStartup]
    public static class AmorTentaculumStartup
    {
        static AmorTentaculumStartup()
        {
            Log.Message("[<color='#CB78E3'>Amor Tentaculum</color>] Initializing mod.");
        }
    }

    public class AmorTentaculumMod : Mod
    {
        AmorTentaculumModSettings settings;

        public override string SettingsCategory()
        {
            return "Amor Tentaculum";
        }

        public AmorTentaculumMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<AmorTentaculumModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            AmorTentaculumModSettings.DoWindowContents(inRect);
        }

    }

    public class AmorTentaculumModSettings : ModSettings
    {
        /// <summary>
        /// Modifier to Host Incubation Speed
        /// </summary>
        public static float hostIncubationSpeedFactor = 1.0f;
        
        /// <summary>
        /// Modifier to Host Genetic Rewrite Speed
        /// </summary>
        public static float geneticRewriteSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Host Genetic Rewrite Speed
        /// </summary>
        public static float hostAcclimationSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Host Egg Production Speed
        /// </summary>
        public static float hostEggProductionSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Host Resource Production Speed
        /// </summary>
        public static float hostResourceProductionSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Tentacle Resource Production Speed
        /// </summary>
        public static float tentacleResourceProductionSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Tentacle Resource Production Speed
        /// </summary>
        public static float larvaEvolutionSpeedFactor = 1.0f;

        /// <summary>
        /// Modifier to Tentacle Egg Production Speed
        /// </summary>
        public static float tentacleEggProductionSpeedFactor = 1.0f;

        private static Vector2 scrollPosition = Vector2.zero;
        private static float dynamicContentHeight = 100f;
        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 50f);

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, dynamicContentHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);

            listingStandard.Begin(viewRect);

            listingStandard.Label("Note that modifications to these settings will require a restart to take effect!");

            listingStandard.GapLine(16f);

            hostIncubationSpeedFactor = listingStandard.SliderLabeled(
                "Host Incubation Speed Factor: {0}x".Translate(hostIncubationSpeedFactor.ToString("0.00")),
                hostIncubationSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed that the Host egg incubation hediff progresses.  This is the initial stage of Host transformation.");

            listingStandard.GapLine(16f);

            geneticRewriteSpeedFactor = listingStandard.SliderLabeled(
                "Genetic Rewrite Speed Factor: {0}x".Translate(geneticRewriteSpeedFactor.ToString("0.00")),
                geneticRewriteSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed that the Host genetic rewriting hediff progresses.  This is the intermediate stage of Host transformation.");
            
            listingStandard.GapLine(16f);

            hostAcclimationSpeedFactor = listingStandard.SliderLabeled(
                "Host Acclimation Speed Factor: {0}x".Translate(hostAcclimationSpeedFactor.ToString("0.00")),
                hostAcclimationSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed that the Host acclimation hediffs progress.  This is the final stage of Host transformation.");

            listingStandard.GapLine(16f);

            hostEggProductionSpeedFactor = listingStandard.SliderLabeled(
                "Host Egg Production Speed Factor: {0}x".Translate(hostEggProductionSpeedFactor.ToString("0.00")),
                hostEggProductionSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed at which Hosts incubate eggs.");

            listingStandard.GapLine(16f);

            hostResourceProductionSpeedFactor = listingStandard.SliderLabeled(
                "Host Resource Production Speed Factor: {0}x".Translate(hostResourceProductionSpeedFactor.ToString("0.00")),
                hostResourceProductionSpeedFactor, 0.25f, 4f, 0.35f,
               "The speed at which Hosts produce resources.");

            listingStandard.GapLine(16f);

            larvaEvolutionSpeedFactor = listingStandard.SliderLabeled(
               "Larva Evolution Speed Factor: {0}x".Translate(larvaEvolutionSpeedFactor.ToString("0.00")),
                larvaEvolutionSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed at which Larvae evolve.");

            listingStandard.GapLine(16f);

            tentacleEggProductionSpeedFactor = listingStandard.SliderLabeled(
                "Tentacle Egg Production Speed Factor: {0}x".Translate(tentacleEggProductionSpeedFactor.ToString("0.00")),
                tentacleEggProductionSpeedFactor, 0.25f, 4f, 0.35f,
                "The speed at which Tentacles incubate eggs.");

            listingStandard.GapLine(16f);

            tentacleResourceProductionSpeedFactor = listingStandard.SliderLabeled(
                "Tentacle Resource Production Speed Factor: {0}x".Translate(tentacleResourceProductionSpeedFactor.ToString("0.00")),
                tentacleResourceProductionSpeedFactor, 0.25f, 4f, 0.35f,
               "The speed at which Tentacles produce resources.");
            listingStandard.GapLine(16f);

            if (listingStandard.ButtonText("Reset Speed Factors"))
            {
                hostIncubationSpeedFactor = 1f;
                geneticRewriteSpeedFactor = 1f;
                hostAcclimationSpeedFactor = 1f;
                hostEggProductionSpeedFactor = 1f;
                hostResourceProductionSpeedFactor = 1f;
                tentacleEggProductionSpeedFactor = 1f;
                tentacleResourceProductionSpeedFactor = 1f;
            }


            float totalHeightUsed = listingStandard.CurHeight;

            listingStandard.End();
            Widgets.EndScrollView();
            dynamicContentHeight = totalHeightUsed + 20f;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref hostIncubationSpeedFactor, "hostIncubationSpeedFactor", 1f);
            Scribe_Values.Look(ref geneticRewriteSpeedFactor, "geneticRewriteSpeedFactor", 1f);
            Scribe_Values.Look(ref hostAcclimationSpeedFactor, "hostAcclimationSpeedFactor", 1f);
            Scribe_Values.Look(ref hostEggProductionSpeedFactor, "hostEggProductionSpeedFactor", 1f);
            Scribe_Values.Look(ref hostResourceProductionSpeedFactor, "hostResourceProductionSpeedFactor", 1f);
            Scribe_Values.Look(ref tentacleEggProductionSpeedFactor, "tentacleEggProductionSpeedFactor", 1f);
            Scribe_Values.Look(ref tentacleResourceProductionSpeedFactor, "tentacleResourceProductionSpeedFactor", 1f);

            base.ExposeData();
        }
    }
}