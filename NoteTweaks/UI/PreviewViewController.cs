using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using NoteTweaks.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Empty.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Empty.bsml")]
    internal class NotePreviewViewController : BSMLAutomaticViewController
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        internal static GameObject NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
        
        private static readonly float NoteSize = 0.5f;
        private static Vector3 _initialPosition = new Vector3(0f, 0.67f, 4.5f);
        
        internal static bool HasInitialized;
        private static Vector3 _initialArrowPosition = Vector3.one;
        private static Vector3 _initialDotPosition = Vector3.one;
        private static Vector3 _initialChainDotPosition = Vector3.one;
        
        private static Mesh _dotGlowMesh;
        
        private static readonly int Color0 = Shader.PropertyToID("_Color");
        private static readonly int Color1 = Shader.PropertyToID("_SimpleColor");

        private static readonly List<string> FaceNames = new List<string> { "NoteArrow", "NoteCircleGlow", "Circle" };
        private static readonly List<string> GlowNames = new List<string> { "NoteArrowGlow", "AddedNoteCircleGlow" };

        public NotePreviewViewController()
        {
            DontDestroyOnLoad(NoteContainer);
        }
        
        [UIValue("NoteContainerZPos")]
        [UsedImplicitly]
        private float NoteContainerZPos
        {
            get => _initialPosition.z;
            set
            {
                _initialPosition.z = value;
                NotifyPropertyChanged();
            }
        }

        public static void UpdateDotMesh()
        {
            if (_dotGlowMesh == null)
            {
                _dotGlowMesh = NoteContainer.transform.GetChild(0).Find("NoteCircleGlow").GetComponent<MeshFilter>().mesh;
            }

            Managers.Meshes.DotMesh = Utils.Meshes.GenerateFaceMesh(Config.DotMeshSides);
            //Managers.Meshes.DotMesh = Utils.Meshes.GenerateStarMesh(Config.DotMeshSides);

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform noteCircleGlowTransform = noteCube.transform.Find("NoteCircleGlow");
                if (noteCircleGlowTransform != null)
                {
                    noteCircleGlowTransform.GetComponent<MeshFilter>().mesh = Managers.Meshes.DotMesh;
                }
                else
                {
                    noteCube.transform.Find("Circle").GetComponent<MeshFilter>().mesh = Managers.Meshes.DotMesh;
                }
            }
        }

        public static void UpdateVisibility()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform noteArrowTransform = noteCube.transform.Find("NoteArrow");

                if (noteArrowTransform != null)
                {
                    if (noteCube.transform.Find("NoteArrow").gameObject.activeSelf)
                    {
                        // is not a dot note
                        noteCube.transform.Find("NoteArrowGlow").gameObject.SetActive(Config.EnableFaceGlow);
                    }
                    else
                    {
                        // is a dot note
                        Transform noteCircleGlowTransform = noteCube.transform.Find("NoteCircleGlow");
                        if (noteCircleGlowTransform != null)
                        {
                            GameObject dotObject = noteCircleGlowTransform.gameObject;
                            dotObject.SetActive(Config.EnableDots);
                            noteCube.transform.Find("AddedNoteCircleGlow").gameObject.SetActive(Config.EnableFaceGlow && Config.EnableDots);
                        }
                    }
                }
                else
                {
                    // is a chain link
                    GameObject dotObject = noteCube.transform.Find("Circle").gameObject;
                    dotObject.SetActive(Config.EnableChainDots);
                    noteCube.transform.Find("AddedNoteCircleGlow").gameObject.SetActive(Config.EnableChainDotGlow && Config.EnableChainDots);
                }
            }
        }
        
        public static void UpdateDotPosition()
        {
            if (_initialDotPosition == Vector3.one)
            {
                _initialDotPosition = NoteContainer.transform.GetChild(0).Find("NoteCircleGlow").localPosition;
            }
            
            Vector3 dotPosition = new Vector3(_initialDotPosition.x + Config.DotPosition.x, _initialDotPosition.y + Config.DotPosition.y, _initialDotPosition.z);
            Vector3 initialDotGlowPosition = new Vector3(_initialDotPosition.x + Config.DotPosition.x, _initialDotPosition.y + Config.DotPosition.y, _initialDotPosition.z + 0.001f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform noteCircleGlowTransform = noteCube.transform.Find("NoteCircleGlow");
                if (noteCircleGlowTransform != null)
                {
                    Vector3 dotGlowPosition = initialDotGlowPosition + (Vector3)(noteCube.name.Contains("_L_") ? Config.LeftGlowOffset : Config.RightGlowOffset);
                    
                    noteCircleGlowTransform.localPosition = dotPosition;
                    noteCube.transform.Find("AddedNoteCircleGlow").localPosition = dotGlowPosition;
                }
            }
        }

        public static void UpdateDotScale()
        {
            Vector3 scale = new Vector3(Config.DotScale.x / 5f, Config.DotScale.y / 5f, 1.0f);
            Vector3 glowScale = new Vector3((Config.DotScale.x / 1.5f) * Config.DotGlowScale, (Config.DotScale.y / 1.5f) * Config.DotGlowScale, 1.0f);
            Vector3 chainLinkDotScale = new Vector3(Config.ChainDotScale.x / 18f, Config.ChainDotScale.y / 18f, 1.0f);
            Vector3 chainLinkGlowScale = new Vector3((Config.ChainDotScale.x / 5.4f) * Config.DotGlowScale, (Config.ChainDotScale.y / 5.4f) * Config.DotGlowScale, 1.0f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform noteCircleGlowTransform = noteCube.transform.Find("NoteCircleGlow");
                if (noteCircleGlowTransform != null)
                {
                    noteCircleGlowTransform.localScale = scale;
                    noteCube.transform.Find("AddedNoteCircleGlow").localScale = glowScale;   
                }
                else
                {
                    noteCube.transform.Find("Circle").localScale = chainLinkDotScale;  
                    noteCube.transform.Find("AddedNoteCircleGlow").localScale = chainLinkGlowScale;  
                }
            }
        }

        public static void UpdateDotRotation()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform noteCircleGlowTransform = noteCube.transform.Find("NoteCircleGlow");
                Transform addedNoteCircleGlowTransform = noteCube.transform.Find("AddedNoteCircleGlow");

                if (noteCircleGlowTransform != null)
                {
                    noteCircleGlowTransform.localRotation = Quaternion.identity;
                    noteCircleGlowTransform.Rotate(0f, 0f, Config.RotateDot);
                    addedNoteCircleGlowTransform.localRotation = Quaternion.identity;
                    addedNoteCircleGlowTransform.Rotate(0f, 0f, Config.RotateDot);
                }
            }            
        }
        
        public static void UpdateArrowPosition()
        {
            if (_initialArrowPosition == Vector3.one)
            {
                _initialArrowPosition = NoteContainer.transform.GetChild(0).Find("NoteArrow").localPosition;
            }
            
            Vector3 position = new Vector3(Config.ArrowPosition.x, _initialArrowPosition.y + Config.ArrowPosition.y, _initialArrowPosition.z);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Chain_") || noteCube.name.Contains("_Bomb_"))
                {
                    // not an arrow note, move on
                    continue;
                }
                
                noteCube.transform.Find("NoteArrow").localPosition = position;
                noteCube.transform.Find("NoteArrowGlow").localPosition = position + (Vector3)(noteCube.name.Contains("_L_") ? Config.LeftGlowOffset : Config.RightGlowOffset);
            }
        }

        public static void UpdateArrowScale()
        {
            Vector3 scale = new Vector3(Config.ArrowScale.x, Config.ArrowScale.y, 1.0f);
            Vector3 glowScale = new Vector3(scale.x * Config.ArrowGlowScale * 0.6f, scale.y * Config.ArrowGlowScale * 0.3f, 0.6f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Chain_") || noteCube.name.Contains("_Bomb_"))
                {
                    // not an arrow note, move on
                    continue;
                }
                
                noteCube.transform.Find("NoteArrow").localScale = scale;
                noteCube.transform.Find("NoteArrowGlow").localScale = glowScale;
            }
        }

        public static void UpdateNoteScale()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    // bombs are not notes
                    continue;
                }

                noteCube.transform.localScale = noteCube.name.Contains("_Chain_") ? Vectors.Max(Config.NoteScale * Config.LinkScale, 0.1f) : Config.NoteScale;
            }
        }
        
        public static void UpdateBombScale()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject bombObject = NoteContainer.transform.GetChild(i).gameObject;
                if (!bombObject.name.Contains("_Bomb_"))
                {
                    continue;
                }

                bombObject.transform.localScale = Vector3.one * Config.BombScale;
            }
        }

        public static void UpdateColors()
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            ColorScheme colors = playerData.colorSchemesSettings.GetSelectedColorScheme();
            
            if (!playerData.colorSchemesSettings.overrideDefaultColors)
            {
                colors = playerData.colorSchemesSettings.GetColorSchemeForId("TheFirst");
            }

            Color leftColor = colors._saberAColor;
            float leftBrightness = leftColor.Brightness();
            Color rightColor = colors._saberBColor;
            float rightBrightness = rightColor.Brightness();
            
            if (leftBrightness > Config.LeftMaxBrightness)
            {
                leftColor = leftColor.LerpRGBUnclamped(Color.black, Mathf.InverseLerp(leftBrightness, 0.0f, Config.LeftMaxBrightness));
            }
            else if (leftBrightness < Config.LeftMinBrightness)
            {
                leftColor = leftColor.LerpRGBUnclamped(Color.white, Mathf.InverseLerp(leftBrightness, 1.0f, Config.LeftMinBrightness));
            }
            
            if (rightBrightness > Config.RightMaxBrightness)
            {
                rightColor = rightColor.LerpRGBUnclamped(Color.black, Mathf.InverseLerp(rightBrightness, 0.0f, Config.RightMaxBrightness));
            }
            else if (rightBrightness < Config.RightMinBrightness)
            {
                rightColor = rightColor.LerpRGBUnclamped(Color.white, Mathf.InverseLerp(rightBrightness, 1.0f, Config.RightMinBrightness));
            }
            
            float leftScale = 1.0f + Config.ColorBoostLeft;
            float rightScale = 1.0f + Config.ColorBoostRight;

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Bomb_"))
                {
                    // bombs are separate
                    continue;
                }

                // scaling is intentionally done here
                Color noteColor = (i % 2 == 0) ? leftColor * leftScale : rightColor * rightScale;
                Color faceColor = noteColor;

                float colorScalar = noteColor.maxColorComponent;

                if (colorScalar != 0 && i % 2 == 0 ? Config.NormalizeLeftFaceColor : Config.NormalizeRightFaceColor)
                {
                    faceColor /= colorScalar;
                }

                faceColor = Color.LerpUnclamped(i % 2 == 0 ? Config.LeftFaceColor : Config.RightFaceColor, faceColor, i % 2 == 0 ? Config.LeftFaceColorNoteSkew : Config.RightFaceColorNoteSkew);
                faceColor.a = 1f;
                
                Color glowColor = Color.LerpUnclamped(i % 2 == 0 ? Config.LeftFaceGlowColor : Config.RightFaceGlowColor, noteColor, i % 2 == 0 ? Config.LeftFaceGlowColorNoteSkew : Config.RightFaceGlowColorNoteSkew);
                
                if (colorScalar != 0 && i % 2 == 0 ? Config.NormalizeLeftFaceGlowColor : Config.NormalizeRightFaceGlowColor)
                {
                    glowColor /= colorScalar;
                }
                
                glowColor.a = i % 2 == 0 ? Config.LeftGlowIntensity : Config.RightGlowIntensity;
                
                foreach (MaterialPropertyBlockController controller in noteCube.GetComponents<MaterialPropertyBlockController>())
                {
                    controller.materialPropertyBlock.SetColor(Color0, noteColor);
                    controller.ApplyChanges();

                    FaceNames.ForEach(childName =>
                    {
                        Transform childTransform = controller.transform.Find(childName);
                        if (childTransform)
                        {
                            if (childTransform.TryGetComponent(out MaterialPropertyBlockController childController))
                            {
                                childController.materialPropertyBlock.SetColor(Color0, faceColor);
                                childController.ApplyChanges();
                            }   
                        }
                    });
                    
                    GlowNames.ForEach(childName =>
                    {
                        Transform childTransform = controller.transform.Find(childName);
                        if (childTransform)
                        {
                            Enum.TryParse(i % 2 == 0 ? Config.LeftGlowBlendOp : Config.RightGlowBlendOp, out BlendOp operation);
                            
                            if (childTransform.TryGetComponent(out MaterialPropertyBlockController childController))
                            {
                                foreach (Renderer renderer in childController.renderers)
                                {
                                    // doing this to force a refresh on .material specifically
                                    // for some reason setting the _BlendOp property breaks material instancing
                                    renderer.material = null;
                                    renderer.sharedMaterial = null;
                                    renderer.sharedMaterial = childName.Contains("Arrow") ? Materials.ArrowGlowMaterial : Materials.DotGlowMaterial;
                                    renderer.material = renderer.sharedMaterial;
                                    // (this will never be null, Rider is angy)
                                    // ReSharper disable once PossibleNullReferenceException
                                    renderer.material.SetInt(Materials.BlendOpID, (int)operation);
                                }
                                
                                childController.materialPropertyBlock.SetColor(Color0, glowColor);
                                childController.ApplyChanges();
                            }   
                        }
                    });
                }
            }
        }
        
        public static void UpdateBombColors()
        {
            float scale = 1.0f + Config.BombColorBoost;

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject bombObj = NoteContainer.transform.GetChild(i).gameObject;
                if (!bombObj.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Color bombColor = Config.EnableRainbowBombs ? RainbowGradient.Color : Config.BombColor * scale;
                
                foreach (MaterialPropertyBlockController controller in bombObj.GetComponents<MaterialPropertyBlockController>())
                {
                    controller.materialPropertyBlock.SetColor(Color1, bombColor);
                    controller.ApplyChanges();
                }
            }
        }

        public static void UpdateArrowMeshes()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                if (noteCube.name.Contains("_Chain_") || noteCube.name.Contains("_Bomb_"))
                {
                    continue;
                }
                
                Transform originalArrow = noteCube.transform.Find("NoteArrow");
                if (originalArrow)
                {
                    if (originalArrow.gameObject.TryGetComponent(out MeshFilter originalArrowMeshFilter))
                    {
                        Managers.Meshes.UpdateDefaultArrowMesh(originalArrowMeshFilter.mesh);
                        originalArrowMeshFilter.mesh = Managers.Meshes.CurrentArrowMesh;
                    }
                }
            }
        }

        public static void UpdateOutlines()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                
                bool isLeft = noteCube.name.Contains("_L_");
                bool isBomb = noteCube.name.Contains("_Bomb_");
                
                Color noteColor = Config.BombColor;
                if (noteCube.TryGetComponent(out MaterialPropertyBlockController noteMaterialController))
                {
                    noteColor = noteMaterialController.materialPropertyBlock.GetColor(Color0);
                }
                
                float colorScalar = noteColor.maxColorComponent;

                if (colorScalar != 0 && !isBomb && isLeft ? Config.NormalizeLeftOutlineColor : Config.NormalizeRightOutlineColor)
                {
                    noteColor /= colorScalar;
                }
                
                Color outlineColor = Config.BombOutlineColor;
                if (!isBomb)
                {
                    outlineColor = Color.LerpUnclamped(isLeft ? Config.NoteOutlineLeftColor : Config.NoteOutlineRightColor, noteColor, isLeft ? Config.NoteOutlineLeftColorSkew : Config.NoteOutlineRightColorSkew);   
                }
                
                Transform noteOutline = noteCube.transform.FindChildRecursively("NoteOutline");
                if (noteOutline)
                {
                    noteOutline.gameObject.SetActive(isBomb ? Config.EnableBombOutlines : Config.EnableNoteOutlines);
                    noteOutline.localScale = (Vector3.one * ((isBomb ? Config.BombOutlineScale : Config.NoteOutlineScale) / 100f)) + Vector3.one;
                    
                    if (noteOutline.gameObject.TryGetComponent(out MaterialPropertyBlockController controller))
                    {
                        controller.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, outlineColor);
                        controller.ApplyChanges();
                    }
                }
            }
        }

        private static void CreateChainNote(BurstSliderGameNoteController chainPrefab, string extraName, int cell, int linkNum)
        {
            GameObject chainNote = Instantiate(chainPrefab.transform.GetChild(0).gameObject, NoteContainer.transform);
            chainNote.gameObject.SetActive(false);
            
            chainNote.name = "_NoteTweaks_PreviewNote_" + extraName + $"_{linkNum}";
            DestroyImmediate(chainNote.transform.Find("BigCuttable").gameObject);
            DestroyImmediate(chainNote.transform.Find("SmallCuttable").gameObject);
            
            if (Outlines.InvertedChainMesh == null)
            {
                if (chainNote.TryGetComponent(out MeshFilter chainMeshFilter))
                {
                    Outlines.UpdateDefaultChainMesh(chainMeshFilter.sharedMesh);
                }
            }

            Outlines.AddOutlineObject(chainNote.transform, Outlines.InvertedChainMesh);
            
            Vector3 position = new Vector3(((-NoteSize / 2) + (cell - (NoteSize * 2)) * NoteSize) - 0.1f, linkNum / 6.667f, linkNum * -0.05f);
            chainNote.transform.localPosition = position;
            chainNote.transform.Rotate(90f, 0f, 0f);
            
            MeshRenderer noteMeshRenderer = chainNote.gameObject.GetComponent<MeshRenderer>();
            noteMeshRenderer.sharedMaterial = Materials.NoteMaterial;
            
            Transform originalDot = chainNote.transform.Find("Circle");
            if (originalDot)
            {
                Transform originalDotTransform = originalDot.transform;
                        
                if (_initialChainDotPosition == Vector3.one)
                {
                    _initialChainDotPosition = originalDotTransform.localPosition;
                }
                    
                Vector3 dotPosition = new Vector3(_initialChainDotPosition.x, _initialChainDotPosition.y, _initialChainDotPosition.z - 0.002f);
                Vector3 glowPosition = new Vector3(_initialChainDotPosition.x, _initialChainDotPosition.y, _initialChainDotPosition.z - 0.001f);
                Vector3 dotScale = new Vector3(Config.ChainDotScale.x / 18f, Config.ChainDotScale.y / 18f, 1.0f);
                Vector3 glowScale = new Vector3((Config.ChainDotScale.x / 5.4f) * Config.DotGlowScale, (Config.ChainDotScale.y / 5.4f) * Config.DotGlowScale, 1.0f);

                originalDotTransform.localScale = dotScale;
                originalDotTransform.localPosition = dotPosition;
                    
                MeshRenderer meshRenderer = originalDot.GetComponent<MeshRenderer>();
                    
                meshRenderer.GetComponent<MeshFilter>().mesh = Managers.Meshes.DotMesh;
                
                meshRenderer.sharedMaterial = Materials.ReplacementDotMaterial;
                    
                GameObject newGlowObject = Instantiate(originalDot.gameObject, originalDot.parent);
                newGlowObject.name = "AddedNoteCircleGlow";
                        
                newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                newGlowObject.transform.localPosition = glowPosition;
                newGlowObject.transform.localScale = glowScale;

                if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                {
                    newGlowMeshRenderer.sharedMaterial = Materials.DotGlowMaterial;
                }
            }
            
            chainNote.gameObject.SetActive(true);
        }

        private static void CreateNote(GameNoteController notePrefab, string extraName, int cell)
        {
            GameObject noteCube = Instantiate(notePrefab.transform.GetChild(0).gameObject, NoteContainer.transform);
            noteCube.gameObject.SetActive(false);
            
            noteCube.name = "_NoteTweaks_PreviewNote_" + extraName;
            DestroyImmediate(noteCube.transform.Find("BigCuttable").gameObject);
            DestroyImmediate(noteCube.transform.Find("SmallCuttable").gameObject);
            
            if (Outlines.InvertedNoteMesh == null)
            {
                if (noteCube.TryGetComponent(out MeshFilter cubeMeshFilter))
                {
                    Outlines.UpdateDefaultNoteMesh(cubeMeshFilter.sharedMesh);
                }
            }

            Outlines.AddOutlineObject(noteCube.transform, Outlines.InvertedNoteMesh);
            
            Vector3 position = new Vector3(((NoteSize / 2) + (cell % 2) * NoteSize) + 0.1f, (-(int)Math.Floor((float)cell / 2) * NoteSize) + NoteSize + 0.15f, 0);
            noteCube.transform.localPosition = position;
            noteCube.transform.Rotate(90f, 0f, 0f);
            
            MeshRenderer noteMeshRenderer = noteCube.gameObject.GetComponent<MeshRenderer>();
            noteMeshRenderer.sharedMaterial = Materials.NoteMaterial;
            
            if (_dotGlowMesh == null)
            {
                _dotGlowMesh = noteCube.transform.Find("NoteCircleGlow").GetComponent<MeshFilter>().mesh;
            }
            
            Transform originalDot = noteCube.transform.Find("NoteCircleGlow");
            if (originalDot)
            {
                Transform originalDotTransform = originalDot.transform;
                        
                if (_initialDotPosition == Vector3.one)
                {
                    _initialDotPosition = originalDotTransform.localPosition;
                }
                    
                Vector3 dotPosition = new Vector3(_initialDotPosition.x + Config.DotPosition.x, _initialDotPosition.y + Config.DotPosition.y, _initialDotPosition.z);
                Vector3 glowPosition = new Vector3(_initialDotPosition.x + Config.DotPosition.x, _initialDotPosition.y + Config.DotPosition.y, _initialDotPosition.z + 0.001f);
                Vector3 dotScale = new Vector3(Config.DotScale.x / 5f, Config.DotScale.y / 5f, 1.0f);
                Vector3 glowScale = new Vector3((Config.DotScale.x / 1.5f) * Config.DotGlowScale, (Config.DotScale.y / 1.5f) * Config.DotGlowScale, 1.0f);

                originalDotTransform.localScale = dotScale;
                originalDotTransform.localPosition = dotPosition;
                    
                MeshRenderer meshRenderer = originalDot.GetComponent<MeshRenderer>();
                    
                meshRenderer.GetComponent<MeshFilter>().mesh = Managers.Meshes.DotMesh;
                        
                meshRenderer.material = Materials.ReplacementDotMaterial;
                meshRenderer.sharedMaterial = Materials.ReplacementDotMaterial;
                    
                GameObject newGlowObject = Instantiate(originalDot.gameObject, originalDot.parent);
                newGlowObject.name = "AddedNoteCircleGlow";
                        
                newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                newGlowObject.transform.localPosition = glowPosition;
                newGlowObject.transform.localScale = glowScale;

                if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                {
                    newGlowMeshRenderer.material = Materials.DotGlowMaterial;
                    newGlowMeshRenderer.sharedMaterial = Materials.DotGlowMaterial;
                }
            }
            
            noteCube.transform.Find("NoteArrowGlow").GetComponent<MeshRenderer>().sharedMaterial = Materials.ArrowGlowMaterial;
            
            if (cell >= 2)
            {
                // dot notes
                noteCube.transform.Find("NoteArrow").gameObject.SetActive(false);
                noteCube.transform.Find("NoteArrowGlow").gameObject.SetActive(false);
                noteCube.transform.Find("NoteArrow").GetComponent<Renderer>().enabled = false;
                noteCube.transform.Find("NoteArrowGlow").GetComponent<Renderer>().enabled = false;
                
                noteCube.transform.Find("NoteCircleGlow").gameObject.SetActive(true);
                noteCube.transform.Find("AddedNoteCircleGlow").gameObject.SetActive(true);
                noteCube.transform.Find("NoteCircleGlow").GetComponent<Renderer>().enabled = true;
                noteCube.transform.Find("AddedNoteCircleGlow").GetComponent<Renderer>().enabled = true;
            }
            
            noteCube.gameObject.SetActive(true);
        }

        private static void CreateBomb(BombNoteController bombPrefab, string extraName, int cell)
        {
            GameObject bombContainer = Instantiate(bombPrefab.gameObject, NoteContainer.transform);
            bombContainer.gameObject.SetActive(false);
            
            DestroyImmediate(bombContainer.GetComponent<BombNoteController>());
            DestroyImmediate(bombContainer.GetComponent<BaseNoteVisuals>());
            DestroyImmediate(bombContainer.GetComponent<NoteMovement>());
            DestroyImmediate(bombContainer.GetComponent<NoteFloorMovement>());
            DestroyImmediate(bombContainer.GetComponent<NoteJump>());
            
            bombContainer.name = "_NoteTweaks_Bomb_" + extraName;
            
            Vector3 position = new Vector3(-NoteSize + ((NoteSize * 1.25f) * cell) - 0.1f, 1.333f, 0);
            bombContainer.transform.localPosition = position;
            bombContainer.transform.Rotate(90f, 0f, 0f);
            
            GameObject bombObject = bombContainer.transform.GetChild(0).gameObject;
            
            DestroyImmediate(bombObject.GetComponent<SphereCollider>());
            DestroyImmediate(bombObject.GetComponent<SphereCuttableBySaber>());
            
            if (Outlines.InvertedBombMesh == null)
            {
                if (bombObject.TryGetComponent(out MeshFilter bombMeshFilter))
                {
                    Outlines.UpdateDefaultBombMesh(bombMeshFilter.sharedMesh);
                }
            }

            Outlines.AddOutlineObject(bombContainer.transform, Outlines.InvertedBombMesh);
            
            MeshRenderer bombMeshRenderer = bombObject.gameObject.GetComponent<MeshRenderer>();
            bombMeshRenderer.sharedMaterial = Materials.BombMaterial;
            
            bombContainer.gameObject.SetActive(true);
        }

        private static CancellationTokenSource _currentTokenSource;
        public static async Task CutoutFadeOut()
        {
            _floatTokenSource.Cancel();
            
            _currentTokenSource?.Cancel();
            _currentTokenSource?.Dispose();

            _currentTokenSource = new CancellationTokenSource();

            await Animate(time =>
            {
                NoteContainer.transform.localScale = (Vector3.one * (Mathf.Abs(time - 1f) / 2)) + new Vector3(0.5f, 0.5f, 0.5f);
                
                Vector3 pos = _initialPosition;
                pos.y = _initialPosition.y * (Mathf.Abs(time - 1f) * 2f) - _initialPosition.y;
                NoteContainer.transform.localPosition = pos;
                
                for (int i = 0; i < NoteContainer.transform.childCount; i++)
                {
                    GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                    if (noteCube.TryGetComponent(out CutoutEffect cutoutEffect))
                    {
                        cutoutEffect.SetCutout(Easing.OutCirc(time));
                    }
                    
                    for (int j = 0; j < noteCube.transform.childCount; j++)
                    {
                        GameObject childObject = noteCube.transform.GetChild(j).gameObject;
                        if (childObject.TryGetComponent(out CutoutEffect childCutoutEffect))
                        {
                            childCutoutEffect.SetCutout(Easing.OutCirc(time));
                        }
                        
                        for (int k = 0; k < childObject.transform.childCount; k++)
                        {
                            GameObject childChildObject = childObject.transform.GetChild(k).gameObject;
                            if (childChildObject.TryGetComponent(out CutoutEffect childChildCutoutEffect))
                            {
                                childChildCutoutEffect.SetCutout(Easing.OutCirc(time));
                            }
                        }
                    }
                }

                if (time >= 1f)
                {
                    NoteContainer.SetActive(false);
                    _initialPosition.z = 4.5f;
                }
            }, _currentTokenSource.Token, 0.4f);
        }

        private async Task CutoutFadeIn()
        {
            _currentTokenSource?.Cancel();
            _currentTokenSource?.Dispose();

            _currentTokenSource = new CancellationTokenSource();
            
            NoteContainer.transform.localRotation = Quaternion.AngleAxis(0f, new Vector3(0f, 1f, 0f));

            await Animate(time =>
            {
                NoteContainer.transform.localScale = (Vector3.one * (time / 2)) + new Vector3(0.5f, 0.5f, 0.5f);
                
                Vector3 pos = _initialPosition;
                pos.y = _initialPosition.y * (time * 2f) - _initialPosition.y;
                NoteContainer.transform.localPosition = pos;
                
                for (int i = 0; i < NoteContainer.transform.childCount; i++)
                {
                    GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                    if (noteCube.TryGetComponent(out CutoutEffect cutoutEffect))
                    {
                        cutoutEffect.SetCutout(Easing.OutExpo(Mathf.Abs(time - 1f)));
                    }

                    for (int j = 0; j < noteCube.transform.childCount; j++)
                    {
                        GameObject childObject = noteCube.transform.GetChild(j).gameObject;
                        if (childObject.TryGetComponent(out CutoutEffect childCutoutEffect))
                        {
                            childCutoutEffect.SetCutout(Easing.OutExpo(Mathf.Abs(time - 1f)));
                        }
                        
                        for (int k = 0; k < childObject.transform.childCount; k++)
                        {
                            GameObject childChildObject = childObject.transform.GetChild(k).gameObject;
                            if (childChildObject.TryGetComponent(out CutoutEffect childChildCutoutEffect))
                            {
                                childChildCutoutEffect.SetCutout(Easing.OutExpo(Mathf.Abs(time - 1f)));
                            }
                        }
                    }
                }
            }, _currentTokenSource.Token, 0.8f);
            
            _ = MakeNotesFloat();
        }

        private static async Task Animate(Action<float> transition, CancellationToken cancellationToken, float duration)
        {
            float elapsedTime = 0.0f;
            while (elapsedTime <= duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                float value = Easing.InOutCubic(elapsedTime / duration);
                transition?.Invoke(value);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            transition?.Invoke(1f);
        }

        private static CancellationTokenSource _floatTokenSource;
        private static async Task MakeNotesFloat()
        {
            _floatTokenSource?.Cancel();
            _floatTokenSource?.Dispose();

            _floatTokenSource = new CancellationTokenSource();

            float maxRotate = 5.0f;
            float maxBob = 0.05f;
            float rotateTimeScale = 2f;
            float bobTimeScale = 1f;
            
            Vector3 rotateAngle = new Vector3(0f, 1f, 0f);

            await AnimateForever(time => {
                Vector3 pos = _initialPosition;
                pos.y = _initialPosition.y + (Mathf.Sin(time / bobTimeScale) * maxBob);
                
                NoteContainer.transform.localRotation = Quaternion.AngleAxis((Mathf.Sin(time / rotateTimeScale) * maxRotate), rotateAngle);
                NoteContainer.transform.localPosition = pos;
            }, _floatTokenSource.Token);
        }

        private static async Task AnimateForever(Action<float> animation, CancellationToken cancellationToken)
        {
            float startTime = Time.time;
            
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                animation?.Invoke(Time.time - startTime);
                await Task.Yield();
            }
        }
        
        protected void OnEnable()
        {
            if (NoteContainer == null)
            {
                NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
                DontDestroyOnLoad(NoteContainer);
            }
            
            if (HasInitialized)
            {
                _ = CutoutFadeIn();
                return;
            }
            
            NoteContainer.transform.position = _initialPosition;
            
            // ReSharper disable PossibleNullReferenceException
            MenuTransitionsHelper menuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();
            StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData = menuTransitionsHelper._standardLevelScenesTransitionSetupData;
            SceneInfo gameCoreSceneInfo = standardLevelScenesTransitionSetupData._gameCoreSceneInfo;
            SceneInfo standardGameplaySceneInfo = standardLevelScenesTransitionSetupData._standardGameplaySceneInfo;
            
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameCoreSceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                operation1 =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(standardGameplaySceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                        async operation2 =>
                        {
                            BeatmapObjectsInstaller beatmapObjectsInstaller = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault();
                            
                            GameNoteController notePrefab = beatmapObjectsInstaller._normalBasicNotePrefab;
                            BombNoteController bombPrefab = beatmapObjectsInstaller._bombNotePrefab;
                            BurstSliderGameNoteController chainPrefab = beatmapObjectsInstaller._burstSliderNotePrefab;
                            // ReSharper restore PossibleNullReferenceException
                            
                            SettingsViewController.LoadTextures = true;
                            await Materials.UpdateAll();
                            
                            List<String> noteNames = new List<string> { "L_Arrow", "R_Arrow", "L_Dot", "R_Dot" };
                            for (int i = 0; i < noteNames.Count; i++)
                            {
                                CreateNote(notePrefab, noteNames[i], i);
                            }
                            
                            List<String> chainNames = new List<string> { "L_Chain", "R_Chain" };
                            int chainLinkCount = 6;
                            for (int j = 0; j < chainLinkCount; j++)
                            {
                                for (int i = 0; i < chainNames.Count; i++)
                                {
                                    CreateChainNote(chainPrefab, chainNames[i], i, j); 
                                }
                            }

                            for (int i = 0; i < 3; i++)
                            {
                                CreateBomb(bombPrefab, i.ToString(), i);
                            }
                            
                            UpdateColors();
                            UpdateBombColors();
                            UpdateBombScale();
                            UpdateArrowMeshes();
                            UpdateArrowPosition();
                            UpdateArrowScale();
                            UpdateDotPosition();
                            UpdateDotScale();
                            UpdateDotRotation();
                            UpdateNoteScale();
                            UpdateOutlines();
                            UpdateVisibility();

                            NoteContainer.SetActive(true);

                            HasInitialized = true;

                            await CutoutFadeIn();
                            
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(standardGameplaySceneInfo.sceneName);
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(gameCoreSceneInfo.sceneName);
                        };
                };
        }
    }
}