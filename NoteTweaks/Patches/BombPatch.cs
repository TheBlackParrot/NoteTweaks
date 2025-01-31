﻿using HarmonyLib;
using NoteTweaks.Managers;
using UnityEngine;
#pragma warning disable CS0612

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class BombPatch
    {
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");
        
        [HarmonyPatch(typeof(BombNoteController), "Init")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyAfter("aeroluna.Chroma")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        internal static void BombNoteControllerInitPatch(BombNoteController __instance)
        {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }

            Transform bombRoot = __instance.transform;
            
            if (Outlines.InvertedBombMesh == null)
            {
                if (bombRoot.GetChild(0).TryGetComponent(out MeshFilter bombMeshFilter))
                {
                    Outlines.UpdateDefaultBombMesh(bombMeshFilter.sharedMesh);
                }
            }
            
            if (Plugin.Config.EnableNoteOutlines)
            {
                Outlines.AddOutlineObject(bombRoot, Outlines.InvertedBombMesh);
                Transform noteOutline = bombRoot.FindChildRecursively("NoteOutline");
                    
                noteOutline.gameObject.SetActive(Plugin.Config.EnableNoteOutlines);
                noteOutline.localScale = (Vector3.one * (Plugin.Config.NoteOutlineScale / 100f)) + Vector3.one;
                    
                if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
                {
                    controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, Plugin.Config.BombOutlineColor);
                    controller.ApplyChanges();
                }
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
        // ReSharper disable once InconsistentNaming
        internal static void BeatmapObjectsInstallerInitPatch(BombNoteController ____bombNotePrefab) {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            
            Color bombColor = Plugin.Config.EnableRainbowBombs ? RainbowGradient.Color : Plugin.Config.BombColor * (1.0f + Plugin.Config.BombColorBoost);

            if (____bombNotePrefab.transform.GetChild(0).TryGetComponent(out ConditionalMaterialSwitcher switcher))
            {
                switcher._material0.SetColor(Color0, bombColor);
                switcher._material1.SetColor(Color0, bombColor);
            }
        }
    }
}