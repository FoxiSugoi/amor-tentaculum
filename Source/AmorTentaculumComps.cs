using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AmorTentaculum
{
    public class HediffCompProperties_MultipliedSeverityPerDay : HediffCompProperties_SeverityPerDay
    {


        public required float baseSeverityPerDay;
        public required string modSettingMultiplier;
        private float settingMultiplier;

        public HediffCompProperties_MultipliedSeverityPerDay()
        {
            compClass = typeof(HediffComp_MultipliedSeverityByDay);
        }

        // I'm sure there's a better way to do this, but since this is specifically for our mod, we'll just leave it for now.
        public new float CalculateSeverityPerDay()
        {
            switch (modSettingMultiplier)
            {
                case "hostIncubationSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.hostIncubationSpeedFactor;
                    break;
                case "geneticRewriteSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.geneticRewriteSpeedFactor;
                    break;
                case "hostEggProductionSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.hostEggProductionSpeedFactor;
                    break;
                case "hostResourceProductionSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.hostResourceProductionSpeedFactor;
                    break;
                case "tentacleEggProductionSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.tentacleEggProductionSpeedFactor;
                    break;
                case "tentacleResourceProductionSpeedFactor":
                    settingMultiplier = AmorTentaculumModSettings.tentacleResourceProductionSpeedFactor;
                    break;
                default:
                    settingMultiplier = 1f;
                    break;
            }

            severityPerDay = baseSeverityPerDay * settingMultiplier;
            return severityPerDay;
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (modSettingMultiplier == null)
                yield return "multplier value needs to be set.";

            if (baseSeverityPerDay == 0f)
                yield return "base severity value cannot be 0.";
        }
    }

    public class HediffComp_MultipliedSeverityByDay : HediffComp_SeverityPerDay
    {
        public required float baseSeverityPerDay;
        public required string modSettingMultiplier;
        // private float settingMultiplier;
        private HediffCompProperties_MultipliedSeverityPerDay Props => (HediffCompProperties_MultipliedSeverityPerDay)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            severityPerDay = Props.CalculateSeverityPerDay();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref severityPerDay, "severityPerDay", 0f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && severityPerDay == 0f && Props.severityPerDay != 0f && Props.severityPerDayRange == FloatRange.Zero)
            {
                severityPerDay = Props.CalculateSeverityPerDay();
                Log.Warning("Hediff " + parent.Label + " had severityPerDay not matching parent.");
            }
        }

        public override float SeverityChangePerDay()
        {
            // Log.Message("[<color='#CB78E3'>Amor Tentaculum</color>] DEBUG: Calculating hediff severity...");

            float num = severityPerDay * (parent.CurStage?.severityGainFactor ?? 1f);
            if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(base.Pawn))
            {
                num *= Props.mechanitorFactor;
            }

            return num;
        }

    }

    public class CompProperties_FactionHatcher : CompProperties
    {

        public float hatcherDaystoHatch = 1f;

        public PawnKindDef hatcherPawn;
    
        public CompProperties_FactionHatcher()
        {
            compClass = typeof(CompFactionHatcher);
        }
    }

    public class CompFactionHatcher : ThingComp
    {

        private float gestateProgress;
        public Pawn hatcheeParent;
        public Pawn otherParent;
        public Faction hatcheeFaction;
        public CompProperties_FactionHatcher Props => (CompProperties_FactionHatcher)props;
        private CompTemperatureRuinable FreezerComp => parent.GetComp<CompTemperatureRuinable>();

        public bool TemperatureDamaged
        {
            get
            {
                if (FreezerComp != null)
                {
                    return FreezerComp.Ruined;
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gestateProgress, "gestateProgress", 0f);
            Scribe_References.Look(ref hatcheeParent, "hatcheeParent");
            Scribe_References.Look(ref otherParent, "otherParent");
            Scribe_References.Look(ref hatcheeFaction, "hatcheeFaction");
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(parent.stackCount + count);
            float b = ((ThingWithComps)otherStack).GetComp<CompFactionHatcher>().gestateProgress;
            gestateProgress = Mathf.Lerp(gestateProgress, b, t);
        }

        public override bool AllowStackWith(Thing other)
        {
            CompFactionHatcher comp = ((ThingWithComps)other).GetComp<CompFactionHatcher>();
            if (TemperatureDamaged != comp.TemperatureDamaged)
            {
                return false;
            }
            return base.AllowStackWith(other);
        }

        public override void PostSplitOff(Thing piece)
        {
            CompFactionHatcher comp = ((ThingWithComps)piece).GetComp<CompFactionHatcher>();
            comp.gestateProgress = gestateProgress;
            comp.hatcheeParent = hatcheeParent;
            comp.otherParent = otherParent;
            comp.hatcheeFaction = hatcheeFaction;
        }

        public override void CompTick()
        {

            if (!TemperatureDamaged)
            {
                float num = 1f / (Props.hatcherDaystoHatch * 60000f);
                gestateProgress += num;
                if (gestateProgress > 1f)
                {
                    FactionHatch();
                }
                return;
            }
            CompProperties_EggLayer compProperties_EggLayer = Props.hatcherPawn?.race?.GetCompProperties<CompProperties_EggLayer>();
            if (compProperties_EggLayer != null)
            {
                if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
                {
                    pawn_CarryTracker.TryDropCarriedThing(parent.Position, ThingPlaceMode.Near, out var _);
                }
                if (parent.ParentHolder is CompEggContainer compEggContainer)
                {
                    int stackCount = parent.stackCount;
                    compEggContainer.innerContainer.Remove(parent);
                    parent.Destroy();
                    Thing thing = ThingMaker.MakeThing(compProperties_EggLayer.eggUnfertilizedDef);
                    thing.stackCount = stackCount;
                    compEggContainer.innerContainer.TryAdd(thing);
                }
                else
                {
                    Map mapHeld = parent.MapHeld;
                    IntVec3 positionHeld = parent.PositionHeld;
                    int stackCount2 = parent.stackCount;
                    parent.Destroy();
                    GenSpawn.Spawn(compProperties_EggLayer.eggUnfertilizedDef, positionHeld, mapHeld).stackCount = stackCount2;
                }
            }
        }

        public void FactionHatch()
        {
            try
            {
                PawnGenerationRequest request = new PawnGenerationRequest(Props.hatcherPawn, Faction.OfPlayer, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
                if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
                {
                    pawn_CarryTracker.TryDropCarriedThing(parent.PositionHeld, ThingPlaceMode.Near, out var _);
                }
                for (int i = 0; i < parent.stackCount; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
                    {
                        if (pawn != null)
                        {
                            if (hatcheeParent != null)
                            {
                                if (pawn.playerSettings != null && hatcheeParent.playerSettings != null && hatcheeParent.Faction == hatcheeFaction)
                                {
                                    pawn.playerSettings.AreaRestrictionInPawnCurrentMap = hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
                                }
                                if (pawn.RaceProps.IsFlesh)
                                {
                                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, hatcheeParent);
                                }
                            }
                            if (otherParent != null && (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) && pawn.RaceProps.IsFlesh)
                            {
                                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, otherParent);
                            }
                        }
                        if (parent.Spawned)
                        {
                            FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_AmnioticFluid);
                        }
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    }
                }
            }
            finally
            {
                parent.Destroy();
            }
        }

        public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PrePreTraded(action, playerNegotiator, trader);
            switch (action)
            {
                case TradeAction.PlayerBuys:
                    hatcheeFaction = Faction.OfPlayer;
                    break;
                case TradeAction.PlayerSells:
                    hatcheeFaction = trader.Faction;
                    break;
            }
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
            hatcheeFaction = forFaction;
        }

        public override string CompInspectStringExtra()
        {
            if (!TemperatureDamaged)
            {
                return "EggProgress".Translate() + ": " + gestateProgress.ToStringPercent() + "\n" + "HatchesIn".Translate() + ": " + "PeriodDays".Translate((Props.hatcherDaystoHatch * (1f - gestateProgress)).ToString("F1"));
            }
            return null;
        }

    }


}