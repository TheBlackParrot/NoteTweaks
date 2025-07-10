﻿using HarmonyLib;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using UnityEngine;
#pragma warning disable CS0612

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class BombPatch
    {
        private static PluginConfig Config => PluginConfig.Instance;
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");

        public static void SetStaticBombColor()
        {
            Color staticBombColor = Config.BombColor * (1.0f + Config.BombColorBoost);
            Materials.BombMaterial.SetColor(Color0, staticBombColor);
        }
        
        [HarmonyPatch(typeof(BombNoteController), "Init")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyAfter("aeroluna.Chroma")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        internal static void BombNoteControllerInitPatch(BombNoteController __instance)
        {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }

            Transform bombRoot = __instance.transform;
            
            if (!bombRoot.GetChild(0).TryGetComponent(out MeshFilter bombMeshFilter))
            {
                // erm
                return;
            }
            Meshes.UpdateDefaultBombMesh(bombMeshFilter.sharedMesh);
            
            bombMeshFilter.sharedMesh = Meshes.CurrentBombMesh;
            
            if (Outlines.InvertedBombMesh == null)
            {
                Outlines.UpdateDefaultBombMesh(bombMeshFilter.sharedMesh);
            }
            
            if (Config.EnableBombOutlines)
            {
                Outlines.AddOutlineObject(bombRoot, Outlines.InvertedBombMesh);
                Transform noteOutline = bombRoot.FindChildRecursively("NoteOutline");
                    
                noteOutline.gameObject.SetActive(true);
                noteOutline.localScale = (Vector3.one * (Config.BombOutlineScale / 100f)) + Vector3.one;
                    
                if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
                {
                    Color outlineColor =
                        Config.EnableRainbowBombs && (Config.RainbowBombMode == "Both" ||
                                                      Config.RainbowBombMode == "Only Outlines")
                            ? RainbowGradient.Color
                            : Config.BombOutlineColor;
                    
                    int applyBloom = Config.AddBloomForOutlines && Materials.MainEffectContainer.value ? 1 : 0;
                    controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom == 1 ? Config.OutlineBloomAmount : 1f));
                    controller.ApplyChanges();
                }
            }
            
            if (__instance.transform.GetChild(0).TryGetComponent(out Renderer bombRenderer))
            {
                bombRenderer.sharedMaterial = Materials.BombMaterial;
            }
            
            if (Config.EnableRainbowBombs && (Config.RainbowBombMode == "Both" ||
                                              Config.RainbowBombMode == "Only Bombs")
                                          && !NotePhysicalTweaks.UsesChroma)
            {
                if (__instance.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Color0, RainbowGradient.Color.ColorWithAlpha(1f));
                    materialPropertyBlockController.ApplyChanges();   
                }
            }
        }
        
        [HarmonyPatch(typeof(BeatmapObjectsInstaller), "InstallBindings")]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        internal static void BeatmapObjectsInstallerInitPatch(BombNoteController ____bombNotePrefab) {
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            
            Color bombColor =
                (Config.EnableRainbowBombs && (Config.RainbowBombMode == "Both" ||
                                              Config.RainbowBombMode == "Only Bombs")
                                          && !NotePhysicalTweaks.UsesChroma
                    ? RainbowGradient.Color
                    : Config.BombColor * (1.0f + Config.BombColorBoost)).ColorWithAlpha(1f);

            if (____bombNotePrefab.transform.GetChild(0).TryGetComponent(out ConditionalMaterialSwitcher switcher))
            {
                switcher._material0.SetColor(Color0, bombColor);
                switcher._material1.SetColor(Color0, bombColor);
            }
        }
    }
}