using HarmonyLib;
#if PRE_V1_37_1
using IPA.Utilities;
#endif
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
        
#if PRE_V1_37_1
        private static ColorScheme PatchColors(ColorScheme scheme)
        {
            PatchedScheme = new ColorScheme(
                "NoteTweaksPatched",
                "NoteTweaksPatched",
                true,
                "NoteTweaksPatched",
                false,
                scheme._saberAColor,
                scheme._saberBColor,
                scheme._environmentColor0,
                scheme._environmentColor1,
    #if !V1_29_1
                scheme._environmentColorW,
    #endif
                scheme._supportsEnvironmentColorBoost,
                scheme._environmentColor0Boost,
                scheme._environmentColor1Boost,
    #if !V1_29_1
                scheme._environmentColorWBoost,
    #endif
                scheme._obstaclesColor
            );
#else
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
#endif
            
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
        
#if PRE_V1_37_1
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
#else
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitColorInfo")]
#endif
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void InitColorInfoPatch(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
#if PRE_V1_37_1
            ColorScheme patchedColors = PatchColors(__instance.colorScheme);
#else
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance.colorScheme;

            ColorScheme patchedColors = PatchColors(schemeObj);
#endif

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
#if PRE_V1_37_1
            __instance.gameplayCoreSceneSetupData.SetField("colorScheme", patchedColors);
#endif
        }
        
#if PRE_V1_37_1
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
#else
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "InitColorInfo")]
#endif
        [HarmonyPriority(Priority.LowerThanNormal)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        private static void InitColorInfoPatchMultiplayer(MultiplayerLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
#if PRE_V1_37_1
            ColorScheme patchedColors = PatchColors(__instance.colorScheme);
#else
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance.colorScheme;

            ColorScheme patchedColors = PatchColors(schemeObj);
#endif

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
#if PRE_V1_37_1
            __instance.gameplayCoreSceneSetupData.SetField("colorScheme", patchedColors);
#endif
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
#if PRE_V1_37_1
            ColorScheme patchedColors = PatchColors(__instance._standardLevelSceneSetupData.colorScheme);
#else
            ColorSchemeSO schemeObj = ScriptableObject.CreateInstance<ColorSchemeSO>();
            schemeObj._colorScheme = __instance._standardLevelSceneSetupData.colorScheme;
            ColorScheme patchedColors = PatchColors(schemeObj);
#endif

            __instance._standardLevelSceneSetupData.usingOverrideColorScheme = true;
            __instance._standardLevelSceneSetupData.colorScheme = patchedColors;
        }
    }
}