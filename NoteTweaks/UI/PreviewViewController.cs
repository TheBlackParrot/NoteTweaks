using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Empty.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Empty.bsml")]
    internal class NotePreviewViewController : BSMLAutomaticViewController
    {
        internal static GameObject NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
        
        private static readonly float NoteSize = 0.5f;
        private static readonly Vector3 InitialPosition = new Vector3(-2.5f, 1.5f, 3f);
        
        internal static bool _hasInitialized;
        private static Vector3 _initialArrowPosition = Vector3.one;
        private static Vector3 _initialDotPosition = Vector3.one;

        private static Material _replacementDotMaterial;
        private static Material _dotGlowMaterial;
        private static Mesh _dotMesh;
        private static Mesh _dotGlowMesh;
        private static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        private static readonly Texture2D ReplacementArrowGlowTexture = Utils.Textures.PrepareTexture(OriginalArrowGlowTexture);
        private static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        private static readonly Texture2D ReplacementDotGlowTexture = Utils.Textures.PrepareTexture(OriginalDotGlowTexture);
        
        private static readonly int Color0 = Shader.PropertyToID("_Color");
        
        private static readonly List<String> FaceNames = new List<String> { "NoteArrow", "NoteCircleGlow" };
        private static readonly List<String> GlowNames = new List<String> { "NoteArrowGlow", "AddedNoteCircleGlow" };

        public static void UpdateDotMesh()
        {
            if (_dotGlowMesh == null)
            {
                _dotGlowMesh = NoteContainer.transform.GetChild(0).Find("NoteCircleGlow").GetComponent<MeshFilter>().mesh;
            }
            
            if (_dotMesh != null)
            {
                _dotMesh.Clear();
            }

            _dotMesh = Utils.Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.Find("NoteCircleGlow").GetComponent<MeshFilter>().mesh = _dotMesh;
            }
        }

        public static void UpdateVisibility()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                
                if (noteCube.transform.Find("NoteArrow").gameObject.activeSelf)
                {
                    // is not a dot note
                    noteCube.transform.Find("NoteArrowGlow").gameObject.SetActive(Plugin.Config.EnableFaceGlow);
                }
                else
                {
                    // is a dot note
                    GameObject dotObject = noteCube.transform.Find("NoteCircleGlow").gameObject;
                    dotObject.SetActive(Plugin.Config.EnableDots);
                    noteCube.transform.Find("AddedNoteCircleGlow").gameObject.SetActive(Plugin.Config.EnableFaceGlow && Plugin.Config.EnableDots);
                }
            }
        }
        
        public static void UpdateDotPosition()
        {
            if (_initialDotPosition == Vector3.one)
            {
                _initialDotPosition = NoteContainer.transform.GetChild(0).Find("NoteCircleGlow").localPosition;
            }
            
            Vector3 position = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z);
            Vector3 glowPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z + 0.001f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.Find("NoteCircleGlow").localPosition = position;
                noteCube.transform.Find("AddedNoteCircleGlow").localPosition = glowPosition;
            }
        }

        public static void UpdateDotScale()
        {
            Vector3 scale = new Vector3(Plugin.Config.DotScale.x / 5f, Plugin.Config.DotScale.y / 5f, 1.0f);
            Vector3 glowScale = new Vector3((Plugin.Config.DotScale.x / 1.5f) * Plugin.Config.GlowScale, (Plugin.Config.DotScale.y / 1.5f) * Plugin.Config.GlowScale, 1.0f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.Find("NoteCircleGlow").localScale = scale;
                noteCube.transform.Find("AddedNoteCircleGlow").localScale = glowScale;
            }
        }
        
        public static void UpdateArrowPosition()
        {
            if (_initialArrowPosition == Vector3.one)
            {
                _initialArrowPosition = NoteContainer.transform.GetChild(0).Find("NoteArrow").localPosition;
            }
            
            Vector3 position = new Vector3(Plugin.Config.ArrowPosition.x, _initialArrowPosition.y + Plugin.Config.ArrowPosition.y, _initialArrowPosition.z);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.Find("NoteArrow").localPosition = position;
                noteCube.transform.Find("NoteArrowGlow").localPosition = position;
            }
        }

        public static void UpdateArrowScale()
        {
            Vector3 scale = new Vector3(Plugin.Config.ArrowScale.x, Plugin.Config.ArrowScale.y, 1.0f);
            Vector3 glowScale = new Vector3(scale.x * Plugin.Config.GlowScale * 0.6f, scale.y * Plugin.Config.GlowScale * 0.3f, 0.6f);
            
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.Find("NoteArrow").localScale = scale;
                noteCube.transform.Find("NoteArrowGlow").localScale = glowScale;
            }
        }

        public static void UpdateNoteScale()
        {
            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                noteCube.transform.localScale = Plugin.Config.NoteScale;
            }
        }

        public static void UpdateColors()
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            ColorScheme colors = playerData.colorSchemesSettings.GetSelectedColorScheme();
            
            float leftScale = 1.0f + Plugin.Config.ColorBoostLeft;
            float rightScale = 1.0f + Plugin.Config.ColorBoostRight;

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;

                Color noteColor = (i % 2 == 0) ? colors._saberAColor * leftScale : colors._saberBColor * rightScale;
                Color faceColor = Color.LerpUnclamped(Plugin.Config.FaceColor, noteColor, Plugin.Config.FaceColorNoteSkew);
                faceColor.a = 0f;
                Color glowColor = noteColor;
                glowColor.a = Plugin.Config.GlowIntensity;
                
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
                            if (childTransform.TryGetComponent(out MaterialPropertyBlockController childController))
                            {
                                childController.materialPropertyBlock.SetColor(Color0, glowColor);
                                childController.ApplyChanges();
                            }   
                        }
                    });
                }
            }
        }

        private static void CreateNote(GameNoteController notePrefab, string extraName, int cell)
        {
            GameObject noteCube = Instantiate(notePrefab.transform.GetChild(0).gameObject, NoteContainer.transform);
            noteCube.gameObject.SetActive(false);
            
            noteCube.name = "_NoteTweaks_PreviewNote_" + extraName;
            DestroyImmediate(noteCube.transform.Find("BigCuttable").gameObject);
            DestroyImmediate(noteCube.transform.Find("SmallCuttable").gameObject);
            
            Vector3 position = new Vector3((cell % 2) * NoteSize, -(int)Math.Floor((float)cell / 2) * NoteSize, 0);
            noteCube.transform.localPosition = position;
            noteCube.transform.Rotate(90f, 0f, 0f);

            /*Animation animationComponent = noteCube.AddComponent<Animation>();
            AnimationClip animationClip = Resources.FindObjectsOfTypeAll<AnimationClip>().First(x => x.name.Contains("LevitatingCube"));
            animationComponent.AddClip(animationClip, "LevitatingCube");
            animationComponent.clip = animationClip;
            foreach (var animationEvent in animationComponent.clip.events)
            {
                animationEvent.objectReferenceParameter = noteCube;
            }
            animationComponent.Play("LevitatingCube", PlayMode.StopAll);
            animationComponent["LevitatingCube"].wrapMode = WrapMode.Loop;*/

            if (_dotMesh == null)
            {
                UpdateDotMesh();
            }

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
                    
                Vector3 dotPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z);
                Vector3 glowPosition = new Vector3(_initialDotPosition.x + Plugin.Config.DotPosition.x, _initialDotPosition.y + Plugin.Config.DotPosition.y, _initialDotPosition.z + 0.001f);
                Vector3 dotScale = new Vector3(Plugin.Config.DotScale.x / 5f, Plugin.Config.DotScale.y / 5f, 1.0f);
                Vector3 glowScale = new Vector3((Plugin.Config.DotScale.x / 1.5f) * Plugin.Config.GlowScale, (Plugin.Config.DotScale.y / 1.5f) * Plugin.Config.GlowScale, 1.0f);

                originalDotTransform.localScale = dotScale;
                originalDotTransform.localPosition = dotPosition;
                    
                MeshRenderer meshRenderer = originalDot.GetComponent<MeshRenderer>();
                    
                meshRenderer.GetComponent<MeshFilter>().mesh = _dotMesh;
                        
                meshRenderer.material = _replacementDotMaterial;
                meshRenderer.sharedMaterial = _replacementDotMaterial;
                    
                GameObject newGlowObject = Instantiate(originalDot.gameObject, originalDot.parent);
                newGlowObject.name = "AddedNoteCircleGlow";
                        
                newGlowObject.GetComponent<MeshFilter>().mesh = _dotGlowMesh;
                newGlowObject.transform.localPosition = glowPosition;
                newGlowObject.transform.localScale = glowScale;

                if (newGlowObject.TryGetComponent(out MeshRenderer newGlowMeshRenderer))
                {
                    newGlowMeshRenderer.material = _dotGlowMaterial;
                    newGlowMeshRenderer.sharedMaterial = _dotGlowMaterial;
                }
            }
            
            noteCube.transform.Find("NoteArrowGlow").GetComponent<MeshRenderer>().material.mainTexture = ReplacementArrowGlowTexture;
            
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
        
        protected void OnEnable()
        {
            if (_hasInitialized)
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
                    shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray()
                };
            }
            if(_dotGlowMaterial == null) {
                Plugin.Log.Info("Creating new dot glow material");
                Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
                _dotGlowMaterial = new Material(arrowGlowMat)
                {
                    mainTexture = ReplacementDotGlowTexture
                };
            }
            
            NoteContainer.transform.position = InitialPosition;
            NoteContainer.transform.localRotation = Quaternion.Euler(0, 315, 0);
            
            MenuTransitionsHelper menuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();
            StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData = menuTransitionsHelper._standardLevelScenesTransitionSetupData;
            SceneInfo gameCoreSceneInfo = standardLevelScenesTransitionSetupData._gameCoreSceneInfo;
            SceneInfo standardGameplaySceneInfo = standardLevelScenesTransitionSetupData._standardGameplaySceneInfo;

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameCoreSceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                operation1 =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(standardGameplaySceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                        operation2 =>
                        {
                            BeatmapObjectsInstaller beatmapObjectsInstaller = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault();
                            GameNoteController notePrefab = beatmapObjectsInstaller._normalBasicNotePrefab;
                            
                            List<String> noteNames = new List<string> { "L_Arrow", "R_Arrow", "L_Dot", "R_Dot" };
                            for (int i = 0; i < noteNames.Count; i++)
                            {
                                CreateNote(notePrefab, noteNames[i], i);
                            }
                            
                            UpdateColors();
                            UpdateArrowPosition();
                            UpdateArrowScale();
                            UpdateDotPosition();
                            UpdateDotScale();
                            UpdateNoteScale();
                            UpdateVisibility();

                            NoteContainer.SetActive(true);

                            _hasInitialized = true;
                            
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(standardGameplaySceneInfo.sceneName);
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(gameCoreSceneInfo.sceneName);
                        };
                };
        }

        protected void OnDisable()
        {
        }
    }
}