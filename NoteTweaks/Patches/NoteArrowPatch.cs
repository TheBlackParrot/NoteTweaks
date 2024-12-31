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
        private static Mesh _dotMesh;
        private static Mesh _dotGlowMesh;
        
        private static readonly Material AccDotDepthMaterial = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Custom/ClearDepth"))
        {
            name = "AccDotMaterialDepthClear",
            renderQueue = 1996,
            enableInstancing = true
        };
        private static Material _accDotMaterial;
        private static GameObject CreateAccDotObject()
        {
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            if (_accDotMaterial == null)
            {
                _accDotMaterial = new Material(arrowMat)
                {
                    name = "AccDotMaterial",
                    renderQueue = 1997,
                    color = Plugin.Config.AccDotColor,
                    globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive,
                    enableInstancing = true,
                    shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray()
                };
                
                // uncomment later maybe
                // Utils.Materials.RepairShader(AccDotDepthMaterial);
            }

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "NoteHD").renderQueue = 1995;
            if (obj.TryGetComponent(out MeshRenderer meshRenderer))
            {
                Color _c = Plugin.Config.AccDotColor;
                _c.a = 0f;
                _accDotMaterial.color = _c;
                
                meshRenderer.sharedMaterial = _accDotMaterial;
                meshRenderer.material = _accDotMaterial;   
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
        private static readonly float AccDotSizeStep = ScoreModel.kMaxDistanceForDistanceToCenterScore / ScoreModel.kMaxCenterDistanceCutScore;

        private static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        private static readonly Texture2D ReplacementArrowGlowTexture = Utils.Textures.PrepareTexture(OriginalArrowGlowTexture);
        private static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        private static readonly Texture2D ReplacementDotGlowTexture = Utils.Textures.PrepareTexture(OriginalDotGlowTexture);

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
                Plugin.ClampSettings();

                _dotMesh = Utils.Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);
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

        private static bool IsAllowedToUseAccDots
        {
            get
            {
                if (_gameplayModifiers == null)
                {
                    return false;
                }

                return !(_gameplayModifiers.disappearingArrows || _gameplayModifiers.ghostNotes);
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
                scale.x = Mathf.Max(0.1f, scale.x);
                scale.y = Mathf.Max(0.1f, scale.y);
                scale.z = Mathf.Max(0.1f, scale.z);
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
                if (!Plugin.Config.Enabled)
                {
                    return;
                }

                if (Plugin.Config.EnableAccDot && __instance.gameplayType != NoteData.GameplayType.BurstSliderHead && IsAllowedToUseAccDots)
                {
                    if (!_accDotObject)
                    {
                        _accDotObject = CreateAccDotObject();
                    }
                    
                    _accDotObject.transform.localScale = Vector3.one * (AccDotSizeStep * (Mathf.Abs(Plugin.Config.AccDotSize - 15) + 1));

                    if (Plugin.Config.RenderAccDotsAboveSymbols)
                    {
                        _accDotMaterial.renderQueue = 1999;
                        AccDotDepthMaterial.renderQueue = 1998;
                    }
                    else
                    {
                        _accDotMaterial.renderQueue = 1997;
                        AccDotDepthMaterial.renderQueue = 1996;
                    }
                    
                    Color _c = Plugin.Config.AccDotColor;
                    _c.a = 0f;
                    _accDotMaterial.color = _c;
                    
                    foreach (BoxCuttableBySaber saberBox in ____bigCuttableBySaberList)
                    {
                        Transform originalAccDot = saberBox.transform.parent.Find("AccDotObject");
                        if (!originalAccDot && saberBox.transform.parent.TryGetComponent(out MeshRenderer saberBoxMeshRenderer))
                        {
                            GameObject originalAccDotClearDepthObject = Object.Instantiate(_accDotObject, saberBox.transform.parent);
                            originalAccDotClearDepthObject.name = "AccDotObjectDepthClear";
                            if (originalAccDotClearDepthObject.TryGetComponent(out MeshRenderer originalAccDotClearDepthMeshRenderer))
                            {
                                originalAccDotClearDepthMeshRenderer.material = AccDotDepthMaterial;
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
                    _replacementDotMaterial = new Material(arrowMat)
                    {
                        color = Plugin.Config.FaceColor,
                        shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                        renderQueue = 2000
                    };
                }
                if(_dotGlowMaterial == null) {
                    Plugin.Log.Info("Creating new dot glow material");
                    Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
                    _dotGlowMaterial = new Material(arrowGlowMat)
                    {
                        mainTexture = ReplacementDotGlowTexture,
                        renderQueue = 1999
                    };
                }

                bool isChainHead = false;
                if (__instance.gameObject.TryGetComponent(out GameNoteController c))
                {
                    isChainHead = c.gameplayType == NoteData.GameplayType.BurstSliderHead;   
                }
                
                if (Plugin.Config.EnableAccDot && IsAllowedToUseAccDots)
                {
                    _replacementDotMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                    _dotGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
                }
                else
                {
                    _replacementDotMaterial.renderQueue = 2000;
                    _dotGlowMaterial.renderQueue = 1999;
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
                    
                    Color _c = Color.LerpUnclamped(Plugin.Config.FaceColor, __instance._noteColor, Plugin.Config.FaceColorNoteSkew);
                    _c.a = 0f;
                    if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                    {
                        materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, _c);
                        materialPropertyBlockController.ApplyChanges();   
                    }

                    if (Plugin.Config.EnableAccDot && IsAllowedToUseAccDots)
                    {
                        meshRenderer.sharedMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                    }
                    else
                    {
                        meshRenderer.sharedMaterial.renderQueue = 2000;
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

                        if (arrowGlowObject.TryGetComponent(out MeshRenderer arrowGlowMeshRenderer))
                        {
                            arrowGlowMeshRenderer.material.mainTexture = ReplacementArrowGlowTexture;
                            if (isChainHead)
                            {
                                arrowGlowMeshRenderer.material.renderQueue = 2092;
                            }
                            else
                            {
                                if (Plugin.Config.EnableAccDot && IsAllowedToUseAccDots)
                                {
                                    arrowGlowMeshRenderer.material.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
                                }
                                else
                                {
                                    arrowGlowMeshRenderer.material.renderQueue = 1999;
                                }
                            }
                            
                        }
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

                        if (_dotMesh == null)
                        {
                            _dotMesh = Utils.Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);
                        }
                        meshRenderer.GetComponent<MeshFilter>().mesh = _dotMesh;
                        
                        meshRenderer.material = _replacementDotMaterial;
                        meshRenderer.sharedMaterial = _replacementDotMaterial;
                        
                        Color _c = Color.LerpUnclamped(Plugin.Config.FaceColor, __instance._noteColor, Plugin.Config.FaceColorNoteSkew);
                        _c.a = 0f;
                        if (meshRenderer.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                        {
                            materialPropertyBlockController.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, _c);
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
                            
                            __instance._materialPropertyBlockControllers.SetValue(newGlowObject.GetComponent<MaterialPropertyBlockController>(), __instance._materialPropertyBlockControllers.Length - 1);

                            MeshRenderer[] newRendererList = new MeshRenderer[2];
                            __instance._circleMeshRenderers.CopyTo(newRendererList, 0);
                            newRendererList.SetValue(newGlowObject.GetComponent<MeshRenderer>(), 1);
                            __instance._circleMeshRenderers = newRendererList;
                        }
                        
                        newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                        newGlowObject.transform.localPosition = glowPosition;
                        newGlowObject.transform.localScale = glowScale;

                        if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                        {
                            newGlowMeshRenderer.material = _dotGlowMaterial;
                            newGlowMeshRenderer.sharedMaterial = _dotGlowMaterial;

                            if (isChainLink)
                            {
                                // :|
                                newGlowMeshRenderer.sharedMaterial.renderQueue = 2092;
                                newGlowMeshRenderer.material.renderQueue = 2092;
                            }
                        }
                    }
                }
                
                __instance._materialPropertyBlockControllers.DoIf(x => x.name != "NoteCube" && x.name != "NoteCircleGlow" && x.name != "Circle", controller =>
                {
                    Color noteColor = __instance._noteColor;
                    noteColor.a = Plugin.Config.GlowIntensity;
                    
                    controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, noteColor);
                    controller.ApplyChanges();
                });
            }
        }
    }
}