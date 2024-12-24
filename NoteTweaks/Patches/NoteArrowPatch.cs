using System;
using System.Linq;
using System.Reflection;
using BeatmapLevelSaveDataVersion4;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NotePhysicalTweaks
    {
        private static GameplayModifiers _gameplayModifiers;
        internal static Material ReplacementDotMaterial;
        internal static Material DotGlowMaterial;
        private static readonly GameObject DotObject = GameObject.CreatePrimitive(PrimitiveType.Sphere).gameObject;
        private static readonly Mesh DotMesh = DotObject.GetComponent<MeshFilter>().mesh;
        private static Mesh _dotGlowMesh;

        // thanks BeatLeader
        [HarmonyPatch]
        internal class StandardLevelScenesTransitionSetupDataPatch
        {
            static MethodInfo TargetMethod() => AccessTools.FirstMethod(typeof(StandardLevelScenesTransitionSetupDataSO),
                m => m.Name == nameof(StandardLevelScenesTransitionSetupDataSO.Init) &&
                     m.GetParameters().All(p => p.ParameterType != typeof(IBeatmapLevelData)));
            internal static void Postfix(in GameplayModifiers gameplayModifiers)
            {
                _gameplayModifiers = gameplayModifiers;
            }
        }

        private static bool IsAllowedToScaleNotes
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

        [HarmonyPatch(typeof(BurstSliderGameNoteController), "Init")]
        internal class BurstSliderPatch
        {
            internal static void Postfix(ref BurstSliderGameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
            {
                if (!Plugin.Config.Enabled || !IsAllowedToScaleNotes)
                {
                    return;
                }
                
                Vector3 scale = Plugin.Config.NoteScale * Plugin.Config.LinkScale;
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
            internal static void Postfix(ref GameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
            {
                if (!Plugin.Config.Enabled || !IsAllowedToScaleNotes)
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
        
        [HarmonyPatch(typeof(ColorNoteVisuals), "HandleNoteControllerDidInit")]
        internal class NoteArrowPatch
        {
            private static bool _initialPositionDidSet = false;
            private static Vector3 _initialPosition = Vector3.zero;
            private static bool _initialDotPositionDidSet = false;
            private static Vector3 _initialDotPosition = Vector3.zero;
            
            internal static void Postfix(ColorNoteVisuals __instance, ref MeshRenderer[] ____arrowMeshRenderers, ref MeshRenderer[] ____circleMeshRenderers)
            {
                if (!Plugin.Config.Enabled)
                {
                    return;
                }

                if (ReplacementDotMaterial == null)
                {
                    Plugin.Log.Info("Creating replacement dot material");
                    ReplacementDotMaterial = new Material(Shader.Find("Standard"))
                    {
                        color = new Color(1f, 1f, 1f, 0f)
                    };
                }
                if(DotGlowMaterial == null) {
                    Plugin.Log.Info("Creating new dot glow material");
                    Texture dotGlowTexture = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteCircleGlow").mainTexture;
                    DotGlowMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow"))
                    {
                        mainTexture = dotGlowTexture
                    };
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
                    
                    Transform arrowGlowObject = meshRenderer.transform.parent.Find("NoteArrowGlow");
                    if (arrowGlowObject)
                    {
                        arrowGlowObject.gameObject.SetActive(Plugin.Config.EnableArrowGlow);
                        
                        Transform arrowGlowTransform = arrowGlowObject.transform;
                        
                        Vector3 glowScale = new Vector3(scale.x * 0.6f, scale.y * 0.3f, 0.6f);
                        Vector3 glowPosition = new Vector3(_initialPosition.x + Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
                        
                        arrowGlowTransform.localScale = glowScale;
                        arrowGlowTransform.localPosition = glowPosition;
                    }
                }
                
                foreach (MeshRenderer meshRenderer in ____circleMeshRenderers)
                {
                    if (_dotGlowMesh == null)
                    {
                        _dotGlowMesh = meshRenderer.GetComponent<MeshFilter>().mesh;
                    }
                    
                    Vector3 scale = new Vector3(Plugin.Config.DotScale.x / 5f, Plugin.Config.DotScale.y / 5f, 0.0001f);
                    Vector3 glowScale = new Vector3(Plugin.Config.DotScale.x / 1.5f, Plugin.Config.DotScale.y / 1.5f, 0.0001f);
                    
                    Transform dotGlowObject = meshRenderer.transform.parent.Find("NoteCircleGlow");
                    if (dotGlowObject)
                    {
                        dotGlowObject.gameObject.SetActive(Plugin.Config.EnableDots);
                        
                        Transform dotGlowTransform = dotGlowObject.transform;
                        
                        if (!_initialDotPositionDidSet)
                        {
                            _initialDotPositionDidSet = true;
                            _initialDotPosition = dotGlowTransform.localPosition;
                        }
                        
                        Vector3 dotPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z - 0.1f);
                        Vector3 glowPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z - 0.101f);
                        
                        dotGlowTransform.localScale = scale;
                        dotGlowTransform.localPosition = dotPosition;

                        while (dotGlowObject.parent.Find("AddedNoteCircleGlow"))
                        {
                            Object.DestroyImmediate(dotGlowObject.parent.Find("AddedNoteCircleGlow").gameObject);
                        }
                        
                        GameObject newGlowObject = Object.Instantiate(dotGlowObject.gameObject, dotGlowObject.parent).gameObject;
                        newGlowObject.name = "AddedNoteCircleGlow";
                        newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                        newGlowObject.transform.localPosition = glowPosition;
                        newGlowObject.transform.localScale = glowScale;
                        
                        MeshRenderer newGlowMeshRenderer = newGlowObject.GetComponent<MeshRenderer>();
                        newGlowMeshRenderer.material = DotGlowMaterial;
                        newGlowMeshRenderer.sharedMaterial = DotGlowMaterial;
                        newGlowMeshRenderer.material.color = dotGlowObject.parent.parent.GetComponent<ColorNoteVisuals>()._noteColor;
                        
                        meshRenderer.GetComponent<MeshFilter>().mesh = DotMesh;
                        
                        meshRenderer.material = ReplacementDotMaterial;
                        meshRenderer.sharedMaterial = ReplacementDotMaterial;
                    }
                }
            }
        }
        
        // thanks Loloppe
        // https://github.com/Loloppe/BeatSaber_NoteCutGuide/blob/master/HarmonyPatches/GuideInitializer.cs
        [HarmonyPatch(typeof(NoteJump), nameof(NoteJump.ManualUpdate))]
        static class DestroyGuide {
            static void Prefix(ref Transform ____rotatedObject, ref PlayerTransforms ____playerTransforms) {
                // Compatible with Custom Notes and replay, a bit wacky but oh well.
                if (____rotatedObject.position.z <= ____playerTransforms.headPseudoLocalPos.z - 1f) {
                    var guide = ____rotatedObject?.Find("AddedNoteCircleGlow");
                    if(guide != null)
                        Object.DestroyImmediate(guide.gameObject);
                }
            }
        }
    }
}