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
        private static readonly int Color0 = Shader.PropertyToID("_Color");
        private static Vector3 _initialPosition = Vector3.one;
        
        private static readonly List<String> FaceNames = new List<String> { "NoteArrow" };
        private static readonly List<String> GlowNames = new List<String> { "NoteArrowGlow" };

        public static void UpdateVisibility()
        {
            if (Plugin.Config.EnableDots)
            {
                // todo
            }

            for (int i = 0; i < NoteContainer.transform.childCount; i++)
            {
                GameObject noteCube = NoteContainer.transform.GetChild(i).gameObject;
                GlowNames.ForEach(glowName => noteCube.transform.Find("NoteArrowGlow").gameObject.SetActive(Plugin.Config.EnableFaceGlow));
            }
        }
        
        public static void UpdateArrowPosition()
        {
            if (_initialPosition == Vector3.one)
            {
                _initialPosition = NoteContainer.transform.GetChild(0).Find("NoteArrow").localPosition;
            }
            
            Vector3 position = new Vector3(Plugin.Config.ArrowPosition.x, _initialPosition.y + Plugin.Config.ArrowPosition.y, _initialPosition.z);
            
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
                        MaterialPropertyBlockController childController = controller.transform.Find(childName).GetComponent<MaterialPropertyBlockController>();
                        childController.materialPropertyBlock.SetColor(Color0, faceColor);
                        childController.ApplyChanges(); 
                    });
                    
                    GlowNames.ForEach(childName =>
                    {
                        MaterialPropertyBlockController childController = controller.transform.Find(childName).GetComponent<MaterialPropertyBlockController>();
                        childController.materialPropertyBlock.SetColor(Color0, glowColor);
                        childController.ApplyChanges(); 
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
            
            noteCube.gameObject.SetActive(true);
        }
        
        protected void OnEnable()
        {
            if (_hasInitialized)
            {
                return;
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
                            UpdateNoteScale();

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