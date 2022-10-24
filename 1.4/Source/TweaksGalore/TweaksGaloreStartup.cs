﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace TweaksGalore
{
    [StaticConstructorOnStartup]
    public static class TweaksGaloreStartup
    {
        public static List<ThingDef> allMechanoidDefs = new List<ThingDef>();

        public static SimpleCurve original_maxDryadsPerConnectionStrengthCurve = new SimpleCurve();
        public static SimpleCurve adjusted_maxDryadsPerConnectionStrengthCurve = new SimpleCurve();

        public static SimpleCurve original_connectionLossPerLevelCurve = new SimpleCurve();
        public static SimpleCurve adjusted_connectionLossPerLevelCurve = new SimpleCurve();

        public static SimpleCurve original_connectionLossDailyPerBuildingDistanceCurve = new SimpleCurve();
        public static SimpleCurve adjusted_connectionLossDailyPerBuildingDistanceCurve = new SimpleCurve();

        static TweaksGaloreStartup()
        {
			TweaksGaloreSettings settings = TweaksGaloreMod.settings;

            try { CompatibilityChecks(settings); } catch (Exception e) { LogUtil.LogError("Caught exeption in Tweaks Galore startup: " + e); };
            try { Tweak_FixDeconstructionReturn(settings); } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: FixDeconstructionReturn :: " + e); };
            try { Tweak_StackableChunks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: StackableChunks :: " + e); };
            try { Tweak_MechanoidHeatArmor(settings); } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: MechanoidHeatArmor :: " + e); };
            try { Tweak_PowerUsageTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: PowerUsageTweaks :: " + e); };
            try { Tweak_StrongFloorsBlockInfestations(settings); } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: StrongFloorsBlockInfestations :: " + e); };
            try { Tweak_NoFriendShapedManhunters(settings);  } catch (Exception e) { LogUtil.LogError("Caught exception in Tweak: NoFriendShapedManhunters :: " + e); };
            if (ModLister.RoyaltyInstalled)
            {
                try { Tweak_MeditationAnyFocus(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during MeditationAnyFocus :: " + e); };

                if (settings.tweak_animaTweaks)
                {
                    try { Tweak_AnimaTweaksStartup(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during Anima Tweaks :: " + e); };
                }
            }
            if (ModLister.IdeologyInstalled)
            {
                if(settings.tweak_gauranlenTweaks)
                {
                    try { Tweak_GauranlenTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during Gauranlen Tweaks :: " + e); };
                }
            }
            if (ModLister.BiotechInstalled)
            {
                try { Tweak_GeneticsTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during Genetics Tweaks :: " + e); }

                if (settings.tweak_poluxTweaks)
                {
                    try { Tweak_PoluxTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during Polux Tweaks :: " + e); };
                }
                if (settings.tweak_mechanitorTweaks)
                {
                    try { Tweak_MechanitorTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during Mechanitor Tweaks :: " + e); };
                }
                if (settings.tweak_playerMechTweaks)
                {
                    try { Tweak_PlayerMechTweaks(settings); } catch (Exception e) { LogUtil.LogError("Caught exception during PlayerMech Tweaks :: " + e); };
                }
            }
        }

        public static void Tweak_MechanitorTweaks(TweaksGaloreSettings settings)
        {
            HediffDef implantDef = TGHediffDefOf.MechlinkImplant;
            HediffStage implantStage = implantDef.stages.First();
            implantStage.statOffsets.Find(sm => sm.stat == StatDefOf.MechBandwidth).value = settings.tweak_mechanitorBandwidthBase;
            implantStage.statOffsets.Find(sm => sm.stat == StatDefOf.MechControlGroups).value = settings.tweak_mechanitorControlGroupBase;
        }

        public static void Tweak_PlayerMechTweaksStartup(TweaksGaloreSettings settings)
        {
            List<ThingDef> mechanoidListing = new List<ThingDef>();
            if (settings.tweak_mechTweaksAffectMods)
            {
                mechanoidListing = DefDatabase<ThingDef>.AllDefs.Where(td => td.race?.IsMechanoid ?? false).ToList();
            }
            else
            {
                mechanoidListing = DefDatabase<ThingDef>.AllDefs.Where(td => (td.modContentPack.IsCoreMod || td.modContentPack.IsOfficialMod) && (td.race?.IsMechanoid ?? false)).ToList();
            }

            for (int i = 0; i < mechanoidListing.Count(); i++)
            {
                ThingDef curMech = mechanoidListing[i];
                // Handle Skill Level
                curMech.race.mechFixedSkillLevel = Mathf.RoundToInt(settings.tweak_mechanoidSkillLevel);
                // Handle Work Speed
                Tweak_PlayerMechWorkSpeed(settings, curMech);
            }

            Tweak_PlayerMechTweaks(settings);
        }

        public static void Tweak_PlayerMechTweaks(TweaksGaloreSettings settings)
        {
            StatDefOf.MechEnergyUsageFactor.defaultBaseValue = settings.tweak_mechanoidDrainRate;
        }

        public static void Tweak_PlayerMechWorkSpeed(TweaksGaloreSettings settings, ThingDef mech)
        {
            // TODO: Separate work speeds per work type!
        }

        public static void Tweak_GeneticsTweaks(TweaksGaloreSettings settings)
        {
            foreach (GeneDef gene in DefDatabase<GeneDef>.AllDefs)
            {
                if (settings.tweak_flattenComplexity)
                {
                    gene.biostatCpx = 0;
                }
                if (settings.tweak_flattenMetabolism)
                {
                    gene.biostatMet = 0;
                }
                if (settings.tweak_flattenArchites)
                {
                    gene.biostatArc = 0;
                }
            }
        }

        public static void Tweak_MeditationAnyFocus(TweaksGaloreSettings settings)
        {
            if (settings.tweak_animaMeditationAll)
            {
                List<ThingDef> meditationObjects = DefDatabase<ThingDef>.AllDefs.Where(td => td.GetCompProperties<CompProperties_MeditationFocus>() != null).ToList();
                List<MeditationFocusDef> focusDefs = DefDatabase<MeditationFocusDef>.AllDefs.ToList();

                for (int i = 0; i < meditationObjects.Count(); i++)
                {
                    ThingDef curObj = meditationObjects[i];
                    CompProperties_MeditationFocus curComp = curObj.GetCompProperties<CompProperties_MeditationFocus>();
                    for (int j = 0; j < focusDefs.Count(); j++)
                    {
                        if (!curComp.focusTypes.Contains(focusDefs[j]))
                        {
                            curComp.focusTypes.Add(focusDefs[j]);
                        }
                    }
                }
            }
        }

        public static void Tweak_AnimaTweaksStartup(TweaksGaloreSettings settings)
        {
            if (TweaksGaloreMod.mod.GetPsylinkStuff)
            {
                // Just loads the initial settings for psylink levels.
            }

            // Deal with the Tree Scream
            if (settings.tweak_animaDisableScream)
            {
                ThingDef animaTree = ThingDefOf.Plant_TreeAnima;
                animaTree.comps.Remove(animaTree.GetCompProperties<CompProperties_GiveThoughtToAllMapPawnsOnDestroy>());
                animaTree.comps.Remove(animaTree.GetCompProperties<CompProperties_PlaySoundOnDestroy>());
            }

            Tweak_AnimaTweaks(settings);
        }

        public static void Tweak_AnimaTweaks(TweaksGaloreSettings settings)
        {

            // Deal with the Tree Scream
            if(!settings.tweak_animaDisableScream)
            {
                ThoughtDef screamThought = TGThoughtDefOf.AnimaScream;
                screamThought.stages.First().baseMoodEffect = settings.tweak_animaScreamDebuff;
                screamThought.durationDays = settings.tweak_animaScreamLength;
                screamThought.stackLimit = Mathf.RoundToInt(settings.tweak_animaScreamStackLimit);
            }

            // Deal with Tree Radii
            CompProperties_MeditationFocus focus = ThingDefOf.Plant_TreeAnima.GetCompProperties<CompProperties_MeditationFocus>();

            FocusStrengthOffset_ArtificialBuildings artificialOffset =
                (FocusStrengthOffset_ArtificialBuildings)focus.offsets.Find(os => os.GetType() == typeof(FocusStrengthOffset_ArtificialBuildings));
            FocusStrengthOffset_BuildingDefs naturalOffset =
                (FocusStrengthOffset_BuildingDefs)focus.offsets.Find(os => os.GetType() == typeof(FocusStrengthOffset_BuildingDefs));

            artificialOffset.radius = settings.tweak_animaArtificialBuildingRadius;

            StatPart_ArtificialBuildingsNearbyOffset nearbyBuildingOffset = (StatPart_ArtificialBuildingsNearbyOffset)StatDefOf.MeditationPlantGrowthOffset.parts.Find(sp => sp.GetType() == typeof(StatPart_ArtificialBuildingsNearbyOffset));
            nearbyBuildingOffset.radius = settings.tweak_animaArtificialBuildingRadius;

            naturalOffset.radius = settings.tweak_animaBuffBuildingRadius;
            naturalOffset.maxBuildings = Mathf.RoundToInt(settings.tweak_animaMaxBuffBuildings);

            // Deal with grass requirements
            CompProperties_Psylinkable psycomp = ThingDefOf.Plant_TreeAnima.GetCompProperties<CompProperties_Psylinkable>();
            psycomp.requiredSubplantCountPerPsylinkLevel = settings.tweak_animaPsylinkLevelNeeds;


            // Get all viable shrines quietly
            List<ThingDef> shrineList = new List<ThingDef>();
            // Shrine Small
            ThingDef shrine_natureSmall = DefDatabase<ThingDef>.GetNamedSilentFail("NatureShrine_Small");
            if (shrine_natureSmall != null)
            {
                shrineList.Add(shrine_natureSmall);
            }
            // Shrine Large
            ThingDef shrine_natureLarge = DefDatabase<ThingDef>.GetNamedSilentFail("NatureShrine_Large");
            if (shrine_natureLarge != null)
            {
                shrineList.Add(shrine_natureLarge);
            }
            // Animus Stone
            ThingDef shrine_animusStone = DefDatabase<ThingDef>.GetNamedSilentFail("AnimusStone");
            if (shrine_animusStone != null)
            {
                shrineList.Add(shrine_animusStone);
            }
            // Runestone
            ThingDef shrine_runestone = DefDatabase<ThingDef>.GetNamedSilentFail("VFEV_RuneStone");
            if (shrine_runestone != null)
            {
                shrineList.Add(shrine_runestone);
            }

            // Deal with shrines
            if (!shrineList.NullOrEmpty())
            {
                foreach (ThingDef shrine in shrineList)
                {
                    CompProperties_MeditationFocus shrineFocus = shrine.GetCompProperties<CompProperties_MeditationFocus>();

                    FocusStrengthOffset_ArtificialBuildings shrineRange =
                        (FocusStrengthOffset_ArtificialBuildings)shrineFocus.offsets.Find(os => os.GetType() == typeof(FocusStrengthOffset_ArtificialBuildings));
                    FocusStrengthOffset_BuildingDefs shrineNatureRange =
                        (FocusStrengthOffset_BuildingDefs)focus.offsets.Find(os => os.GetType() == typeof(FocusStrengthOffset_BuildingDefs));

                    shrineRange.radius = settings.tweak_animaShrineBuildingRadius;
                    shrineNatureRange.radius = settings.tweak_animaShrineBuffBuildingRadius;
                }
            }

            // Deal with Psyfocus regen rate
            StatDefOf.MeditationFocusGain.defaultBaseValue = settings.tweak_animaMeditationGain;
        }

        public static void Tweak_GauranlenTweaks(TweaksGaloreSettings settings)
        {
            // Store Original Values If Needed
            CompProperties_TreeConnection originalProps = ThingDefOf.Plant_TreeGauranlen.GetCompProperties<CompProperties_TreeConnection>();
            if (original_connectionLossDailyPerBuildingDistanceCurve.EnumerableNullOrEmpty())
            {
                original_connectionLossDailyPerBuildingDistanceCurve = originalProps.connectionLossDailyPerBuildingDistanceCurve;
            }
            if (original_connectionLossPerLevelCurve.EnumerableNullOrEmpty())
            {
                original_connectionLossPerLevelCurve = originalProps.connectionLossPerLevelCurve;
            }
            if (original_maxDryadsPerConnectionStrengthCurve.EnumerableNullOrEmpty())
            {
                original_maxDryadsPerConnectionStrengthCurve = originalProps.maxDryadsPerConnectionStrengthCurve;
            }

            Tweak_GauranlenDryads(settings);
            Tweak_GauranlenMoss(settings);
            Tweak_GauranlenPods(settings);
        }

        public static void Tweak_GauranlenDryads(TweaksGaloreSettings settings)
        {
            // Deal with the Tree
            {
                CompProperties_TreeConnection treeProps = ThingDefOf.Plant_TreeGauranlen.GetCompProperties<CompProperties_TreeConnection>();
                treeProps.spawnDays = settings.tweak_gauranlenDryadSpawnDays;
                treeProps.initialConnectionStrengthRange = settings.tweak_gauranlenInitialConnectionStrength;
                treeProps.radiusToBuildingForConnectionStrengthLoss = settings.tweak_gauranlenArtificialBuildingRadius;
                treeProps.connectionStrengthGainPerHourPruningBase = settings.tweak_gauranlenConnectionGainPerHourPruning;
                {
                    adjusted_maxDryadsPerConnectionStrengthCurve = new SimpleCurve();
                    adjusted_maxDryadsPerConnectionStrengthCurve.Add(original_maxDryadsPerConnectionStrengthCurve.First());
                    adjusted_maxDryadsPerConnectionStrengthCurve.Add(new CurvePoint(0.76f, settings.tweak_gauranlenMaxDryads));

                    treeProps.maxDryadsPerConnectionStrengthCurve = adjusted_maxDryadsPerConnectionStrengthCurve;
                }
                {
                    adjusted_connectionLossPerLevelCurve = new SimpleCurve();
                    adjusted_connectionLossPerLevelCurve.Add(original_connectionLossPerLevelCurve.First());
                    adjusted_connectionLossPerLevelCurve.Add(new CurvePoint(0.66f, 0.06f * settings.tweak_gauranlenConnectionLossPerLevel));

                    treeProps.connectionLossPerLevelCurve = adjusted_connectionLossPerLevelCurve;
                }
                {
                    adjusted_connectionLossDailyPerBuildingDistanceCurve = new SimpleCurve();
                    adjusted_connectionLossDailyPerBuildingDistanceCurve.Add(new CurvePoint(0, 0.07f * settings.tweak_gauranlenLossPerBuilding));
                    adjusted_connectionLossDailyPerBuildingDistanceCurve.Add(new CurvePoint(settings.tweak_gauranlenArtificialBuildingRadius, 0.01f * settings.tweak_gauranlenLossPerBuilding));

                    treeProps.connectionLossDailyPerBuildingDistanceCurve = adjusted_connectionLossDailyPerBuildingDistanceCurve;
                }
            }
        }

        public static void Tweak_GauranlenMoss(TweaksGaloreSettings settings)
        {
            // Deal with the Moss
            {
                CompProperties_SpawnSubplant subPlantProps = ThingDefOf.Plant_TreeGauranlen.GetCompProperties<CompProperties_SpawnSubplant>();
                subPlantProps.maxRadius = settings.tweak_gauranlenPlantGrowthRadius;

                TGThingDefOf.Plant_MossGauranlen.plant.growDays = settings.tweak_gauranlenMossGrowDays;
            }
        }

        public static void Tweak_GauranlenPods(TweaksGaloreSettings settings)
        {
            // Deal with the Pods
            {
                CompProperties_DryadCocoon cocoonProps = ThingDefOf.DryadCocoon.GetCompProperties<CompProperties_DryadCocoon>();
                cocoonProps.daysToComplete = settings.tweak_gauranlenCocoonDaysToComplete;
            }
            {
                CompProperties_DryadCocoon cocoonprops = ThingDefOf.GaumakerCocoon.GetCompProperties<CompProperties_DryadCocoon>();
                cocoonprops.daysToComplete = settings.tweak_gauranlenGaumakerDaysToComplete;
            }
            {
                ThingDefOf.Plant_PodGauranlen.plant.growDays = settings.tweak_gauranlenMossGrowDays;
                ThingDefOf.Plant_PodGauranlen.plant.harvestYield = settings.tweak_gauranlenPodHarvestYield;
            }
        }

        public static void Tweak_PoluxTweaks(TweaksGaloreSettings settings)
        {
            // Deal with Tree Radii
            CompProperties_PollutionPump pollutionPumpComp = TGThingDefOf.Plant_TreePolux.GetCompProperties<CompProperties_PollutionPump>();
            pollutionPumpComp.radius = settings.tweak_poluxEffectRadius;
            pollutionPumpComp.intervalTicks = Mathf.RoundToInt(25000f * settings.tweak_poluxEffectRate);
            pollutionPumpComp.disabledByArtificialBuildings = !settings.tweak_poluxArtificialDisables;
        }

        public static void Tweak_NoFriendShapedManhunters(TweaksGaloreSettings settings)
        {
            // Tweak: No Friend Shaped Manhunters
            if (settings.tweak_noFriendShapedManhunters)
            {
                List<PawnKindDef> animalPawnKinds = DefDatabase<PawnKindDef>.AllDefs.Where(pk => pk.RaceProps.Animal).ToList();
                foreach (PawnKindDef def in animalPawnKinds)
                {
                    if((settings.tweak_NFSMTrainability_Intermediate && def.RaceProps.trainability == TrainabilityDefOf.Intermediate) ||
                        (settings.tweak_NFSMTrainability_Intermediate && def.RaceProps.trainability == TrainabilityDefOf.Advanced) ||
                        (settings.tweak_NFSMNuzzleHours && def.RaceProps.nuzzleMtbHours >= 0) ||
                        (settings.tweak_NFSMWildness > def.RaceProps.wildness) ||
                        (settings.tweak_NFSMCombatPower > def.combatPower))
                    {
                        def.canArriveManhunter = false;
                        if (settings.tweak_NFSMDisableManhunterOnTame)
                        {
                            def.race.race.manhunterOnTameFailChance = 0f;
                        }
                    }
                }
            }
        }

        public static void CompatibilityChecks(TweaksGaloreSettings settings)
        {
            if (ModLister.GetActiveModWithIdentifier("kentington.saveourship2") != null && settings.tweak_noBreakdowns)
            {
                LogUtil.LogWarning("SOS2 detected during startup. Disabling No Breakdowns tweak due to incompatibility.");
                settings.tweak_noBreakdowns = false;
            }
        }

        public static void Tweak_FixDeconstructionReturn(TweaksGaloreSettings settings)
        {
            // Tweak: Fix Deconstruction Return
            if (settings.tweak_fixDeconstructionReturn)
            {
                foreach (BuildableDef buildableDef in DefDatabase<BuildableDef>.AllDefs)
                {
                    buildableDef.resourcesFractionWhenDeconstructed = 1f;
                }
            }
        }

        public static void Tweak_StackableChunks(TweaksGaloreSettings settings)
        {
            // Tweak: Stackable Chunks
            if (settings.tweak_stackableChunks)
            {

                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    if (!thingDef.thingCategories.NullOrEmpty())
                    {
                        if (thingDef.thingCategories.Contains(ThingCategoryDefOf.StoneChunks))
                        {
                            thingDef.stackLimit = (int)settings.tweak_stackableChunks_stone;
                            thingDef.ResolveReferences();
                            thingDef.PostLoad();
                            ResourceCounter.ResetDefs();
                        }
                        if (thingDef.thingCategories.Contains(ThingCategoryDefOf.Chunks))
                        {
                            thingDef.stackLimit = (int)settings.tweak_stackableChunks_slag;
                            thingDef.ResolveReferences();
                            thingDef.PostLoad();
                            ResourceCounter.ResetDefs();
                        }
                    }
                }
            }
        }

        public static void Tweak_MechanoidHeatArmor(TweaksGaloreSettings settings)
        {
            // Tweak: Mechanoid Heat Armor
            if (!GetAllMechanoids.EnumerableNullOrEmpty())
            {
                foreach (ThingDef def in GetAllMechanoids)
                {
                    if (def != null & !def.defName.Contains("Corpse"))
                    {
                        StatModifier heatStat = def?.statBases?.Find(stat => stat.stat == StatDefOf.ArmorRating_Heat);
                        if (heatStat != null)
                        {
                            heatStat.value = settings.tweak_mechanoidHeatArmour;
                        }
                        else
                        {
                            LogUtil.LogMessage("O21 :: Tweaks Galore :: Mechanoid Recognised: " + def.defName + "/" + def.label + ", but not patched as it has no heat armour value. This is intended behaviour not an error.");
                        }
                    }
                }
            }
        }

        public static void Tweak_PowerUsageTweaks(TweaksGaloreSettings settings)
        {
            // Power Usage Tweaks
            if (settings.tweak_powerUsageTweaks)
            {
                List<ThingDef> standingLamps = new List<ThingDef>
                {
                    DefDatabase<ThingDef>.GetNamedSilentFail("StandingLamp"),
                    DefDatabase<ThingDef>.GetNamedSilentFail("StandingLamp_Red"),
                    DefDatabase<ThingDef>.GetNamedSilentFail("StandingLamp_Green"),
                    DefDatabase<ThingDef>.GetNamedSilentFail("StandingLamp_Blue")
                };

                if (!standingLamps.NullOrEmpty())
                {
                    for (int i = 0; i < standingLamps.Count; i++)
                    {
                        standingLamps[i].SetPowerUsage((int)settings.tweak_powerUsage_lamp);
                    }
                }

                DefDatabase<ThingDef>.GetNamedSilentFail("SunLamp").SetPowerUsage((int)settings.tweak_powerUsage_sunlamp);
                DefDatabase<ThingDef>.GetNamedSilentFail("Autodoor").SetPowerUsage((int)settings.tweak_powerUsage_autodoor);
                DefDatabase<ThingDef>.GetNamedSilentFail("VanometricPowerCell").SetPowerUsage(-(int)settings.tweak_powerUsage_vanometricCell);
            }
        }

        public static void Tweak_StrongFloorsBlockInfestations(TweaksGaloreSettings settings)
        {
            // Strong Floors Block Infestations
            if (settings.patch_strongFloorsStopInfestations)
            {
                List<TerrainDef> strongFloors = new List<TerrainDef>();

                foreach (TerrainDef terrain in DefDatabase<TerrainDef>.AllDefs)
                {
                    if (terrain != null)
                    {
                        if (terrain.costList != null && terrain.costList.Any(t => t.thingDef.stuffProps != null && ((bool)(t.thingDef?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Metallic)) || (bool)(t.thingDef?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Stony)))))
                        {
                            strongFloors.Add(terrain);
                        }
                        else if (!terrain.stuffCategories.NullOrEmpty() && (terrain.stuffCategories.Contains(StuffCategoryDefOf.Metallic) || terrain.stuffCategories.Contains(StuffCategoryDefOf.Stony)))
                        {
                            strongFloors.Add(terrain);
                        }
                    }
                }

                for (int i = 0; i < strongFloors.Count; i++)
                {
                    if (strongFloors[i].tags == null)
                    {
                        strongFloors[i].tags = new List<string>();
                    }
                    strongFloors[i].tags.Add("BlocksInfestations");
                }
            }
        }

        public static void SetPowerUsage(this ThingDef thing, int powerConsumption)
        {
            if(thing != null)
            {
                CompProperties_Power comp = thing.GetCompProperties<CompProperties_Power>();
                if(comp != null)
                {
                    thing.GetCompProperties<CompProperties_Power>().basePowerConsumption = powerConsumption;
                }
            }
        }

        public static List<ThingDef> GetAllMechanoids
        {
            get
            {
                if (allMechanoidDefs.NullOrEmpty())
                {
                    IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                                       where def?.race?.IsMechanoid ?? false
                                                       select def;

                    allMechanoidDefs = enumerable.ToList();

                    ThingDef alpha = DefDatabase<ThingDef>.GetNamedSilentFail("O21_Alien_MechadroidAlpha");
                    ThingDef delta = DefDatabase<ThingDef>.GetNamedSilentFail("O21_Alien_MechadroidDelta");
                    ThingDef gamma = DefDatabase<ThingDef>.GetNamedSilentFail("O21_Alien_MechadroidGamma");

                    if (alpha != null) { allMechanoidDefs.Add(alpha); }
                    if (delta != null) { allMechanoidDefs.Add(delta); }
                    if (gamma != null) { allMechanoidDefs.Add(gamma); }
                }
                return allMechanoidDefs;
            }
        }
    }
}
