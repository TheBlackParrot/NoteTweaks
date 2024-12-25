using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class NotePhysicalTweaks
    {
        private static GameplayModifiers _gameplayModifiers;
        private static Material _replacementDotMaterial;
        private static Material _dotGlowMaterial;
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
            private static bool _initialChainDotPositionDidSet = false;
            private static Vector3 _initialChainDotPosition = Vector3.zero;
            
            internal static void Postfix(ColorNoteVisuals __instance, ref MeshRenderer[] ____arrowMeshRenderers, ref MeshRenderer[] ____circleMeshRenderers)
            {
                if (!Plugin.Config.Enabled)
                {
                    return;
                }

                if (_replacementDotMaterial == null)
                {
                    Plugin.Log.Info("Creating replacement dot material");
                    Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
                    _replacementDotMaterial = new Material(arrowMat.shader)
                    {
                        color = new Color(1f, 1f, 1f, 1f)
                    };
                }
                if(_dotGlowMaterial == null) {
                    Plugin.Log.Info("Creating new dot glow material");
                    Texture dotGlowTexture = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteCircleGlow").mainTexture;
                    _dotGlowMaterial = new Material(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow"))
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
                        arrowGlowObject.gameObject.SetActive(Plugin.Config.EnableFaceGlow);
                        
                        Transform arrowGlowTransform = arrowGlowObject.transform;
                        
                        Vector3 glowScale = new Vector3(scale.x * Plugin.Config.GlowScale * 0.6f, scale.y * Plugin.Config.GlowScale * 0.3f, 0.6f);
                        Vector3 glowPosition = new Vector3(_initialPosition.x + Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
                        
                        arrowGlowTransform.localScale = glowScale;
                        arrowGlowTransform.localPosition = glowPosition;
                        
                        MeshRenderer arrowGlowMeshRenderer = arrowGlowObject.GetComponent<MeshRenderer>();
                        
                        Color noteColor = arrowGlowObject.parent.parent.GetComponent<ColorNoteVisuals>()._noteColor;
                        noteColor *= Plugin.Config.GlowIntensity;
                        arrowGlowMeshRenderer.material.color = noteColor;
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
                        dotScale = new Vector3(Plugin.Config.ChainDotScale.x / 18f, Plugin.Config.ChainDotScale.y / 18f, 0.0001f);
                        glowScale = new Vector3((Plugin.Config.ChainDotScale.x / 6f) * Plugin.Config.GlowScale, (Plugin.Config.ChainDotScale.y / 6f) * Plugin.Config.GlowScale, 0.0001f);
                    }
                    else
                    {
                        dotPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z);
                        glowPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z + 0.001f);
                        dotScale = new Vector3(Plugin.Config.DotScale.x / 5f, Plugin.Config.DotScale.y / 5f, 0.0001f);
                        glowScale = new Vector3((Plugin.Config.DotScale.x / 1.5f) * Plugin.Config.GlowScale, (Plugin.Config.DotScale.y / 1.5f) * Plugin.Config.GlowScale, 0.0001f);
                    }
                    
                    Transform originalDot = isChainLink ? meshRenderer.transform.parent.Find("Circle") : meshRenderer.transform.parent.Find("NoteCircleGlow");
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

                        while (originalDot.parent.Find("AddedNoteCircleGlow"))
                        {
                            Object.DestroyImmediate(originalDot.parent.Find("AddedNoteCircleGlow").gameObject);
                        }
                        
                        meshRenderer.GetComponent<MeshFilter>().mesh = DotMesh;
                        
                        meshRenderer.material = _replacementDotMaterial;
                        meshRenderer.sharedMaterial = _replacementDotMaterial;

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
                        
                        GameObject newGlowObject = Object.Instantiate(originalDot.gameObject, originalDot.parent).gameObject;
                        newGlowObject.name = "AddedNoteCircleGlow";
                        newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                        newGlowObject.transform.localPosition = glowPosition;
                        newGlowObject.transform.localScale = glowScale;
                        
                        MeshRenderer newGlowMeshRenderer = newGlowObject.GetComponent<MeshRenderer>();
                        newGlowMeshRenderer.material = _dotGlowMaterial;
                        newGlowMeshRenderer.sharedMaterial = _dotGlowMaterial;
                        Color noteColor = originalDot.parent.parent.GetComponent<ColorNoteVisuals>()._noteColor;
                        noteColor *= Plugin.Config.GlowIntensity;
                        newGlowMeshRenderer.material.color = noteColor;
                    }
                }
            }
        }
    }
}