using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Loader;
#if !V1_29_1
using IPA.Utilities.Async;
#endif
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using NoteTweaks.Utils;
using SiraUtil.Affinity;
using SongCore.Data;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using static SongCore.Collections;
#pragma warning disable CS0612

namespace NoteTweaks.Patches
{
    internal class NotePhysicalTweaks
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        private static GameplayModifiers _gameplayModifiers;
        
        private static Mesh _dotGlowMesh;
        
        private static GameObject CreateAccDotObject()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.layer = LayerMask.NameToLayer("Note");
            
            if (obj.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterial = Materials.AccDotMaterial;
            }
            if (obj.TryGetComponent(out SphereCollider sphereCollider))
            {
                Object.DestroyImmediate(sphereCollider);
            }
            
            obj.SetActive(false);
            Plugin.Log.Info("setup acc dot object");

            return obj;
        }

        private static GameObject _accDotObject;
        internal static GameObject AccDotObject
        {
            get
            {
                if (_accDotObject == null)
                {
                    _accDotObject = CreateAccDotObject();
                }
                return _accDotObject;
            }
        }

        internal const float AccDotSizeStep = ScoreModel.kMaxDistanceForDistanceToCenterScore / ScoreModel.kMaxCenterDistanceCutScore;

        internal static bool AutoDisable;
        private static bool _fixDots = true;
        internal static bool UsesChroma;

#if LATEST
        private static bool MapHasRequirement(BeatmapKey beatmapKey, string requirement, bool alsoCheckSuggestions = false)
#elif !PRE_V1_37_1
        private static bool MapHasRequirement(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, string requirement, bool alsoCheckSuggestions = false)
#else
        private static bool MapHasRequirement(IDifficultyBeatmap beatmapLevel, string requirement, bool alsoCheckSuggestions = false)
