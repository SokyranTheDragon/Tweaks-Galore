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
        public static TweaksGaloreSettings settings = TweaksGaloreMod.settings;

        static TweaksGaloreStartup()
        {
            foreach(TweakSectionDef section in DefDatabase<TweakSectionDef>.AllDefs)
            {
                try
                {
                    section.Worker.DoOnStartup();
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"Caught Exception initialising '{section.defName}' section:\n" + e);
                }
            }

            foreach(TweakDef tweak in DefDatabase<TweakDef>.AllDefs)
            {
                try
                {
                    tweak.Worker.OnStartup();
                }
                catch (Exception e)
                {
                    LogUtil.LogError($"Caught Exception initialising '{tweak.defName}' tweak:\n" + e);
                }
            }

            try { InitializeSettingsDefs(settings); } catch (Exception e) { LogUtil.LogError("Caught Exception initialising settings: " + e); };

            try { InitializePositionArrangements(settings); } catch (Exception e) { LogUtil.LogError("Caught Exception initialising position arrangements: " + e); };

            try { settings.ConvertOldSettings(); } catch (Exception e) { LogUtil.LogError("Caught Exception converting pre-overhaul settings: " + e); };

            try { CompatibilityChecks(settings); } catch (Exception e) { LogUtil.LogError("Caught exeption in compatibility checks: " + e); };
        }

        public static void InitializePositionArrangements(TweaksGaloreSettings settings)
        {
            foreach(TweakDef tweak in DefDatabase<TweakDef>.AllDefs)
            {
                if (tweak.subSection != null)
                {
                    tweak.subSection.AddTweak(tweak);
                }
                if (!tweak.subSections.NullOrEmpty())
                {
                    for (int i = 0; i < tweak.subSections.Count; i++)
                    {
                        tweak.subSections[i].AddTweak(tweak);
                    }
                }
                if (tweak.section != null)
                {
                    tweak.section.AddTweak(tweak);
                }
                if (!tweak.sections.NullOrEmpty())
                {
                    for (int i = 0; i < tweak.sections.Count; i++)
                    {
                        tweak.sections[i].AddTweak(tweak);
                    }
                }
            }
            foreach(TweakSubSectionDef subSection in DefDatabase<TweakSubSectionDef>.AllDefs)
            {
                if(subSection.section != null)
                {
                    subSection.section.AddSubSection(subSection);
                }
                if (!subSection.sections.NullOrEmpty())
                {
                    for (int i = 0; i < subSection.sections.Count; i++)
                    {
                        subSection.sections[i].AddSubSection(subSection);
                    }
                }
            }
            foreach(TweakSectionDef section in DefDatabase<TweakSectionDef>.AllDefs)
            {
                if(section.category != null)
                {
                    section.category.AddSection(section);
                }
                if (!section.categories.NullOrEmpty())
                {
                    for (int i = 0; i < section.categories.Count; i++)
                    {
                        section.categories[i].AddSection(section);
                    }
                }
            }
        }

        public static void AddTweak(this TweakBaseDef holder, TweakDef tweak)
        {
            if (holder.heldTweaks.NullOrEmpty())
            {
                holder.heldTweaks = new List<TweakDef>();
            }
            if (!holder.heldTweaks.Contains(tweak))
            {
                holder.heldTweaks.Add(tweak);
            }
        }

        public static void AddSubSection(this TweakSectionDef holder, TweakSubSectionDef subSect)
        {
            if (holder.heldSubSections.NullOrEmpty())
            {
                holder.heldSubSections = new List<TweakSubSectionDef>();
            }
            if (!holder.heldSubSections.Contains(subSect))
            {
                holder.heldSubSections.Add(subSect);
            }
        }

        public static void AddSection(this TweakCategoryDef holder, TweakSectionDef sect)
        {
            if (holder.heldSections.NullOrEmpty())
            {
                holder.heldSections = new List<TweakSectionDef>();
            }
            if (!holder.heldSections.Contains(sect))
            {
                holder.heldSections.Add(sect);
            }
        }

        public static void InitializeSettingsDefs(TweaksGaloreSettings settings)
        {
            foreach(TweakDef def in DefDatabase<TweakDef>.AllDefs)
            {
                switch (def.tweakType)
                {
                    case TweakType.Bool:
                        RegisterTweak_Bool(def, settings);
                        break;
                    case TweakType.Float:
                        RegisterTweak_Float(def, settings);
                        break;
                    case TweakType.FloatRange:
                        RegisterTweak_FloatRange(def, settings);
                        break;
                    case TweakType.Int:
                        RegisterTweak_Int(def, settings);
                        break;
                    case TweakType.IntRange:
                        RegisterTweak_IntRange(def, settings);
                        break;
                    case TweakType.Custom:
                        break;
                    default:
                        LogUtil.LogError($"TweakDef {def.defName} has an invalid <tweakType> value. Accepted values are Bool, Float or Int.");
                        break;
                }
            }
        }

        public static void RegisterTweak_Bool(TweakDef def, TweaksGaloreSettings s)
        {
            s.GetBoolSetting(def.defName, def.DefaultBool);
        }

        public static void RegisterTweak_Float(TweakDef def, TweaksGaloreSettings s)
        {
            s.GetFloatSetting(def.defName, def.DefaultFloat);
        }

        public static void RegisterTweak_FloatRange(TweakDef def, TweaksGaloreSettings s)
        {
            s.GetFloatRangeSetting(def.defName, def.DefaultFloatRange);
        }

        public static void RegisterTweak_Int(TweakDef def, TweaksGaloreSettings s)
        {
            s.GetIntSetting(def.defName, def.DefaultInt);
        }

        public static void RegisterTweak_IntRange(TweakDef def, TweaksGaloreSettings s)
        {
            s.GetIntRangeSetting(def.defName, def.DefaultIntRange);
        }

        public static void Tweak_PoluxTweaks(TweaksGaloreSettings settings)
        {
            // Deal with Tree Radii
            CompProperties_PollutionPump pollutionPumpComp = TGThingDefOf.Plant_TreePolux.GetCompProperties<CompProperties_PollutionPump>();
            pollutionPumpComp.radius = settings.tweak_poluxEffectRadius;
            pollutionPumpComp.intervalTicks = Mathf.RoundToInt(25000f * settings.tweak_poluxEffectRate);
            pollutionPumpComp.disabledByArtificialBuildings = !settings.tweak_poluxArtificialDisables;
        }

        public static void CompatibilityChecks(TweaksGaloreSettings settings)
        {
            if (ModLister.GetActiveModWithIdentifier("kentington.saveourship2") != null && settings.tweak_noBreakdowns)
            {
                LogUtil.LogWarning("SOS2 detected during startup. Disabling No Breakdowns tweak due to incompatibility.");
                settings.tweak_noBreakdowns = false;
            }
        }
    }
}
