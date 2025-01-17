using HarmonyLib;
using IPA.Utilities;
using NoteTweaks.Managers;
using UnityEngine;
#pragma warning disable CS0612

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class BombPatch
    {
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");
        private static readonly Color DefaultColor = new Color(0.251f, 0.251f, 0.251f, 1.0f);
        
        [HarmonyPatch(typeof(BombNoteController), "Init")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        internal static void BombNoteControllerInitPatch(BombNoteController __instance)
        {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }

            Color bombColor = Plugin.Config.EnableRainbowBombs ? RainbowGradient.Color : Plugin.Config.BombColor * (1.0f + Plugin.Config.BombColorBoost);
            
            if (__instance.transform.GetChild(0).TryGetComponent(out Renderer bombRenderer))
            {
                bombRenderer.sharedMaterial = Materials.BombMaterial;
                bombRenderer.sharedMaterial.SetColor(Color0, bombColor);
            }

            if (__instance.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
            {
                materialPropertyBlockController.materialPropertyBlock.SetColor(Color0, bombColor);
                materialPropertyBlockController.ApplyChanges();   
            }

            if (!NotePhysicalTweaks.IsAllowedToScaleNotes)
            {
                return;
            }

            Vector3 scale = Vector3.one * Plugin.Config.BombScale;
            
            __instance.transform.localScale = scale;
            __instance._cuttableBySaber.GetComponent<SphereCollider>().radius = 0.18f * (1.0f / Plugin.Config.BombScale);
        }
        
        [HarmonyPatch(typeof(BeatmapObjectsInstaller), "InstallBindings")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPostfix]
        internal static void BeatmapObjectsInstallerInitPatch(BombNoteController ____bombNotePrefab) {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            
            Color bombColor = Plugin.Config.EnableRainbowBombs ? RainbowGradient.Color : Plugin.Config.BombColor * (1.0f + Plugin.Config.BombColorBoost);

            if (____bombNotePrefab.TryGetComponent(out ConditionalMaterialSwitcher switcher))
            {
                if (switcher._material0.GetColor(Color0) == DefaultColor)
                {
                    switcher._material0.SetColor(Color0, bombColor);
                }
                
                if (switcher._material1.GetColor(Color0) == DefaultColor)
                {
                    switcher._material1.SetColor(Color0, bombColor);
                }
            }
        }
    }
}