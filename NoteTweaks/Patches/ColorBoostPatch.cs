using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NoteColorTweaks
    {
        private static Color originalLeftColor;
        private static Color originalRightColor;
        
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "InitColorInfo")]
        [HarmonyPostfix]
        private static void InitColorInfoPatch(StandardLevelScenesTransitionSetupDataSO __instance)
        {
            if (!Plugin.Config.Enabled)
            {
                return;
            }
            
            ColorScheme oldScheme = __instance.colorScheme;
            float leftScale = 1.0f + Plugin.Config.ColorBoostLeft;
            float rightScale = 1.0f + Plugin.Config.ColorBoostRight;
            
            if (originalLeftColor != oldScheme._saberAColor && originalLeftColor != (oldScheme._saberAColor * leftScale))
            {
                originalLeftColor = oldScheme._saberAColor;
            }
            if (originalRightColor != oldScheme._saberBColor && originalRightColor != (oldScheme._saberBColor * rightScale))
            {
                originalRightColor = oldScheme._saberBColor;
            }
            
            oldScheme._saberAColor = originalLeftColor * leftScale;
            oldScheme._saberBColor = originalRightColor * rightScale;
            __instance.colorScheme = oldScheme;
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
            oldScheme._saberAColor /= (1.0f + Plugin.Config.ColorBoostLeft);
            oldScheme._saberBColor /= (1.0f + Plugin.Config.ColorBoostRight);

            __instance.colorScheme = oldScheme;
        }
    }
}