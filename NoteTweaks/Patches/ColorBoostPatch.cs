using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NoteColorTweaks
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        internal static Color OriginalLeftColor;
        internal static Color OriginalRightColor;
        
        private static ColorScheme PatchColors(ColorScheme scheme)
        {
            float leftScale = 1.0f + Config.ColorBoostLeft;
            float rightScale = 1.0f + Config.ColorBoostRight;
            
            if (OriginalLeftColor != scheme._saberAColor && OriginalLeftColor != (scheme._saberAColor * leftScale))
            {
                OriginalLeftColor = scheme._saberAColor;
            }
            if (OriginalRightColor != scheme._saberBColor && OriginalRightColor != (scheme._saberBColor * rightScale))
            {
                OriginalRightColor = scheme._saberBColor;
            }
            
            Color leftColor = OriginalLeftColor;
            float leftBrightness = leftColor.Brightness();
            Color rightColor = OriginalRightColor;
            float rightBrightness = rightColor.Brightness();

            if (leftBrightness > Config.LeftMaxBrightness)
            {
                leftColor = leftColor.LerpRGBUnclamped(Color.black, Mathf.InverseLerp(leftBrightness, 0.0f, Config.LeftMaxBrightness));
            }
            else if (leftBrightness < Config.LeftMinBrightness)
            {
                leftColor = leftColor.LerpRGBUnclamped(Color.white, Mathf.InverseLerp(leftBrightness, 1.0f, Config.LeftMinBrightness));
            }
            
            if (rightBrightness > Config.RightMaxBrightness)
            {
                rightColor = rightColor.LerpRGBUnclamped(Color.black, Mathf.InverseLerp(rightBrightness, 0.0f, Config.RightMaxBrightness));
            }
            else if (rightBrightness < Config.RightMinBrightness)
            {
                rightColor = rightColor.LerpRGBUnclamped(Color.white, Mathf.InverseLerp(rightBrightness, 1.0f, Config.RightMinBrightness));
            }
            
            scheme._saberAColor = leftColor * leftScale;
            scheme._saberBColor = rightColor * rightScale;

            return scheme;
        }

        // i honestly did not think this would touch save data. but it does! what the shit
        private static bool _didApplySaveFix;
        [HarmonyPatch(typeof(PlayerDataModel), "Save")]
        [HarmonyPatch(typeof(PlayerDataModel), "SaveAsync")]
        [HarmonyPrefix]
        // ReSharper disable once InconsistentNaming
        private static bool SaveFix(PlayerDataModel __instance)
        {
            if (SceneManager.GetActiveScene().name != "GameCore" || !Config.Enabled)
            {
                return true;
            }
            
            ColorSchemesSettings settings = __instance.playerData.colorSchemesSettings;
            ColorScheme selectedScheme = settings.GetSelectedColorScheme();
            float leftScale = 1.0f + Config.ColorBoostLeft;
            float rightScale = 1.0f + Config.ColorBoostRight;
            
            if (selectedScheme._saberAColor == OriginalLeftColor * leftScale && selectedScheme._saberBColor == OriginalRightColor * rightScale)
            {
                if (selectedScheme.colorSchemeId.Contains("User") && settings.overrideDefaultColors)
                {
                    _didApplySaveFix = true;
                    Plugin.Log.Info("Applied color scheme save data fix");
                    
                    selectedScheme._saberAColor = OriginalLeftColor;
                    selectedScheme._saberBColor = OriginalRightColor;
                    settings.SetColorSchemeForId(selectedScheme);
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(PlayerDataModel), "Save")]
        [HarmonyPatch(typeof(PlayerDataModel), "SaveAsync")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void SaveFixUndo(PlayerDataModel __instance)
        {
            if (SceneManager.GetActiveScene().name != "GameCore" || !Config.Enabled)
            {
                return;
            }
            
            ColorSchemesSettings settings = __instance.playerData.colorSchemesSettings;
            ColorScheme selectedScheme = settings.GetSelectedColorScheme();
            
            if (_didApplySaveFix)
            {
                _didApplySaveFix = false;
                settings.SetColorSchemeForId(PatchColors(selectedScheme));
                Plugin.Log.Info("Undid color scheme save data fix, data has been saved");
            }
        }
        
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitColorInfo")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void InitColorInfoPatch(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            ColorScheme patchedColors = PatchColors(__instance.colorScheme);

            if (patchedColors._saberAColor == __instance.colorScheme._saberAColor &&
                patchedColors._saberBColor == __instance.colorScheme._saberBColor)
            {
                return;
            }

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
        }

        [HarmonyPatch]
        internal class MissionInitPatch
        {
            [UsedImplicitly]
            static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(MissionLevelScenesTransitionSetupDataSO),
                m => m.Name == nameof(MissionLevelScenesTransitionSetupDataSO.Init) &&
                     m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));
            
            // ReSharper disable once InconsistentNaming
            internal static void Postfix(MissionLevelScenesTransitionSetupDataSO __instance)
            {
                if (!Config.Enabled)
                {
                    return;
                }
                
                __instance.gameplayCoreSceneSetupData.SetField("colorScheme", PatchColors(__instance.gameplayCoreSceneSetupData.colorScheme));
            }
        }

        [HarmonyPatch(typeof(StandardLevelRestartController), "RestartLevel")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void StandardLevelRestartControllerPatch(StandardLevelRestartController __instance)
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            ColorScheme oldScheme = __instance._standardLevelSceneSetupData.colorScheme;
            oldScheme._saberAColor = OriginalLeftColor;
            oldScheme._saberBColor = OriginalRightColor;
            ColorScheme patchedColors = PatchColors(oldScheme);

            if (patchedColors._saberAColor == oldScheme._saberAColor &&
                patchedColors._saberBColor == oldScheme._saberBColor)
            {
                return;
            }
            
            __instance._standardLevelSceneSetupData.usingOverrideColorScheme = true;
            __instance._standardLevelSceneSetupData.colorScheme = patchedColors;
        }
        
        [HarmonyPatch(typeof(MissionLevelRestartController), "RestartLevel")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void MissionLevelRestartControllerPatch(MissionLevelRestartController __instance)
        {
            if (!Config.Enabled)
            {
                return;
            }

            ColorScheme oldScheme = __instance._missionLevelSceneSetupData.gameplayCoreSceneSetupData.colorScheme;
            oldScheme._saberAColor = OriginalLeftColor;
            oldScheme._saberBColor = OriginalRightColor;
            __instance._missionLevelSceneSetupData.gameplayCoreSceneSetupData.SetField("colorScheme", PatchColors(oldScheme));
        }
        
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Finish")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void FinishPatch(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            ColorScheme oldScheme = __instance.colorScheme;
            oldScheme._saberAColor = OriginalLeftColor;
            oldScheme._saberBColor = OriginalRightColor;

            __instance.colorScheme = oldScheme;
        }
        
        [HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Finish")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void FinishMissionPatch(MissionLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled)
            {
                return;
            }

            ColorScheme oldScheme = __instance.gameplayCoreSceneSetupData.colorScheme;
            oldScheme._saberAColor = OriginalLeftColor;
            oldScheme._saberBColor = OriginalRightColor;

            __instance.gameplayCoreSceneSetupData.SetField("colorScheme", oldScheme);
        }
    }
}