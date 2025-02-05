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

        private static ColorScheme _patchedScheme;
        
        private static ColorScheme PatchColors(ColorScheme scheme)
        {
            _patchedScheme = new ColorScheme(
                "NoteTweaksPatched",
                "NoteTweaksPatched",
                true,
                "NoteTweaksPatched",
                false,
                scheme._saberAColor,
                scheme._saberBColor,
                scheme._environmentColor0,
                scheme._environmentColor1,
                scheme._environmentColorW,
                scheme._supportsEnvironmentColorBoost,
                scheme._environmentColor0Boost,
                scheme._environmentColor1Boost,
                scheme._environmentColorWBoost,
                scheme._obstaclesColor);
            
            float leftScale = 1.0f + Config.ColorBoostLeft;
            float rightScale = 1.0f + Config.ColorBoostRight;
            
            Color leftColor = _patchedScheme._saberAColor;
            float leftBrightness = leftColor.Brightness();
            Color rightColor = _patchedScheme._saberBColor;
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
            
            _patchedScheme._saberAColor = leftColor * leftScale;
            _patchedScheme._saberAColor.a = 1f;
            _patchedScheme._saberBColor = rightColor * rightScale;
            _patchedScheme._saberBColor.a = 1f;

            return _patchedScheme;
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

            ColorScheme scheme = __instance.colorScheme;
            ColorScheme tempScheme = new ColorScheme(
                "NoteTweaksUnPatched",
                "NoteTweaksUnPatched",
                true,
                "NoteTweaksUnPatched",
                false,
                scheme._saberAColor,
                scheme._saberBColor,
                scheme._environmentColor0,
                scheme._environmentColor1,
                scheme._environmentColorW,
                scheme._supportsEnvironmentColorBoost,
                scheme._environmentColor0Boost,
                scheme._environmentColor1Boost,
                scheme._environmentColorWBoost,
                scheme._obstaclesColor);
            
            ColorScheme patchedColors = PatchColors(tempScheme);

            __instance.usingOverrideColorScheme = true;
            __instance.colorScheme = patchedColors;
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
            
            ColorScheme scheme = __instance._standardLevelSceneSetupData.colorScheme;
            ColorScheme tempScheme = new ColorScheme(
                "NoteTweaksUnPatched",
                "NoteTweaksUnPatched",
                true,
                "NoteTweaksUnPatched",
                false,
                scheme._saberAColor,
                scheme._saberBColor,
                scheme._environmentColor0,
                scheme._environmentColor1,
                scheme._environmentColorW,
                scheme._supportsEnvironmentColorBoost,
                scheme._environmentColor0Boost,
                scheme._environmentColor1Boost,
                scheme._environmentColorWBoost,
                scheme._obstaclesColor);
            
            ColorScheme patchedColors = PatchColors(tempScheme);

            __instance._standardLevelSceneSetupData.usingOverrideColorScheme = true;
            __instance._standardLevelSceneSetupData.colorScheme = patchedColors;
        }
    }
}