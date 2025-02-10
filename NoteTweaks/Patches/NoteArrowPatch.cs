using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using IPA.Utilities.Async;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using NoteTweaks.Utils;
using SongCore.Data;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using static SongCore.Collections;
#pragma warning disable CS0612

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
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

        private static GameObject _accDotObject = CreateAccDotObject();
        private const float AccDotSizeStep = ScoreModel.kMaxDistanceForDistanceToCenterScore / ScoreModel.kMaxCenterDistanceCutScore;

        internal static bool AutoDisable;
        private static bool _fixDots = true;
        private static readonly int Color0 = Shader.PropertyToID("_Color");

        private static bool MapHasRequirement(BeatmapLevel beatmapLevel, BeatmapKey beatmapKey, string requirement)
        {
            bool hasRequirement = false;
            
            ExtraSongData.DifficultyData diffData = RetrieveDifficultyData(beatmapLevel, beatmapKey);
            if (diffData != null)
            {
                hasRequirement = diffData.additionalDifficultyData._requirements.Any(x => x == requirement);
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
                AutoDisable =
                    (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Noodle Extensions") &&
                     Config.DisableIfNoodle) ||
                    (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Vivify") &&
                     Config.DisableIfVivify);

                _fixDots = true;
                if (MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Noodle Extensions"))
                {
                    _fixDots = Config.FixDotsIfNoodle;
                }
                
                _gameplayModifiers = gameplayModifiers;
                Plugin.ClampSettings();
            }

            // ReSharper disable once InconsistentNaming
            internal static bool Prefix(StandardLevelScenesTransitionSetupDataSO __instance)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(async () => await Materials.UpdateAll());
                return true;
            }
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
                if (!Config.Enabled || !IsAllowedToScaleNotes || AutoDisable)
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
                        Color glowColor = colorNoteVisuals._noteColor;
                            
                        if (isLeft ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                        {
                            float colorScalar = colorNoteVisuals._noteColor.maxColorComponent;
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

                if (Config.EnableNoteOutlines)
                {
                    Outlines.AddOutlineObject(chainRoot, Outlines.InvertedChainMesh);
                    Transform noteOutline = chainRoot.Find("NoteOutline");
                    
                    noteOutline.gameObject.SetActive(Config.EnableNoteOutlines);
                    noteOutline.localScale = (Vector3.one * (Config.NoteOutlineScale / 100f)) + Vector3.one;

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
                        
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(Materials.SaneAlphaValue));
                        controller.ApplyChanges();
                    }
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

                if (Config.EnableAccDot && __instance.gameplayType != NoteData.GameplayType.BurstSliderHead && !IsUsingHiddenTypeModifier)
                {
                    if (!_accDotObject)
                    {
                        _accDotObject = CreateAccDotObject();
                    }
                    
                    _accDotObject.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Config.AccDotSize - 15) + 1));
                    
                    foreach (BoxCuttableBySaber saberBox in ____bigCuttableBySaberList)
                    {
                        Transform originalAccDot = saberBox.transform.parent.Find("AccDotObject");
                        if (!originalAccDot && saberBox.transform.parent.TryGetComponent(out MeshRenderer saberBoxMeshRenderer))
                        {
                            GameObject originalAccDotClearDepthObject = Object.Instantiate(_accDotObject, saberBox.transform.parent);
                            originalAccDotClearDepthObject.name = "AccDotObjectDepthClear";
                            if (originalAccDotClearDepthObject.TryGetComponent(out MeshRenderer originalAccDotClearDepthMeshRenderer))
                            {
                                originalAccDotClearDepthMeshRenderer.material = Materials.AccDotDepthMaterial;
                                originalAccDotClearDepthMeshRenderer.allowOcclusionWhenDynamic = false;
                                originalAccDotClearDepthMeshRenderer.renderingLayerMask = saberBoxMeshRenderer.renderingLayerMask;
                            }
                            originalAccDotClearDepthObject.SetActive(true);

                            GameObject originalAccDotObject = Object.Instantiate(_accDotObject, saberBox.transform.parent);
                            originalAccDotObject.name = "AccDotObject";
                            if (originalAccDotObject.TryGetComponent(out MeshRenderer originalAccDotMeshRenderer))
                            {
                                originalAccDotMeshRenderer.allowOcclusionWhenDynamic = false;
                                originalAccDotMeshRenderer.renderingLayerMask = saberBoxMeshRenderer.renderingLayerMask;
                                
                                originalAccDotMeshRenderer.sharedMaterial = Materials.AccDotMaterial;
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
                
                ColorType colorType = __instance._noteData.colorType;
                bool isLeft = colorType == ColorType.ColorA;

                // ok buddy, ok pal
                GameNoteController instance = __instance;
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
                        
                        if(glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && instance.gameObject.TryGetComponent(out ColorNoteVisuals colorNoteVisuals))
                        {
                            Color glowColor = colorNoteVisuals._noteColor;
                            
                            if (isLeft ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                            {
                                float colorScalar = colorNoteVisuals._noteColor.maxColorComponent;
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

                if (__instance.gameplayType == NoteData.GameplayType.BurstSliderHead)
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
                
                if (Config.EnableNoteOutlines)
                {
                    Outlines.AddOutlineObject(noteRoot, __instance.gameplayType == NoteData.GameplayType.BurstSliderHead ? Outlines.InvertedChainHeadMesh : Outlines.InvertedNoteMesh);
                    Transform noteOutline = noteRoot.Find("NoteOutline");
                    
                    noteOutline.gameObject.SetActive(Config.EnableNoteOutlines);
                    noteOutline.localScale = (Vector3.one * (Config.NoteOutlineScale / 100f)) + Vector3.one;

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
                        
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor.ColorWithAlpha(Materials.SaneAlphaValue));
                        controller.ApplyChanges();
                    }
                }

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
                
                foreach (MeshRenderer meshRenderer in ____arrowMeshRenderers)
                {
                    Transform arrowTransform = meshRenderer.gameObject.transform;
                    
                    Vector3 scale = new Vector3(Config.ArrowScale.x, Config.ArrowScale.y, 1.0f);
                    Vector3 position = new Vector3(Config.ArrowPosition.x, InitialPosition.y + Config.ArrowPosition.y, InitialPosition.z);
                    
                    arrowTransform.localScale = scale;
                    arrowTransform.localPosition = position;
                    
                    meshRenderer.sharedMaterial = Materials.ReplacementArrowMaterial;
                    
                    ColorType colorType = __instance._noteController.noteData.colorType;
                    bool isLeft = colorType == ColorType.ColorA;

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
                        Color faceColor = __instance._noteColor;
                            
                        if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                        {
                            float colorScalar = __instance._noteColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                faceColor /= colorScalar;
                            }
                        }
                        
                        Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c.ColorWithAlpha(Materials.SaneAlphaValue));
                        materialPropertyBlockController.ApplyChanges();   
                    }

                    if (meshRenderer.gameObject.TryGetComponent(out ConditionalMaterialSwitcher switcher))
                    {
                        switcher._material0 = Materials.ReplacementArrowMaterial;
                        switcher._material1 = Materials.ReplacementArrowMaterial;
                    }

                    Transform arrowGlowObject = meshRenderer.transform.parent.Find("NoteArrowGlow");
                    if (arrowGlowObject)
                    {
                        arrowGlowObject.gameObject.SetActive(Config.EnableFaceGlow);
                        
                        Transform arrowGlowTransform = arrowGlowObject.transform;
                        
                        Vector3 glowScale = new Vector3(scale.x * Config.ArrowGlowScale * 0.6f, scale.y * Config.ArrowGlowScale * 0.3f, 0.6f);
                        Vector3 glowPosition = new Vector3(InitialPosition.x + Config.ArrowPosition.x, InitialPosition.y + Config.ArrowPosition.y, InitialPosition.z);
                        glowPosition += (Vector3)(isLeft ? Config.LeftGlowOffset : Config.RightGlowOffset);
                        
                        arrowGlowTransform.localScale = glowScale;
                        arrowGlowTransform.localPosition = glowPosition;
                        
                        arrowGlowObject.GetComponent<MeshRenderer>().sharedMaterial = Materials.ArrowGlowMaterial;
                    }
                }

                bool isChainLink = __instance.GetComponent<BurstSliderGameNoteController>() != null;
                
                foreach (MeshRenderer meshRenderer in ____circleMeshRenderers)
                {
                    if (_dotGlowMesh == null)
                    {
                        _dotGlowMesh = meshRenderer.GetComponent<MeshFilter>().mesh;
                    }
                    
                    ColorType colorType = __instance._noteController.noteData.colorType;
                    bool isLeft = colorType == ColorType.ColorA;

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
                            Color faceColor = __instance._noteColor;
                            
                            if (isLeft ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                            {
                                float colorScalar = __instance._noteColor.maxColorComponent;
                                if (colorScalar != 0)
                                {
                                    faceColor /= colorScalar;
                                }
                            }
                        
                            Color c = Color.LerpUnclamped(isLeft ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, isLeft ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                            c.a = _fixDots ? 1f : materialPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId).a;
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                            materialPropertyBlockController.ApplyChanges();   
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
                
                if (__instance.transform.name == "NoteCube" && Config.EnableNoteOutlines)
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
                
                float originalAlpha = instance.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId).a;
                
                instance.materialPropertyBlock.SetFloat(CutoutEffect._cutoutPropertyID, Mathf.Min(Mathf.Max(Mathf.Abs(originalAlpha - 1.0f), 0f), 1f));
                
                Transform glowTransform = instance.transform.parent.Find("AddedNoteCircleGlow");
                if (glowTransform != null)
                {
                    if (glowTransform.TryGetComponent(out MaterialPropertyBlockController glowPropertyBlockController))
                    {
                        Color wantedGlowColor = glowPropertyBlockController.materialPropertyBlock.GetColor(ColorNoteVisuals._colorId);
                        wantedGlowColor.a = originalAlpha * (gameNoteController._noteData.colorType == ColorType.ColorA ? Config.LeftGlowIntensity : Config.RightGlowIntensity);
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
        
        /*[HarmonyPatch(typeof(SliderController), "Hide")]
        public static class SliderControllerPatch
        {
            // ReSharper disable once InconsistentNaming
            [UsedImplicitly]
            private static bool Prefix(SliderController __instance)
            {
                if (!Config.Enabled || AutoDisable)
                {
                    return true;
                }
                
                Color color = __instance._saber.saberType == SaberType.SaberA ? NoteColorTweaks.OriginalLeftColor : NoteColorTweaks.OriginalRightColor;

                __instance._initColor = color;
                __instance._materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, color);
                __instance._materialPropertyBlockController.ApplyChanges();

                return true;
            }
        }*/
    }
}