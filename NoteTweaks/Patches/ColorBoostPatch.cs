using HarmonyLib;
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.LowerThanNormal)]
    internal class NoteColorTweaks
    {
        private static PluginConfig Config => PluginConfig.Instance;

        internal static ColorScheme PatchedScheme;
        
        private static ColorScheme PatchColors(ColorSchemeSO schemeObj)
        {
            PatchedScheme = new ColorScheme(schemeObj)
            {
                _colorSchemeId = "NoteTweaksPatched",
                _colorSchemeNameLocalizationKey = "NoteTweaksPatched",
                _useNonLocalizedName = true,
                _nonLocalizedName = "NoteTweaksPatched",
                _isEditable = false
            };
            
            float leftScale = 1.0f + Config.ColorBoostLeft;
            float rightScale = 1.0f + Config.ColorBoostRight;
            
            Color leftColor = PatchedScheme._saberAColor;
            float leftBrightness = leftColor.Brightness();
            Color rightColor = PatchedScheme._saberBColor;
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
            
            PatchedScheme._saberAColor = leftColor * leftScale;
            PatchedScheme._saberAColor.a = 1f;
            PatchedScheme._saberBColor = rightColor * rightScale;
            PatchedScheme._saberBColor.a = 1f;

            return PatchedScheme;
        }
        
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitColorInfo")]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void InitColorInfoPatch(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance.colorScheme;

            ColorScheme patchedColors = PatchColors(schemeObj);

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
        }
        
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "InitColorInfo")]
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void InitColorInfoPatchMultiplayer(MultiplayerLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance.colorScheme;

            ColorScheme patchedColors = PatchColors(schemeObj);

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
        }

        [HarmonyPatch(typeof(StandardLevelRestartController), "RestartLevel")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.LowerThanNormal)]
        // ReSharper disable once InconsistentNaming
        private static void StandardLevelRestartControllerPatch(StandardLevelRestartController __instance)
        {
            if (!Config.Enabled|| NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance._standardLevelSceneSetupData.colorScheme;
            ColorScheme patchedColors = PatchColors(schemeObj);

            __instance._standardLevelSceneSetupData.usingOverrideColorScheme = true;
            __instance._standardLevelSceneSetupData.colorScheme = patchedColors;
        }
    }
}