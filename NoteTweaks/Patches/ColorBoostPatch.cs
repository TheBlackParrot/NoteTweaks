using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NoteColorTweaks
    {
        internal static Color OriginalLeftColor;
        internal static Color OriginalRightColor;
        
        private static ColorScheme PatchColors(ColorScheme scheme)
        {
            float leftScale = 1.0f + Plugin.Config.ColorBoostLeft;
            float rightScale = 1.0f + Plugin.Config.ColorBoostRight;
            
            if (OriginalLeftColor != scheme._saberAColor && OriginalLeftColor != (scheme._saberAColor * leftScale))
            {
                OriginalLeftColor = scheme._saberAColor;
            }
            if (OriginalRightColor != scheme._saberBColor && OriginalRightColor != (scheme._saberBColor * rightScale))
            {
                OriginalRightColor = scheme._saberBColor;
            }
            
            scheme._saberAColor = OriginalLeftColor * leftScale;
            scheme._saberBColor = OriginalRightColor * rightScale;

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
            if (SceneManager.GetActiveScene().name != "GameCore" || !Plugin.Config.Enabled)
            {
                return true;
            }
            
            ColorSchemesSettings settings = __instance.playerData.colorSchemesSettings;
            ColorScheme selectedScheme = settings.GetSelectedColorScheme();
            float leftScale = 1.0f + Plugin.Config.ColorBoostLeft;
            float rightScale = 1.0f + Plugin.Config.ColorBoostRight;
            
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
            if (SceneManager.GetActiveScene().name != "GameCore" || !Plugin.Config.Enabled)
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
            
            if (!Plugin.Config.Enabled)
            {
                return;
            }

            __instance.colorScheme = PatchColors(__instance.colorScheme);
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
                if (!Plugin.Config.Enabled)
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
            if (!Plugin.Config.Enabled)
            {
                return;
            }
            
            ColorScheme oldScheme = __instance._standardLevelSceneSetupData.colorScheme;
            oldScheme._saberAColor = OriginalLeftColor;
            oldScheme._saberBColor = OriginalRightColor;
            __instance._standardLevelSceneSetupData.colorScheme = PatchColors(oldScheme);
        }
        
        [HarmonyPatch(typeof(MissionLevelRestartController), "RestartLevel")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void MissionLevelRestartControllerPatch(MissionLevelRestartController __instance)
        {
            if (!Plugin.Config.Enabled)
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
            if (!Plugin.Config.Enabled)
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
            if (!Plugin.Config.Enabled)
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