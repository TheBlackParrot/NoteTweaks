using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using NoteTweaks.Managers;
using NoteTweaks.Utils;
using SongCore.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using static SongCore.Collections;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NotePhysicalTweaks
    {
        private static GameplayModifiers _gameplayModifiers;

        private static Mesh _dotMesh;
        private static Mesh _dotGlowMesh;
        
        private static GameObject CreateAccDotObject()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "NoteHD").renderQueue = 1995;
            if (obj.TryGetComponent(out MeshRenderer meshRenderer))
            {
                Color c = Plugin.Config.AccDotColor;
                c.a = 0f;
                Materials.AccDotMaterial.color = c;
                
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
                     Plugin.Config.DisableIfNoodle) ||
                    MapHasRequirement(__instance.beatmapLevel, __instance.beatmapKey, "Vivify");
                
                _gameplayModifiers = gameplayModifiers;
                Plugin.ClampSettings();

                _dotMesh = Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);
            }

            // ReSharper disable once InconsistentNaming
            internal static bool Prefix(StandardLevelScenesTransitionSetupDataSO __instance)
            {
                #pragma warning disable CS4014
                Materials.UpdateAll();
                #pragma warning restore CS4014
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
                if (!Plugin.Config.Enabled || !IsAllowedToScaleNotes || AutoDisable)
                {
                    return;
                }
                
                if (__instance.transform.GetChild(0).TryGetComponent(out MeshRenderer cubeRenderer))
                {
                    cubeRenderer.sharedMaterial = Materials.NoteMaterial;
                }
                
                Transform glowTransform = __instance.transform.GetChild(0).Find("AddedNoteCircleGlow");
                if (glowTransform != null)
                {
                    if(glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && __instance.gameObject.TryGetComponent(out ColorNoteVisuals colorNoteVisuals))
                    {
                        ColorType colorType = __instance._noteData.colorType;
                        bool isLeft = colorType == ColorType.ColorA;
                        
                        Color glowColor = colorNoteVisuals._noteColor;
                            
                        if (isLeft ? Plugin.Config.NormalizeLeftFaceGlowColor : Plugin.Config.NormalizeRightFaceGlowColor)
                        {
                            float colorScalar = colorNoteVisuals._noteColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                glowColor /= colorScalar;
                            }
                        }
                        
                        Color c = Color.LerpUnclamped(isLeft ? Plugin.Config.LeftFaceGlowColor : Plugin.Config.RightFaceGlowColor, glowColor, isLeft ? Plugin.Config.LeftFaceGlowColorNoteSkew : Plugin.Config.RightFaceGlowColorNoteSkew);
                        c.a = isLeft ? Plugin.Config.LeftGlowIntensity : Plugin.Config.RightGlowIntensity;
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        materialPropertyBlockController.ApplyChanges();
                    }
                } 
                
                Vector3 scale = Vectors.Max(Plugin.Config.NoteScale * Plugin.Config.LinkScale, 0.1f);
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
                if (!Plugin.Config.Enabled || AutoDisable)
                {
                    return;
                }

                if (Plugin.Config.EnableAccDot && __instance.gameplayType != NoteData.GameplayType.BurstSliderHead && !IsUsingHiddenTypeModifier)
                {
                    if (!_accDotObject)
                    {
                        _accDotObject = CreateAccDotObject();
                    }
                    
                    _accDotObject.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Plugin.Config.AccDotSize - 15) + 1));
                    
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
                
                if (__instance.transform.GetChild(0).TryGetComponent(out MeshRenderer cubeRenderer))
                {
                    cubeRenderer.sharedMaterial = Materials.NoteMaterial;
                }
                
                List<string> objs = new List<string> { "NoteArrowGlow", "AddedNoteCircleGlow" };

                // ok buddy, ok pal
                GameNoteController instance = __instance;
                objs.Do(objName =>
                {
                    Transform glowTransform = instance.transform.GetChild(0).Find(objName);
                    if (glowTransform != null)
                    {
                        if(glowTransform.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController) && instance.gameObject.TryGetComponent(out ColorNoteVisuals colorNoteVisuals))
                        {
                            ColorType colorType = instance._noteData.colorType;
                            bool isLeft = colorType == ColorType.ColorA;
                        
                            Color glowColor = colorNoteVisuals._noteColor;
                            
                            if (isLeft ? Plugin.Config.NormalizeLeftFaceGlowColor : Plugin.Config.NormalizeRightFaceGlowColor)
                            {
                                float colorScalar = colorNoteVisuals._noteColor.maxColorComponent;
                                if (colorScalar != 0)
                                {
                                    glowColor /= colorScalar;
                                }
                            }
                        
                            Color c = Color.LerpUnclamped(isLeft ? Plugin.Config.LeftFaceGlowColor : Plugin.Config.RightFaceGlowColor, glowColor, isLeft ? Plugin.Config.LeftFaceGlowColorNoteSkew : Plugin.Config.RightFaceGlowColorNoteSkew);
                            c.a = isLeft ? Plugin.Config.LeftGlowIntensity : Plugin.Config.RightGlowIntensity;
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                            materialPropertyBlockController.ApplyChanges();
                        }
                    } 
                });

                if (!IsAllowedToScaleNotes)
                {
                    return;
                }
                
                Vector3 scale = Plugin.Config.NoteScale;
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
                if (!Plugin.Config.Enabled || AutoDisable)
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
            private static bool _initialPositionDidSet;
            private static Vector3 _initialPosition = Vector3.zero;
            private static bool _initialDotPositionDidSet;
            private static Vector3 _initialDotPosition = Vector3.zero;
            private static bool _initialChainDotPositionDidSet;
            private static Vector3 _initialChainDotPosition = Vector3.zero;
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            internal static void Postfix(ColorNoteVisuals __instance, ref MeshRenderer[] ____arrowMeshRenderers, ref MeshRenderer[] ____circleMeshRenderers)
            {
                if (!Plugin.Config.Enabled || AutoDisable || IsUsingHiddenTypeModifier)
                {
                    return;
                }
                
                foreach (MeshRenderer meshRenderer in ____arrowMeshRenderers)
                {
                    Transform arrowTransform = meshRenderer.gameObject.transform;

                    if (!_initialPositionDidSet)
                    {
                        _initialPositionDidSet = true;
                        _initialPosition = arrowTransform.localPosition;
                    }
                    
                    Vector3 scale = new Vector3(Plugin.Config.ArrowScale.x, Plugin.Config.ArrowScale.y, 1.0f);
                    Vector3 position = new Vector3(Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
                    
                    arrowTransform.localScale = scale;
                    arrowTransform.localPosition = position;
                    
                    meshRenderer.sharedMaterial = Materials.ReplacementArrowMaterial;
                    
                    if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        ColorType colorType = __instance._noteController.noteData.colorType;
                        bool isLeft = colorType == ColorType.ColorA;
                        
                        Color faceColor = __instance._noteColor;
                            
                        if (isLeft ? Plugin.Config.NormalizeLeftFaceColor : Plugin.Config.NormalizeRightFaceColor)
                        {
                            float colorScalar = __instance._noteColor.maxColorComponent;
                            if (colorScalar != 0)
                            {
                                faceColor /= colorScalar;
                            }
                        }
                        
                        Color c = Color.LerpUnclamped(isLeft ? Plugin.Config.LeftFaceColor : Plugin.Config.RightFaceColor, faceColor, isLeft ? Plugin.Config.LeftFaceColorNoteSkew : Plugin.Config.RightFaceColorNoteSkew);
                        c.a = 0f;
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                        materialPropertyBlockController.ApplyChanges();   
                    }

                    Transform arrowGlowObject = meshRenderer.transform.parent.Find("NoteArrowGlow");
                    if (arrowGlowObject)
                    {
                        arrowGlowObject.gameObject.SetActive(Plugin.Config.EnableFaceGlow);
                        
                        Transform arrowGlowTransform = arrowGlowObject.transform;
                        
                        Vector3 glowScale = new Vector3(scale.x * Plugin.Config.ArrowGlowScale * 0.633f, scale.y * Plugin.Config.ArrowGlowScale * 0.3f, 0.6f);
                        Vector3 glowPosition = new Vector3(_initialPosition.x + Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
                        
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

                    Vector3 dotPosition;
                    Vector3 glowPosition;
                    Vector3 dotScale;
                    Vector3 glowScale;
                    if (isChainLink)
                    {
                        dotPosition = new Vector3(_initialChainDotPosition.x, _initialChainDotPosition.y, _initialChainDotPosition.z);
                        glowPosition = new Vector3(_initialChainDotPosition.x, _initialChainDotPosition.y, _initialChainDotPosition.z + 0.001f);
                        dotScale = new Vector3(Plugin.Config.ChainDotScale.x / 18f, Plugin.Config.ChainDotScale.y / 18f, 1.0f);
                        glowScale = new Vector3((Plugin.Config.ChainDotScale.x / 5.4f) * Plugin.Config.DotGlowScale, (Plugin.Config.ChainDotScale.y / 5.4f) * Plugin.Config.DotGlowScale, 1.0f);
                    }
                    else
                    {
                        dotPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z);
                        glowPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z + 0.001f);
                        dotScale = new Vector3(Plugin.Config.DotScale.x / 5f, Plugin.Config.DotScale.y / 5f, 1.0f);
                        glowScale = new Vector3((Plugin.Config.DotScale.x / 1.5f) * Plugin.Config.DotGlowScale, (Plugin.Config.DotScale.y / 1.5f) * Plugin.Config.DotGlowScale, 1.0f);
                    }
                    
                    Transform originalDot = isChainLink ? meshRenderer.transform.parent.Find("Circle") : meshRenderer.transform.parent.Find("NoteCircleGlow");
                    Transform addedDot = meshRenderer.transform.parent.Find("AddedNoteCircleGlow");
                    if (originalDot)
                    {
                        if (isChainLink)
                        {
                            originalDot.gameObject.SetActive(Plugin.Config.EnableChainDots);

                            if (!Plugin.Config.EnableChainDots)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            originalDot.gameObject.SetActive(Plugin.Config.EnableDots);

                            if (!Plugin.Config.EnableDots)
                            {
                                continue;
                            }   
                        }
                        
                        Transform originalDotTransform = originalDot.transform;
                        
                        if (!_initialDotPositionDidSet)
                        {
                            _initialDotPositionDidSet = true;
                            _initialDotPosition = originalDotTransform.localPosition;
                        }
                        if (!_initialChainDotPositionDidSet)
                        {
                            _initialChainDotPositionDidSet = true;
                            _initialChainDotPosition = originalDotTransform.localPosition;
                        }
                        
                        originalDotTransform.localScale = dotScale;
                        originalDotTransform.localPosition = dotPosition;
                        if (!isChainLink)
                        {
                            originalDotTransform.localRotation = Quaternion.identity;
                            originalDotTransform.Rotate(0f, 0f, Plugin.Config.RotateDot);
                        }

                        if (_dotMesh == null)
                        {
                            _dotMesh = Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);
                        }
                        meshRenderer.GetComponent<MeshFilter>().mesh = _dotMesh;
                        
                        meshRenderer.sharedMaterial = Materials.ReplacementDotMaterial;
                        
                        if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                        {
                            ColorType colorType = __instance._noteController.noteData.colorType;
                            bool isLeft = colorType == ColorType.ColorA;

                            Color faceColor = __instance._noteColor;
                            
                            if (isLeft ? Plugin.Config.NormalizeLeftFaceColor : Plugin.Config.NormalizeRightFaceColor)
                            {
                                float colorScalar = __instance._noteColor.maxColorComponent;
                                if (colorScalar != 0)
                                {
                                    faceColor /= colorScalar;
                                }
                            }
                        
                            Color c = Color.LerpUnclamped(isLeft ? Plugin.Config.LeftFaceColor : Plugin.Config.RightFaceColor, faceColor, isLeft ? Plugin.Config.LeftFaceColorNoteSkew : Plugin.Config.RightFaceColorNoteSkew);
                            c.a = 0f;
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, c);
                            materialPropertyBlockController.ApplyChanges();   
                        }

                        if (isChainLink)
                        {
                            if (!Plugin.Config.EnableChainDotGlow)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!Plugin.Config.EnableFaceGlow)
                            {
                                continue;
                            }   
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
                            newGlowObject.transform.Rotate(0f, 0f, Plugin.Config.RotateDot);
                        }

                        if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                        {
                            newGlowMeshRenderer.sharedMaterial = Materials.DotGlowMaterial;
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(SliderController), "Hide")]
        public static class SliderControllerPatch
        {
            // ReSharper disable once InconsistentNaming
            [UsedImplicitly]
            private static bool Prefix(SliderController __instance)
            {
                if (!Plugin.Config.Enabled || AutoDisable)
                {
                    return true;
                }
                
                Color color = __instance._saber.saberType == SaberType.SaberA ? NoteColorTweaks.OriginalLeftColor : NoteColorTweaks.OriginalRightColor;

                __instance._initColor = color;
                __instance._materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, color);
                __instance._materialPropertyBlockController.ApplyChanges();

                return true;
            }
        }
    }
}