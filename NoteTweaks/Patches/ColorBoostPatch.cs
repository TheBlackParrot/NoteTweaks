using HarmonyLib;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NoteColorTweaks
    {
        internal static Color OriginalLeftColor;
        internal static Color OriginalRightColor;
        
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
            
            if (OriginalLeftColor != oldScheme._saberAColor && OriginalLeftColor != (oldScheme._saberAColor * leftScale))
            {
                OriginalLeftColor = oldScheme._saberAColor;
            }
            if (OriginalRightColor != oldScheme._saberBColor && OriginalRightColor != (oldScheme._saberBColor * rightScale))
            {
                OriginalRightColor = oldScheme._saberBColor;
            }
            
            oldScheme._saberAColor = OriginalLeftColor * leftScale;
            oldScheme._saberBColor = OriginalRightColor * rightScale;
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