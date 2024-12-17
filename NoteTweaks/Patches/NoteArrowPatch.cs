using System;
using System.Linq;
using System.Reflection;
using BeatmapLevelSaveDataVersion4;
using HarmonyLib;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class Container
    {
        private static GameplayModifiers _gameplayModifiers;
        
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
        
        [HarmonyPatch(typeof(GameNoteController), "Init")]
        internal class NotePatch
        {
            internal static void Postfix(ref GameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
            {
                if (!Plugin.Config.Enabled || !IsAllowedToScaleNotes)
                {
                    return;
                }
                
                Vector3 scale = Plugin.Config.NoteScale * Vector3.one;
                Vector3 invertedScale = (1.0f / Plugin.Config.NoteScale) * Vector3.one;

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
                
                foreach (MeshRenderer meshRenderer in ____circleMeshRenderers)
                {
                    Vector3 scale = new Vector3(Plugin.Config.DotScale.x / 2, Plugin.Config.DotScale.y / 2, 1.0f);
                    
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
                        
                        Vector3 glowPosition = new Vector3(Plugin.Config.ArrowPosition.x, _initialDotPosition.y + Plugin.Config.ArrowPosition.y, _initialDotPosition.z);
                        
                        dotGlowTransform.localScale = scale;
                        dotGlowTransform.localPosition = glowPosition;
                    }
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
            }
        }
    }
}