#endif
        {
            bool hasRequirement = false;
            
#if LATEST
            SongData.DifficultyData diffData = GetCustomLevelSongDifficultyData(beatmapKey);
#elif !PRE_V1_37_1
            ExtraSongData.DifficultyData diffData = RetrieveDifficultyData(beatmapLevel, beatmapKey);
#else
            ExtraSongData.DifficultyData diffData = RetrieveDifficultyData(beatmapLevel);
#endif
            if (diffData != null)
            {
                hasRequirement = diffData.additionalDifficultyData._requirements.Any(x => x == requirement);
                if (!hasRequirement && alsoCheckSuggestions)
                {
                    hasRequirement = diffData.additionalDifficultyData._suggestions.Any(x => x == requirement);
                }
            }
            return hasRequirement;
        }

        // thanks BeatLeader
        [HarmonyPatch]
        internal class StandardLevelScenesTransitionSetupDataPatch
        {
            [UsedImplicitly]
            static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(StandardLevelScenesTransitionSetupDataSO),
                m => m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init) &&
                     m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));
            
            // ReSharper disable once InconsistentNaming
            internal static void Postfix(StandardLevelScenesTransitionSetupDataSO __instance, in GameplayModifiers gameplayModifiers)
            {
                if (!Config.Enabled)
                {
                    UsesChroma = false;
                    return;
                }

#if LATEST
                AutoDisable =
                    (MapHasRequirement(__instance.beatmapKey, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(__instance.beatmapKey, "Vivify") &&
                     Config.DisableIfVivify);
                
                UsesChroma = PluginManager.GetPluginFromId("Chroma") != null &&
                             MapHasRequirement(__instance.beatmapKey, "Chroma", true);

                _fixDots = true;
                if (MapHasRequirement(__instance.beatmapKey, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
#elif !PRE_V1_37_1
                AutoDisable =
                    (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Vivify") &&
                     Config.DisableIfVivify);
                
                UsesChroma = PluginManager.GetPluginFromId("Chroma") != null &&
                             MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Chroma", true);

                _fixDots = true;
                if (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
#else
                AutoDisable =
                    (MapHasRequirement(__instance.difficultyBeatmap, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(__instance.difficultyBeatmap, "Vivify") &&
                     Config.DisableIfVivify);
                
                UsesChroma = PluginManager.GetPluginFromId("Chroma") != null &&
                             MapHasRequirement(__instance.difficultyBeatmap, "Chroma", true);

                _fixDots = true;
                if (MapHasRequirement(__instance.difficultyBeatmap, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
#endif
                
                _gameplayModifiers = gameplayModifiers;
                Plugin.ClampSettings();
            }
            
#if PRE_V1_39_1
            [HarmonyPatch(typeof(EnvironmentSceneSetup), "InstallBindings")]
            internal class EnvironmentSceneSetupPatch
            {
                internal static void Postfix()
                {
                    if (!Config.Enabled)
                    {
                        return;
                    }
                
    #if V1_29_1
                    Materials.UpdateMainEffectContainerWorkaroundThing();
                
                    Managers.Textures.SetDefaultTextures();
                    Managers.Meshes.UpdateSphereMesh(Config.BombMeshSlices, Config.BombMeshStacks, Config.BombMeshSmoothNormals, Config.BombMeshWorldNormals);
                    GlowTextures.LoadTextures(); // for some reason this needs to be explicitly called here in 1.29. idk
                    Materials.UpdateAll();
                    BombPatch.SetStaticBombColor();
    #else
                    Managers.Textures.SetDefaultTextures();
                
                    Managers.Meshes.UpdateSphereMesh(Config.BombMeshSlices, Config.BombMeshStacks, Config.BombMeshSmoothNormals, Config.BombMeshWorldNormals);
                
                    UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
                    {
                        await Materials.UpdateAll();
                        BombPatch.SetStaticBombColor();
                    });
    #endif
                }
            }
#else
            // ReSharper disable once InconsistentNaming
            internal static bool Prefix(StandardLevelScenesTransitionSetupDataSO __instance)
            {
                if (!Config.Enabled)
                {
                    return true;
                }
                
                Managers.Meshes.UpdateSphereMesh(Config.BombMeshSlices, Config.BombMeshStacks, Config.BombMeshSmoothNormals, Config.BombMeshWorldNormals);
                
                UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
                {
                    await Materials.UpdateAll();
                    BombPatch.SetStaticBombColor();
                });
                
                return true;
            }
#endif
        }

        internal static bool IsAllowedToScaleNotes
        {
            get
            {
                if (_gameplayModifiers == null)
                {
                    return false;
                }

                return !(_gameplayModifiers.proMode || _gameplayModifiers.smallCubes || _gameplayModifiers.strictAngles);
            }
        }

        private static bool IsUsingHiddenTypeModifier
        {
            get
            {
                if (_gameplayModifiers == null)
                {
                    return false;
                }

                return _gameplayModifiers.disappearingArrows || _gameplayModifiers.ghostNotes;
            }
        }

        [HarmonyPatch(typeof(BurstSliderGameNoteController), "Init")]
        internal class BurstSliderPatch
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            internal static void Postfix(ref BurstSliderGameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return;
                }
                
                Transform chainRoot = __instance.transform.GetChild(0);
                
                if (chainRoot.TryGetComponent(out MeshRenderer cubeRenderer))
                {
                    cubeRenderer.sharedMaterial = Materials.NoteMaterial;
                }
                
                ColorType colorType = __instance._noteData.colorType;
                bool isLeft = colorType == ColorType.ColorA;
                
                Transform glowTransform = chainRoot.Find("AddedNoteCircleGlow");
                if (glowTransform != null)
                {
                    if (glowTransform.TryGetComponent(out MeshRenderer glowRenderer))
                    {
                        Enum.TryParse(isLeft ? Config.LeftGlowBlendOp : Config.RightGlowBlendOp, out BlendOp operation);
                        glowRenderer.material.SetInt(Materials.BlendOpID, (int)operation);
                    }
                    
                    if(glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && __instance.gameObject.TryGetComponent(out ColorNoteVisuals colorNoteVisuals))
                    {
                        Color glowColor = colorNoteVisuals._colorManager.ColorForType(colorType);
                            
                        if (isLeft ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                        {
                            float colorScalar = glowColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                glowColor /= colorScalar;
                            }
                        }
                        
                        Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceGlowColor : Config.RightFaceGlowColor, glowColor, isLeft ? Config.LeftFaceGlowColorNoteSkew : Config.RightFaceGlowColorNoteSkew);
                        c.a = isLeft ? Config.LeftGlowIntensity : Config.RightGlowIntensity;
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        materialPropertyBlockController.ApplyChanges();
                    }
                }
                
                if (Outlines.InvertedChainMesh == null)
                {
                    if (chainRoot.TryGetComponent(out MeshFilter chainMeshFilter))
                    {
                        Outlines.UpdateDefaultChainMesh(chainMeshFilter.sharedMesh);
                    }
                }

                if (Config.EnableNoteOutlines && !IsUsingHiddenTypeModifier)
                {
                    Outlines.AddOutlineObject(chainRoot, Outlines.InvertedChainMesh);
                    Transform noteOutline = chainRoot.Find("NoteOutline");
                    
                    noteOutline.gameObject.SetActive(Config.EnableNoteOutlines);
                    
                    Vector3 chainScale = (Vector3.one * (Config.NoteOutlineScale / 100f)) + Vector3.one;
                    chainScale.y = 1f + Config.NoteOutlineScale / 20f;
                    noteOutline.localScale = chainScale;

                    if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
                    {
                        Color noteColor = Config.BombColor;
                        if (cubeRenderer.TryGetComponent(out MaterialPropertyBlockController noteMaterialController))
                        {
                            noteColor = noteMaterialController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        }
                
                        float colorScalar = noteColor.maxColorComponent;

                        if (colorScalar != 0 && isLeft ? Config.NormalizeLeftOutlineColor : Config.NormalizeRightOutlineColor)
                        {
                            noteColor /= colorScalar;
                        }

                        Color outlineColor = Color.LerpUnclamped(isLeft ? Config.NoteOutlineLeftColor : Config.NoteOutlineRightColor, noteColor, isLeft ? Config.NoteOutlineLeftColorSkew : Config.NoteOutlineRightColorSkew);
                        
                        bool applyBloom = Config.AddBloomForOutlines && Materials.MainEffectContainer.value;
#if PRE_V1_39_1
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : 1f));
                        controller.materialPropertyBlock.SetFloat(Materials.FinalColorMul, isLeft ? Config.LeftOutlineFinalColorMultiplier : Config.RightOutlineFinalColorMultiplier);
#else
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : Materials.SaneAlphaValue));
#endif
                        controller.ApplyChanges();
                    }
                }
                
                // alpha's being weird with dots
                bool applyBloomToFace = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value;
                Transform dotRoot = chainRoot.Find("Circle");
                if (applyBloomToFace && dotRoot != null && _fixDots)
                {
                    if (dotRoot.gameObject.TryGetComponent(out MaterialPropertyBlockController dotController))
                    {
                        Color c = dotController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        c.a = Config.FaceSymbolBloomAmount;
                        dotController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        dotController.ApplyChanges();
                    }
                }

                if (!IsAllowedToScaleNotes)
                {
                    return;
                }
                
                Vector3 scale = Vectors.Max(Config.NoteScale * Config.LinkScale, 0.1f);
                Vector3 invertedScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);

                __instance.transform.localScale = scale;
                
                foreach (BoxCuttableBySaber saberBox in ____bigCuttableBySaberList)
                {
                    saberBox.transform.localScale = invertedScale;
                }
                
                foreach (BoxCuttableBySaber saberBox in ____smallCuttableBySaberList)
                {
                    saberBox.transform.localScale = invertedScale;
                }
            }
        }
        
        [HarmonyPatch(typeof(GameNoteController), "Init")]
        internal class NotePatch
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            internal static void Postfix(ref GameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return;
                }
                
                Transform noteRoot = __instance.transform.GetChild(0);
                
                __instance.gameObject.TryGetComponent(out ColorNoteVisuals colorNoteVisuals);
                ColorType colorType = __instance._noteData.colorType;
                bool isLeft = colorType == ColorType.ColorA;
                bool isChainHead = __instance.gameplayType == NoteData.GameplayType.BurstSliderHead;

                if (Config.EnableAccDot && !isChainHead && !IsUsingHiddenTypeModifier)
                {
                    AccDotObject.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Config.AccDotSize - 15) + 1));
                    
                    foreach (BoxCuttableBySaber saberBox in ____bigCuttableBySaberList)
                    {
                        Transform originalAccDot = saberBox.transform.parent.Find("AccDotObject");
                        if (!originalAccDot && saberBox.transform.parent.TryGetComponent(out MeshRenderer saberBoxMeshRenderer))
                        {
                            GameObject originalAccDotClearDepthObject = Object.Instantiate(AccDotObject, saberBox.transform.parent);
                            originalAccDotClearDepthObject.name = "AccDotObjectDepthClear";
                            if (originalAccDotClearDepthObject.TryGetComponent(out MeshRenderer originalAccDotClearDepthMeshRenderer))
                            {
                                originalAccDotClearDepthMeshRenderer.material = Materials.AccDotDepthMaterial;
                                originalAccDotClearDepthMeshRenderer.allowOcclusionWhenDynamic = false;
                                originalAccDotClearDepthMeshRenderer.renderingLayerMask = saberBoxMeshRenderer.renderingLayerMask;
                            }
                            originalAccDotClearDepthObject.SetActive(true);

                            GameObject originalAccDotObject = Object.Instantiate(AccDotObject, saberBox.transform.parent);
                            originalAccDotObject.name = "AccDotObject";
                            if (originalAccDotObject.TryGetComponent(out MeshRenderer originalAccDotMeshRenderer))
                            {
                                originalAccDotMeshRenderer.allowOcclusionWhenDynamic = false;
                                originalAccDotMeshRenderer.renderingLayerMask = saberBoxMeshRenderer.renderingLayerMask;
                                
                                originalAccDotMeshRenderer.sharedMaterial = Materials.AccDotMaterial;
                                
                                Color accDotColor = colorNoteVisuals._colorManager.ColorForType(colorType);
                                                
                                if (isLeft ? Config.NormalizeLeftAccDotColor : Config.NormalizeRightAccDotColor)
                                {
                                    float colorScalar = accDotColor.maxColorComponent;
                                    if (colorScalar != 0)
                                    {
                                        accDotColor /= colorScalar;
                                    }
                                }
                                                    
                                originalAccDotMeshRenderer.material.color =
                                    Color.LerpUnclamped(isLeft ? Config.LeftAccDotColor : Config.RightAccDotColor,
                                        accDotColor,
                                        isLeft ? Config.LeftAccDotColorNoteSkew : Config.RightAccDotColorNoteSkew).ColorWithAlpha(0f);
                            }
                            originalAccDotObject.SetActive(true);
                            
                            if (saberBox.TryGetComponent(out NoteBigCuttableColliderSize colliderSize))
                            {
                                float ratio = colliderSize._defaultColliderSize.x / colliderSize._defaultColliderSize.y;
                                originalAccDotClearDepthObject.transform.localScale *= ratio;
                                originalAccDotObject.transform.localScale *= ratio;
                            }
                        }
                    }
                }
                
                if (noteRoot.TryGetComponent(out MeshRenderer cubeRenderer))
                {
                    cubeRenderer.sharedMaterial = Materials.NoteMaterial;
                }
                
                List<string> objs = new List<string> { "NoteArrowGlow", "AddedNoteCircleGlow" };

                // ok buddy, ok pal
                objs.Do(objName =>
                {
                    Transform glowTransform = noteRoot.Find(objName);
                    if (glowTransform != null)
                    {
                        if (glowTransform.TryGetComponent(out MeshRenderer glowRenderer))
                        {
                            Enum.TryParse(isLeft ? Config.LeftGlowBlendOp : Config.RightGlowBlendOp, out BlendOp operation);
                            glowRenderer.material.SetInt(Materials.BlendOpID, (int)operation);
                        }
                        
                        if(glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && colorNoteVisuals != null)
                        {
                            Color glowColor = colorNoteVisuals._colorManager.ColorForType(colorType);
                            
                            if (isLeft ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                            {
                                float colorScalar = glowColor.maxColorComponent;
                                if (colorScalar != 0)
                                {
                                    glowColor /= colorScalar;
                                }
                            }
                        
                            Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceGlowColor : Config.RightFaceGlowColor, glowColor, isLeft ? Config.LeftFaceGlowColorNoteSkew : Config.RightFaceGlowColorNoteSkew);
                            c.a = isLeft ? Config.LeftGlowIntensity : Config.RightGlowIntensity;
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                            materialPropertyBlockController.ApplyChanges();
                        }
                    } 
                });

                if (isChainHead)
                {
                    if (Outlines.InvertedChainHeadMesh == null)
                    {
                        if (noteRoot.TryGetComponent(out MeshFilter cubeMeshFilter))
                        {
                            Outlines.UpdateDefaultChainHeadMesh(cubeMeshFilter.sharedMesh);
                        }
                    } 
                }
                else
                {
                    if (Outlines.InvertedNoteMesh == null)
                    {
                        if (noteRoot.TryGetComponent(out MeshFilter cubeMeshFilter))
                        {
                            Outlines.UpdateDefaultNoteMesh(cubeMeshFilter.sharedMesh);
                        }
                    }   
                }
                
                if (Config.EnableNoteOutlines && !IsUsingHiddenTypeModifier)
                {
                    Outlines.AddOutlineObject(noteRoot, isChainHead ? Outlines.InvertedChainHeadMesh : Outlines.InvertedNoteMesh);
                    Transform noteOutline = noteRoot.Find("NoteOutline");
                    
                    noteOutline.gameObject.SetActive(Config.EnableNoteOutlines);
                    
                    Vector3 noteScale = (Vector3.one * (Config.NoteOutlineScale / 100f)) + Vector3.one;
                    if (isChainHead)
                    {
                        noteScale.y += Config.NoteOutlineScale / 100f;
                        
                        Vector3 pos = Vector3.zero;
                        // it's weird i know
                        pos.y = (Config.NoteOutlineScale / 433f) * -1f;
                        noteOutline.localPosition = pos;
                    }
                    noteOutline.localScale = noteScale;

                    if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
                    {
                        Color noteColor = Config.BombColor;
                        if (cubeRenderer.TryGetComponent(out MaterialPropertyBlockController noteMaterialController))
                        {
                            noteColor = noteMaterialController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        }
                
                        float colorScalar = noteColor.maxColorComponent;

                        if (colorScalar != 0 && isLeft ? Config.NormalizeLeftOutlineColor : Config.NormalizeRightOutlineColor)
                        {
                            noteColor /= colorScalar;
                        }

                        Color outlineColor = Color.LerpUnclamped(isLeft ? Config.NoteOutlineLeftColor : Config.NoteOutlineRightColor, noteColor, isLeft ? Config.NoteOutlineLeftColorSkew : Config.NoteOutlineRightColorSkew);
                        
                        bool applyBloom = Config.AddBloomForOutlines && Materials.MainEffectContainer.value;
#if PRE_V1_39_1
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : 1f));
                        controller.materialPropertyBlock.SetFloat(Materials.FinalColorMul, isLeft ? Config.LeftOutlineFinalColorMultiplier : Config.RightOutlineFinalColorMultiplier);
