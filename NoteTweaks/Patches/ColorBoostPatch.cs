using HarmonyLib;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NoteColorTweaks
    {
        private static Color _originalLeftColor;
        private static Color _originalRightColor;
        
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
            
            if (_originalLeftColor != oldScheme._saberAColor && _originalLeftColor != (oldScheme._saberAColor * leftScale))
            {
                _originalLeftColor = oldScheme._saberAColor;
            }
            if (_originalRightColor != oldScheme._saberBColor && _originalRightColor != (oldScheme._saberBColor * rightScale))
            {
                _originalRightColor = oldScheme._saberBColor;
            }
            
            oldScheme._saberAColor = _originalLeftColor * leftScale;
            oldScheme._saberBColor = _originalRightColor * rightScale;
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