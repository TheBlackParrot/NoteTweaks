using System;
using System.Linq;
using System.Reflection;
using BeatmapLevelSaveDataVersion4;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NotePhysicalTweaks
    {
        private static GameplayModifiers _gameplayModifiers;
        internal static readonly Material ReplacementDotMaterial = new Material(Shader.Find("Standard"))
        {
            name = "ReplacementDotMaterial",
            color = new Color(1f, 1f, 1f, Plugin.Config.EnableArrowGlow ? 2f : 0f)
        };
        private static readonly GameObject DotObject = GameObject.CreatePrimitive(PrimitiveType.Sphere).gameObject;
        private static readonly Mesh DotMesh = DotObject.GetComponent<MeshFilter>().mesh;

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
                        Vector3 glowPosition = new Vector3(Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
                        
                        arrowGlowTransform.localScale = glowScale;
                        arrowGlowTransform.localPosition = glowPosition;
                    }
                }
                
                foreach (MeshRenderer meshRenderer in ____circleMeshRenderers)
                {
                    Vector3 scale = new Vector3(Plugin.Config.DotScale.x / 5, Plugin.Config.DotScale.y / 5, 0.001f);
                    
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
                        
                        Vector3 glowPosition = new Vector3(Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z - 0.1f);
                        
                        dotGlowTransform.localScale = scale;
                        dotGlowTransform.localPosition = glowPosition;
                        //dotGlowTransform.localRotation = Quaternion.FromToRotation(Vector3.one, Vector3.forward);
                        
                        meshRenderer.GetComponent<MeshFilter>().mesh = DotMesh;
                        
                        meshRenderer.material = ReplacementDotMaterial;
                        meshRenderer.sharedMaterial = ReplacementDotMaterial;
                    }
                }
            }
        }
    }
}