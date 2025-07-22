using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Loader;
using IPA.Utilities.Async;
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
    [UsedImplicitly]
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

        internal const float ACC_DOT_SIZE_STEP = ScoreModel.kMaxDistanceForDistanceToCenterScore / ScoreModel.kMaxCenterDistanceCutScore;

        internal static bool AutoDisable;
        private static bool _fixDots = true;
        internal static bool UsesChroma;

#if V1_40_6
        private static bool MapHasRequirement(BeatmapKey beatmapKey, string requirement, bool alsoCheckSuggestions = false)
#else
        private static bool MapHasRequirement(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, string requirement, bool alsoCheckSuggestions = false)
#endif
        {
            bool hasRequirement = false;
            
#if V1_40_6
            SongData.DifficultyData diffData = GetCustomLevelSongDifficultyData(beatmapKey);
#else
            ExtraSongData.DifficultyData diffData = RetrieveDifficultyData(beatmapLevel, beatmapKey);
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

        [HarmonyPatch]
        internal class LevelScenesTransitionPatches
        {
            private static bool PrefixCall()
            {
                if (!Config.Enabled)
                {
                    return true;
                }

                Managers.Meshes.UpdateSphereMesh(Config.BombMeshSlices, Config.BombMeshStacks,
                    Config.BombMeshSmoothNormals, Config.BombMeshWorldNormals);

                UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
                {
                    await Materials.UpdateAll();
                    BombPatch.SetStaticBombColor();
                });

                return true;
            }

#if V1_40_6
            private static void PostfixCall(BeatmapKey beatmapKey, in GameplayModifiers gameplayModifiers)
#else
            private static void PostfixCall(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, in GameplayModifiers gameplayModifiers)
#endif
            {
                if (!Config.Enabled)
                {
                    UsesChroma = false;
                    return;
                }

#if V1_40_6
                AutoDisable =
                    (MapHasRequirement(beatmapKey, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(beatmapKey, "Vivify") &&
                     Config.DisableIfVivify);
                
                UsesChroma = PluginManager.GetPluginFromId("Chroma") != null &&
                             MapHasRequirement(beatmapKey, "Chroma", true);

                _fixDots = true;
                if (MapHasRequirement(beatmapKey, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
#else
                AutoDisable =
                    (MapHasRequirement(beatmapLevel, beatmapKey, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(beatmapLevel, beatmapKey, "Vivify") &&
                     Config.DisableIfVivify);

                UsesChroma = PluginManager.GetPluginFromId("Chroma") != null &&
                             MapHasRequirement(beatmapLevel, beatmapKey, "Chroma", true);

                _fixDots = true;
                if (MapHasRequirement(beatmapLevel, beatmapKey, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
#endif

                _gameplayModifiers = gameplayModifiers;
                _fixDots = (_fixDots && !gameplayModifiers.ghostNotes);

                Plugin.ClampSettings();
            }
            
            [HarmonyPatch]
            internal class StandardLevelScenesTransitionSetupDataPatch
            {
                // thanks BeatLeader
                [UsedImplicitly]
                private static MethodInfo TargetMethod() => AccessTools.FirstMethod(
                    typeof(StandardLevelScenesTransitionSetupDataSO),
                    m => m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init) &&
                         m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));

                // ReSharper disable once InconsistentNaming
                internal static void Postfix(StandardLevelScenesTransitionSetupDataSO __instance, in GameplayModifiers gameplayModifiers)
                {
#if V1_40_6
                    PostfixCall(__instance.beatmapKey, gameplayModifiers);
#else
                    PostfixCall(__instance.beatmapLevel, __instance.beatmapKey, gameplayModifiers);
#endif
                }

                // ReSharper disable once InconsistentNaming
                internal static bool Prefix()
                {
                    return PrefixCall();
                }
            }

            [HarmonyPatch]
            internal class MultiplayerLevelScenesTransitionSetupDataPatch
            {
                [UsedImplicitly]
                [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
                // ReSharper disable once InconsistentNaming
                internal static void Postfix(MultiplayerLevelScenesTransitionSetupDataSO __instance, in GameplayModifiers gameplayModifiers)
                {
#if V1_40_6
                    PostfixCall(__instance.beatmapKey, gameplayModifiers);
#else
                    PostfixCall(__instance.beatmapLevel, __instance.beatmapKey, gameplayModifiers);
#endif
                }

                [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
                // ReSharper disable once InconsistentNaming
                internal static bool Prefix()
                {
                    return PrefixCall();
                }
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
                
                if (chainRoot.TryGetComponent(out MeshFilter meshFilter))
                {
                    Managers.Meshes.UpdateDefaultChainLinkMesh(meshFilter.sharedMesh);
                    meshFilter.sharedMesh = Managers.Meshes.CurrentChainLinkMesh;
                }
                
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

                if (Config.EnableNoteOutlines && !_gameplayModifiers.ghostNotes)
                {
                    Outlines.AddOutlineObject(chainRoot, Outlines.InvertedChainMesh, false);
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
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : Materials.SaneAlphaValue));
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
                bool isDot = __instance.noteData.cutDirection == NoteCutDirection.Any;

                if (noteRoot.TryGetComponent(out MeshFilter meshFilter))
                {
                    if (!isChainHead)
                    {
                        Managers.Meshes.UpdateDefaultNoteMesh(meshFilter.sharedMesh);
                        meshFilter.sharedMesh = isDot ? Managers.Meshes.CurrentDotNoteMesh : Managers.Meshes.CurrentNoteMesh;
                    }
                    else
                    {
                        Managers.Meshes.UpdateDefaultChainHeadMesh(meshFilter.sharedMesh);
                        meshFilter.sharedMesh = Managers.Meshes.CurrentChainHeadMesh;
                    }
                }

                if (Config.EnableAccDot && !isChainHead && !(_gameplayModifiers.ghostNotes || _gameplayModifiers.disappearingArrows))
                {
                    AccDotObject.transform.localScale = Vector3.one * (ACC_DOT_SIZE_STEP * (Mathf.Abs(Config.AccDotSize - 15) + 1));
                    
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
                    if (isDot)
                    {
                        if (Outlines.InvertedDotNoteMesh == null)
                        {
                            if (noteRoot.TryGetComponent(out MeshFilter cubeMeshFilter))
                            {
                                Outlines.UpdateDefaultDotNoteMesh(cubeMeshFilter.sharedMesh);
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
                }
                
                if (Config.EnableNoteOutlines && !_gameplayModifiers.ghostNotes)
                {
                    Outlines.AddOutlineObject(noteRoot, isChainHead ? Outlines.InvertedChainHeadMesh : (isDot ? Outlines.InvertedDotNoteMesh : Outlines.InvertedNoteMesh), !isChainHead);
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
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(applyBloom ? Config.OutlineBloomAmount : Materials.SaneAlphaValue));
                        controller.ApplyChanges();
                    }
                }
                
                Transform dotRoot = noteRoot.Find("NoteCircleGlow");
                bool applyBloomToFace = Config.AddBloomForFaceSymbols && Materials.MainEffectContainer.value;
                
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
            }
        }

        private static NoteCutDirection _lastDirection;
        
        [HarmonyPatch]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal class HandleCutDataPersistencePatch
        {
            private static void SetLastDirection(ref NoteController noteController)
            {
                _lastDirection = noteController.noteData.cutDirection;
            }
            
            // this fixes debris meshes in BeatLeader replays specifically...
            // (this is never called in normal gameplay)
            [HarmonyPatch(typeof(ScoreController), "HandleNoteWasCut")]
            [HarmonyPrefix]
            internal static void SetLastDirectionScoreController(NoteController noteController)
            {
                SetLastDirection(ref noteController);
            }
            
            // ...and this fixes it in normal gameplay
            // (this is never called in BeatLeader replays)
            [HarmonyPatch(typeof(NoteController), "SendNoteWasCutEvent")]
            [HarmonyPrefix]
            internal static void SetLastDirectionNoteController(NoteController __instance)
            {
                SetLastDirection(ref __instance);
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
                if (__instance.transform.GetChild(0).TryGetComponent(out MeshFilter debrisMeshFilter))
                {
                    bool isChainHead = __instance.gameObject.name.Contains("CubeNoteHalfDebris");
                    bool isChainLink = __instance.gameObject.name.Contains("CubeNoteSliceDebris");
                    bool isDotNote = _lastDirection == NoteCutDirection.Any;
                    
                    debrisMeshFilter.sharedMesh = isChainHead ? Managers.Meshes.CurrentChainHeadMesh :
                        isChainLink ? Managers.Meshes.CurrentChainLinkMesh :
                        isDotNote ? Managers.Meshes.CurrentDotNoteMesh : Managers.Meshes.CurrentNoteMesh;
                }
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
                if (!Config.Enabled || AutoDisable)
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
                    meshRenderer.gameObject.SetActive(Config.EnableArrows);
                    if (!Config.EnableArrows)
                    {
                        continue;
                    }
                    
                    Transform arrowTransform = meshRenderer.gameObject.transform;
                    
                    Vector3 scale = new Vector3(Config.ArrowScale.x, Config.ArrowScale.y, 1.0f);
                    Vector3 position = new Vector3(Config.ArrowPosition.x, InitialPosition.y + Config.ArrowPosition.y, InitialPosition.z);
                    
                    scale.Scale(Config.NoteScale);
                    position.Scale(Config.NoteScale);
                    
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

                    if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && !_gameplayModifiers.ghostNotes)
                    {
                        // worried coloring symbols might be seen as advantageous in GN (ghostNotes check)
                        
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
                        glowPosition.Scale(Config.NoteScale);
                        
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

                    dotPosition.Scale(Config.NoteScale);
                    glowPosition.Scale(Config.NoteScale);
                    dotScale.Scale(Config.NoteScale);
                    glowScale.Scale(Config.NoteScale);
                    
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
                            
                            if (!_gameplayModifiers.ghostNotes)
                            {
                                // worried coloring symbols might be seen as advantageous in GN
                                
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

                bool ghostNotesWorkaround = true;
                if (_gameplayModifiers != null)
                {
                    ghostNotesWorkaround = !_gameplayModifiers.ghostNotes;
                }
                
                if (__instance.transform.name == "NoteCube" && Config.EnableNoteOutlines && ghostNotesWorkaround)
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
                if (instance == null)
                {
                    // ...uh
                    return true;
                }
                
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
                    accDotTransform.transform.localScale = Vector3.one * (ACC_DOT_SIZE_STEP * (Mathf.Abs(Config.AccDotSize - 15) + 1)) * ratio * cutoutAmount;
                    accDotClearTransform.transform.localScale = Vector3.one * (ACC_DOT_SIZE_STEP * (Mathf.Abs(Config.AccDotSize - 15) + 1)) * ratio * cutoutAmount;
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

            if (originalColor == (isLeft ? NoteColorTweaks.PatchedScheme._saberAColor : NoteColorTweaks.PatchedScheme._saberBColor))
            {
                // not chroma
                return true;
            }
            
            float brightness = originalColor.Brightness();
            
            float maxBrightness = isLeft ? Config.LeftMaxBrightness : Config.RightMaxBrightness;
            float minBrightness = isLeft ? Config.LeftMinBrightness : Config.RightMinBrightness;
            
            if (brightness > maxBrightness)
            {
                originalColor = originalColor.LerpRGBUnclamped(Color.black, Mathf.InverseLerp(brightness, 0.0f, maxBrightness));
            }
            else if (brightness < minBrightness)
            {
                originalColor = originalColor.LerpRGBUnclamped(Color.white, Mathf.InverseLerp(brightness, 1.0f, minBrightness));
            }
            float colorScale = 1.0f + (isLeft ? Config.ColorBoostLeft : Config.ColorBoostRight);
            originalColor *= colorScale;
            
            Transform noteRoot = colorNoteVisuals.transform.GetChild(0);
            
            if(noteRoot.TryGetComponent(out MaterialPropertyBlockController noteMaterialPropertyBlockController))
            {
                noteMaterialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, originalColor);
                noteMaterialPropertyBlockController.ApplyChanges();
            }
            
            List<string> glowObjs = new List<string> { "NoteArrowGlow", "AddedNoteCircleGlow" };
            glowObjs.Do(objName =>
            {
                Transform glowTransform = noteRoot.Find(objName);
                if (glowTransform != null)
                {
                    if (glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        Color glowColor = originalColor;
                            
                        if (isLeft ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                        {
                            float colorScalar = glowColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                glowColor /= colorScalar;
                            }
                        }
                        
                        Color oldGlowColor = materialPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        Color fixedColor = glowColor.ColorWithAlpha(oldGlowColor.a);
                            
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
                            float colorScalar = faceColor.maxColorComponent;
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

            if (Config.EnableAccDot)
            {
                Transform originalAccDotObject = noteRoot.Find("AccDotObject");

                if (originalAccDotObject.TryGetComponent(out MeshRenderer originalAccDotMeshRenderer))
                {
                    Color accDotColor = originalColor;

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
                                isLeft ? Config.LeftAccDotColorNoteSkew : Config.RightAccDotColorNoteSkew)
                            .ColorWithAlpha(0f);
                }
            }

            if (Config.EnableNoteOutlines)
            {
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
            }

            return true; 
        }
    }
}