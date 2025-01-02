using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;

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
        
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitColorInfo")]
        [HarmonyPostfix]
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
            static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(MissionLevelScenesTransitionSetupDataSO),
                m => m.Name == nameof(MissionLevelScenesTransitionSetupDataSO.Init) &&
                     m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));
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