#else
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : Materials.SaneAlphaValue));
#endif
                        controller.ApplyChanges();
                    }
                }
                
                Transform dotRoot = noteRoot.Find("NoteCircleGlow");
                bool applyBloomToFace = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value;
                
#if PRE_V1_39_1
                if (_fixDots && dotRoot != null)
                {
                    if (noteRoot.gameObject.TryGetComponent(out MaterialPropertyBlockController noteController) && dotRoot.gameObject.TryGetComponent(out MaterialPropertyBlockController dotController))
                    {
                        Color noteColor = noteController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        Color faceColor = noteColor;
                    
                        if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                        {
                            float colorScalar = noteColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                faceColor /= colorScalar;
                            }
                        }
                        
                        Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                        c.a = applyBloomToFace ? Config.FaceSymbolBloomAmount : Materials.SaneAlphaValue;
                        dotController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        dotController.ApplyChanges();
                    }   
                }
#else
                if (applyBloomToFace && dotRoot != null && _fixDots)
                {
                    if (dotRoot.gameObject.TryGetComponent(out MaterialPropertyBlockController dotController))
                    {
                        Color c = dotController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        c.a = Config.FaceSymbolBloomAmount;
                        dotController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        dotController.ApplyChanges();
                    }
                }
#endif

                if (!IsAllowedToScaleNotes)
                {
                    return;
                }
                
                Vector3 scale = Config.NoteScale;
                Vector3 invertedScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);

                __instance.transform.localScale = scale;
                
                foreach (BoxCuttableBySaber saberBox in ____bigCuttableBySaberList)
                {
                    saberBox.transform.localScale = invertedScale;
                }
                
                foreach (BoxCuttableBySaber saberBox in ____smallCuttableBySaberList)
                {
                    saberBox.transform.localScale = invertedScale;
                }
            }
        }

        [HarmonyPatch(typeof(NoteDebris), "Init")]
        internal class DebrisPatch
        {
            // ReSharper disable once InconsistentNaming
            internal static void Postfix(NoteDebris __instance)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return;
                }
                
                if (__instance.transform.GetChild(0).TryGetComponent(out MeshRenderer debrisRenderer))
                {
                    debrisRenderer.sharedMaterial = Materials.DebrisMaterial;
                }
            }
        }

        [HarmonyPatch(typeof(NoteDebrisSpawner), "SpawnDebris")]
        internal class DebrisSpawnerPatch
        {
            // ReSharper disable once InconsistentNaming
            internal static bool Prefix(ref Vector3 noteScale)
            {
                if (!Config.Enabled || AutoDisable || !IsAllowedToScaleNotes)
                {
                    return true;
                }

                noteScale = Config.NoteScale;
                
                return true;
            }
        }

        [HarmonyPatch(typeof(ColorNoteVisuals), "HandleNoteControllerDidInit")]
        [HarmonyAfter("aeroluna.Chroma")]
        [HarmonyPriority(int.MinValue)]
        internal class NoteArrowPatch
        {
            private static readonly Vector3 InitialPosition = new Vector3(0.0f, 0.11f, -0.25f);
            private static readonly Vector3 InitialDotPosition = new Vector3(0.0f, 0.0f, -0.25f);
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            internal static void Postfix(ColorNoteVisuals __instance, ref MeshRenderer[] ____arrowMeshRenderers, ref MeshRenderer[] ____circleMeshRenderers)
            {
                if (!Config.Enabled || AutoDisable || IsUsingHiddenTypeModifier)
                {
                    return;
                }
                
                if (__instance.gameObject.TryGetComponent(out MirroredGameNoteController _))
                {
                    // just don't even touch these for now, actually
                    return;
                }
                
                ColorType colorType = __instance._noteController.noteData.colorType;
                bool isLeft = colorType == ColorType.ColorA;
                
                foreach (MeshRenderer meshRenderer in ____arrowMeshRenderers)
                {
                    Transform arrowTransform = meshRenderer.gameObject.transform;
                    
                    Vector3 scale = new Vector3(Config.ArrowScale.x, Config.ArrowScale.y, 1.0f);
                    Vector3 position = new Vector3(Config.ArrowPosition.x, InitialPosition.y + Config.ArrowPosition.y, InitialPosition.z);
                    
                    arrowTransform.localScale = scale;
                    arrowTransform.localPosition = position;
                    
                    meshRenderer.sharedMaterial = Materials.ReplacementArrowMaterial;

                    if (meshRenderer.gameObject.name != "NoteArrowGlow")
                    {
                        if (meshRenderer.TryGetComponent(out MeshFilter arrowMeshFilter))
                        {
                            Managers.Meshes.UpdateDefaultArrowMesh(arrowMeshFilter.mesh);
                            arrowMeshFilter.mesh = Managers.Meshes.CurrentArrowMesh;
                        }
                    }

                    if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        Color faceColor = __instance._colorManager.ColorForType(colorType);
                            
                        if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                        {
                            float colorScalar = faceColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                faceColor /= colorScalar;
                            }
                        }
                        
                        bool applyBloom = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value;
                        Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c.ColorWithAlpha(applyBloom ? Config.FaceSymbolBloomAmount : Materials.SaneAlphaValue));
                        materialPropertyBlockController.ApplyChanges();
                        
                        meshRenderer.material.SetInt(Materials.SrcFactorID, Materials.SrcFactor);
                        meshRenderer.material.SetInt(Materials.DstFactorID, Materials.DstFactor);
                        meshRenderer.material.SetInt(Materials.SrcFactorAlphaID, applyBloom ? 1 : Materials.SrcFactorAlpha);
                        meshRenderer.material.SetInt(Materials.DstFactorAlphaID, Materials.DstFactorAlpha);
                    }

                    if (meshRenderer.gameObject.TryGetComponent(out ConditionalMaterialSwitcher switcher))
                    {
                        switcher._material0 = Materials.ReplacementArrowMaterial;
                        switcher._material1 = Materials.ReplacementArrowMaterial;
                    }

                    Transform arrowGlowObject = meshRenderer.transform.parent.Find("NoteArrowGlow");
                    if (arrowGlowObject)
                    {
                        arrowGlowObject.GetComponent<MeshRenderer>().sharedMaterial = Materials.ArrowGlowMaterial;
                        arrowGlowObject.gameObject.SetActive(Config.EnableFaceGlow);
                        
                        Transform arrowGlowTransform = arrowGlowObject.transform;
                        
                        Vector3 glowScale = new Vector3(scale.x * Config.ArrowGlowScale * 0.6f, scale.y * Config.ArrowGlowScale * 0.3f, 0.6f);
                        Vector3 glowPosition = new Vector3(InitialPosition.x + Config.ArrowPosition.x, InitialPosition.y + Config.ArrowPosition.y, InitialPosition.z);
                        glowPosition += (Vector3)(isLeft ? Config.LeftGlowOffset : Config.RightGlowOffset);
                        
                        arrowGlowTransform.localScale = glowScale;
                        arrowGlowTransform.localPosition = glowPosition;
                    }
                }

                bool isChainLink = __instance.GetComponent<BurstSliderGameNoteController>() != null;
                
                foreach (MeshRenderer meshRenderer in ____circleMeshRenderers)
                {
                    if (_dotGlowMesh == null)
                    {
                        _dotGlowMesh = meshRenderer.GetComponent<MeshFilter>().mesh;
                    }

                    Vector3 dotPosition;
                    Vector3 glowPosition;
                    Vector3 dotScale;
                    Vector3 glowScale;
                    if (isChainLink)
                    {
                        dotPosition = InitialDotPosition;
                        glowPosition = new Vector3(InitialDotPosition.x, InitialDotPosition.y, InitialDotPosition.z + 0.001f);
                        dotScale = new Vector3(Config.ChainDotScale.x / (_fixDots ? 18f : 10f), Config.ChainDotScale.y / (_fixDots ? 18f : 10f), 1.0f);
                        glowScale = new Vector3((Config.ChainDotScale.x / 5.4f) * Config.DotGlowScale, (Config.ChainDotScale.y / 5.4f) * Config.DotGlowScale, 1.0f);
                    }
                    else
                    {
                        dotPosition = new Vector3(InitialDotPosition.x + Config.DotPosition.x, InitialDotPosition.y + Config.DotPosition.y, InitialDotPosition.z);
                        glowPosition = new Vector3(InitialDotPosition.x + Config.DotPosition.x, InitialDotPosition.y + Config.DotPosition.y, InitialDotPosition.z + 0.001f);
                        dotScale = new Vector3(Config.DotScale.x / (_fixDots ? 5f : 2f), Config.DotScale.y / (_fixDots ? 5f : 2f), 1.0f);
                        glowScale = new Vector3((Config.DotScale.x / 1.5f) * Config.DotGlowScale, (Config.DotScale.y / 1.5f) * Config.DotGlowScale, 1.0f);
                    }
                    
                    glowPosition += (Vector3)(isLeft ? Config.LeftGlowOffset : Config.RightGlowOffset);
                    
                    Transform originalDot = isChainLink ? meshRenderer.transform.parent.Find("Circle") : meshRenderer.transform.parent.Find("NoteCircleGlow");
                    Transform addedDot = meshRenderer.transform.parent.Find("AddedNoteCircleGlow");
                    if (originalDot)
                    {
                        if (isChainLink)
                        {
                            originalDot.gameObject.SetActive(Config.EnableChainDots);

                            if (!Config.EnableChainDots)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            originalDot.gameObject.SetActive(Config.EnableDots);

                            if (!Config.EnableDots)
                            {
                                continue;
                            }   
                        }
                        
                        Transform originalDotTransform = originalDot.transform;
                        
                        originalDotTransform.localScale = dotScale;
                        originalDotTransform.localPosition = dotPosition;
                        if (!isChainLink)
                        {
                            originalDotTransform.localRotation = Quaternion.identity;
                            originalDotTransform.Rotate(0f, 0f, Config.RotateDot);
                        }

                        if (_fixDots)
                        {
                            meshRenderer.GetComponent<MeshFilter>().mesh = Managers.Meshes.DotMesh;
                            meshRenderer.sharedMaterial = Materials.ReplacementDotMaterial;
                        }
                        
                        if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                        {
                            if (_fixDots)
                            {
                                if (!originalDotTransform.TryGetComponent(out CutoutEffect _))
                                {
                                    if (meshRenderer.transform.parent.TryGetComponent(out CutoutEffect parentCutoutEffect))
                                    {
                                        CutoutEffect cutoutEffect = originalDotTransform.gameObject.AddComponent<CutoutEffect>();
                                        cutoutEffect._materialPropertyBlockController = materialPropertyBlockController;
                                        cutoutEffect._useRandomCutoutOffset = parentCutoutEffect._useRandomCutoutOffset;
                                    }
                                }
                            }
                            Color faceColor = __instance._colorManager.ColorForType(colorType);
                            
                            if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                            {
                                float colorScalar = faceColor.maxColorComponent;
                                if (colorScalar != 0)
                                {
                                    faceColor /= colorScalar;
                                }
                            }
                            
                            bool applyBloom = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value;
                            Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                            c.a = _fixDots ? (applyBloom ? Config.FaceSymbolBloomAmount : Materials.SaneAlphaValue) : materialPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId).a;
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                            materialPropertyBlockController.ApplyChanges();
                            
                            meshRenderer.material.SetInt(Materials.SrcFactorID, Materials.SrcFactor);
                            meshRenderer.material.SetInt(Materials.DstFactorID, Materials.DstFactor);
                            meshRenderer.material.SetInt(Materials.SrcFactorAlphaID, applyBloom ? 1 : Materials.SrcFactorAlpha);
                            meshRenderer.material.SetInt(Materials.DstFactorAlphaID, Materials.DstFactorAlpha);
                        }

                        if (isChainLink)
                        {
                            if (!Config.EnableChainDotGlow)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!Config.EnableFaceGlow)
                            {
                                continue;
                            }   
                        }

                        if (!_fixDots)
                        {
                            continue;
                        }

                        GameObject newGlowObject;
                        if (addedDot)
                        {
                            newGlowObject = addedDot.gameObject;
                        }
                        else
                        {
                            newGlowObject = Object.Instantiate(originalDot.gameObject, originalDot.parent);
                            newGlowObject.name = "AddedNoteCircleGlow";
                            
                            MaterialPropertyBlockController[] newMaterialPropertyBlockControllers = new MaterialPropertyBlockController[__instance._materialPropertyBlockControllers.Length + 1];
                            __instance._materialPropertyBlockControllers.CopyTo(newMaterialPropertyBlockControllers, 0);
                            newMaterialPropertyBlockControllers.SetValue(newGlowObject.GetComponent<MaterialPropertyBlockController>(), __instance._materialPropertyBlockControllers.Length);
                            __instance._materialPropertyBlockControllers = newMaterialPropertyBlockControllers;

                            MeshRenderer[] newRendererList = new MeshRenderer[2];
                            __instance._circleMeshRenderers.CopyTo(newRendererList, 0);
                            newRendererList.SetValue(newGlowObject.GetComponent<MeshRenderer>(), 1);
                            __instance._circleMeshRenderers = newRendererList;
                        }
                        
                        newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                        
                        newGlowObject.transform.localPosition = glowPosition;
                        newGlowObject.transform.localScale = glowScale;
                        if (!isChainLink)
                        {
                            newGlowObject.transform.localRotation = Quaternion.identity;
                            newGlowObject.transform.Rotate(0f, 0f, Config.RotateDot);
                        }

                        if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                        {
                            newGlowMeshRenderer.sharedMaterial = Materials.DotGlowMaterial;
                        }
                    }
                }
                
                if (Config.EnableAccDot)
                {
                    Transform accDotObject = __instance.transform.GetChild(0).Find("AccDotObject");
                    if (accDotObject != null)
                    {
                        Color accDotColor = __instance._colorManager.ColorForType(colorType);
                                                
                        if (isLeft ? Config.NormalizeLeftAccDotColor : Config.NormalizeRightAccDotColor)
                        {
                            float colorScalar = accDotColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                accDotColor /= colorScalar;
                            }
                        }
                                                    
                        accDotObject.gameObject.GetComponent<Renderer>().material.color =
                            Color.LerpUnclamped(isLeft ? Config.LeftAccDotColor : Config.RightAccDotColor,
                                accDotColor,
                                isLeft ? Config.LeftAccDotColorNoteSkew : Config.RightAccDotColorNoteSkew).ColorWithAlpha(0f);
                    }
                }
            }
        }

        [HarmonyPatch]
        public static class CutoutEffectForOutlinesPatch
        {
            [UsedImplicitly]
            static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(CutoutEffect),
                m => m.Name == nameof(CutoutEffect.SetCutout) &&
                     m.GetParameters().Any(p => p.Name == "cutoutOffset"));
            
            // ReSharper disable once InconsistentNaming
            internal static void Postfix(CutoutEffect __instance, in float cutout, in Vector3 cutoutOffset)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return;
                }
                
                if (__instance.transform.name == "NoteCube" && Config.EnableNoteOutlines && !IsUsingHiddenTypeModifier)
                {
                    Transform noteOutlineTransform = __instance.transform.Find("NoteOutline");
                    if (!noteOutlineTransform)
                    {
                        return;
                    }
                    
                    if (noteOutlineTransform.TryGetComponent(out CutoutEffect outlineCutoutEffect))
                    {
                        // i feel like this should fade in *slower* than normal note cutouts
                        outlineCutoutEffect.SetCutout(Mathf.Pow(cutout, 0.5f), cutoutOffset);
                    }

                    return;
                }

                if (__instance.transform.name == "BombNote(Clone)" && Config.EnableBombOutlines)
                {
                    Transform noteOutlineTransform = __instance.transform.Find("Mesh").Find("NoteOutline");
                    if (!noteOutlineTransform)
                    {
                        return;
                    }
                    
                    if (noteOutlineTransform.TryGetComponent(out CutoutEffect outlineCutoutEffect))
                    {
                        outlineCutoutEffect.SetCutout(Mathf.Pow(cutout, 0.5f), cutoutOffset);
                    }
                }
            }
        }
        
        [HarmonyPatch]
        public static class MaterialPropertyBlockControllerPatch
        {
            private static bool DoFacePatch(MaterialPropertyBlockController instance)
            {
                if (!instance.transform.parent.parent.TryGetComponent(out GameNoteController gameNoteController))
                {
                    return true;
                }

                if (!Materials.MainEffectContainer.value)
                {
                    // this completely breaks stuff if bloom is off. just turn bloom on, it's not 2018 anymore
                    return true;
                }

                Color originalColor = instance.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                float originalAlpha = originalColor.a;
                
                float alphaScale = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value ? Config.FaceSymbolBloomAmount : 1f;
                if (!Mathf.Approximately(originalAlpha, Config.FaceSymbolBloomAmount))
                {
                    instance.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, originalColor.ColorWithAlpha(originalAlpha * alphaScale));
                    instance.materialPropertyBlock.SetFloat(CutoutEffect._cutoutPropertyID, Mathf.Min(Mathf.Max(Mathf.Abs(originalAlpha - 1f), 0f), 1f));
                }
                
                Transform glowTransform = instance.transform.parent.Find("AddedNoteCircleGlow");
                if (glowTransform != null)
                {
                    if (glowTransform.TryGetComponent(out MaterialPropertyBlockController glowPropertyBlockController))
                    {
                        Color wantedGlowColor = glowPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        float fixedAlpha = Mathf.Approximately(originalAlpha, Config.FaceSymbolBloomAmount)
                            ? 1f
                            : originalAlpha;
                        wantedGlowColor.a = fixedAlpha * (gameNoteController._noteData.colorType == ColorType.ColorA ? Config.LeftGlowIntensity : Config.RightGlowIntensity);
                        glowPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, wantedGlowColor);
                        glowPropertyBlockController.ApplyChanges();
                    }
                }

                return true;
            }

            private static bool DoNotePatch(MaterialPropertyBlockController instance)
            {
                if (!instance.transform.TryGetComponent(out CutoutEffect cutoutEffect))
                {
                    return true;
                }
                
                Transform accDotTransform = instance.transform.Find("AccDotObject");
                Transform accDotClearTransform = instance.transform.Find("AccDotObjectDepthClear");
                if (accDotTransform == null || accDotClearTransform == null)
                {
                    return true;
                }
                
                Transform saberBox = instance.transform.Find("BigCuttable");
                
                if (saberBox.TryGetComponent(out NoteBigCuttableColliderSize colliderSize))
                {
                    float ratio = colliderSize._defaultColliderSize.x / colliderSize._defaultColliderSize.y;
                    float cutoutAmount = Mathf.Pow(Mathf.Abs(cutoutEffect._cutout - 1.0f), 1.5f);
                    accDotTransform.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Config.AccDotSize - 15) + 1)) * ratio * cutoutAmount;
                    accDotClearTransform.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Config.AccDotSize - 15) + 1)) * ratio * cutoutAmount;
                }

                return true;
            }

            [UsedImplicitly]
            [HarmonyPatch(typeof(MaterialPropertyBlockController), "ApplyChanges")]
            [HarmonyPrefix]
            // ReSharper disable once InconsistentNaming
            private static bool DoPatching(MaterialPropertyBlockController __instance)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return true;
                }
                
                if (__instance.gameObject.name == "NoteCircleGlow")
                {
                    return DoFacePatch(__instance);
                }

                if (__instance.gameObject.name == "NoteCube")
                {
                    return DoNotePatch(__instance);
                }

                return true;
            }
        }
    }
    
    internal class BeatEffectSpawnerPatch : IAffinity
    { 
        private static PluginConfig Config => PluginConfig.Instance;
        
        [AffinityPrefix]
        [AffinityAfter("aeroluna.Chroma")]
        [AffinityPatch(typeof(BeatEffectSpawner), nameof(BeatEffectSpawner.HandleNoteDidStartJump))]
        private bool DealWithChromaStuff(NoteController noteController)
        { 
            if (!Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return true;
            }
            if (!noteController.TryGetComponent(out ColorNoteVisuals colorNoteVisuals))
            {
                return true;
            }
                
            ColorType colorType = noteController._noteData.colorType;
            Color originalColor = colorNoteVisuals._colorManager.ColorForType(colorType);
            bool isLeft = colorType == ColorType.ColorA;
            
            Transform noteRoot = colorNoteVisuals.transform.GetChild(0);
            List<string> glowObjs = new List<string> { "NoteArrowGlow", "AddedNoteCircleGlow" };
            glowObjs.Do(objName =>
            {
                Transform glowTransform = noteRoot.Find(objName);
                if (glowTransform != null)
                {
                    if (glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        Color oldGlowColor = materialPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        Color fixedColor = originalColor.ColorWithAlpha(oldGlowColor.a);
                            
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, fixedColor);
                        materialPropertyBlockController.ApplyChanges();
                    }
                }
            });
                
            List<string> faceObjs = new List<string> { "NoteArrow", "NoteCircleGlow" };
            faceObjs.Do(objName =>
            {
                Transform faceTransform = noteRoot.Find(objName);
                if (faceTransform != null)
                {
                    if (faceTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        Color faceColor = originalColor;
                            
                        if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                        {
                            float colorScalar = colorNoteVisuals._noteColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                faceColor /= colorScalar;
                            }
                        }
                            
                        Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                            
                        Color oldFaceColor = materialPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        Color fixedColor = c.ColorWithAlpha(oldFaceColor.a);
                            
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, fixedColor);
                        materialPropertyBlockController.ApplyChanges();
                    }
                }
            });

            if (!Config.EnableNoteOutlines)
            {
                return true;
            }
                
            Transform noteOutline = noteRoot.Find("NoteOutline");
                
            if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
            {
                Color noteColor = originalColor;
                
                float colorScalar = noteColor.maxColorComponent;

                if (colorScalar != 0 && isLeft ? Config.NormalizeLeftOutlineColor : Config.NormalizeRightOutlineColor)
                {
                    noteColor /= colorScalar;
                }

                Color outlineColor = Color.LerpUnclamped(isLeft ? Config.NoteOutlineLeftColor : Config.NoteOutlineRightColor, noteColor, isLeft ? Config.NoteOutlineLeftColorSkew : Config.NoteOutlineRightColorSkew);
                        
                Color oldOutlineColor = controller.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                Color fixedColor = outlineColor.ColorWithAlpha(oldOutlineColor.a);
                    
                controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, fixedColor);
                controller.ApplyChanges();
            }

            return true; 
        }
    }
}