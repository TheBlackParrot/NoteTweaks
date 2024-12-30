using System;
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
        private static readonly GameObject NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
        private static readonly float NoteSize = 0.5f;
        private static readonly Vector3 InitialPosition = new Vector3(-2.6f, 0.8f, 2.0f);
        private static bool _hasInitialized;

        private static GameObject CreateNote(GameNoteController notePrefab, string extraName, int cell)
        {
            PlayerData playerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
            ColorScheme colors = playerData.colorSchemesSettings.GetSelectedColorScheme();
            
            GameObject noteTemplate = Instantiate(notePrefab.transform.GetChild(0).gameObject, NoteContainer.transform);
            
            noteTemplate.name = "_NoteTweaks_PreviewNote_" + extraName;
            DestroyImmediate(noteTemplate.transform.Find("BigCuttable").gameObject);
            DestroyImmediate(noteTemplate.transform.Find("SmallCuttable").gameObject);
            
            Vector3 position = new Vector3((cell % 2) * NoteSize, -(int)Math.Floor((float)cell / 2) * NoteSize, 0);
            noteTemplate.transform.localPosition = position;
            noteTemplate.transform.Rotate(90f, 0f, 0f);
            
            noteTemplate.GetComponent<MeshRenderer>().material.color = (cell % 2 == 0) ? colors._saberAColor : colors._saberBColor;
            noteTemplate.gameObject.SetActive(true);
            
            return noteTemplate;
        }
        
        protected void OnEnable()
        {
            if (_hasInitialized)
            {
                NoteContainer.gameObject.SetActive(true);
                return;
            }
            
            NoteContainer.transform.position = InitialPosition;
            NoteContainer.transform.localRotation = Quaternion.Euler(0, 315, 0);
            
            MenuTransitionsHelper menuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().FirstOrDefault();
            StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData = menuTransitionsHelper._standardLevelScenesTransitionSetupData;
            SceneInfo gameCoreSceneInfo = standardLevelScenesTransitionSetupData._gameCoreSceneInfo;
            SceneInfo standardGameplaySceneInfo = standardLevelScenesTransitionSetupData._standardGameplaySceneInfo;

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameCoreSceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                (_) =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(standardGameplaySceneInfo.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).completed +=
                        (__) =>
                        {
                            BeatmapObjectsInstaller beatmapObjectsInstaller = Resources.FindObjectsOfTypeAll<BeatmapObjectsInstaller>().FirstOrDefault();
                            GameNoteController notePrefab = beatmapObjectsInstaller._normalBasicNotePrefab;

                            //notePrefab.Init(NoteData.CreateBasicNoteData(1f, 1f, 0, 1, NoteLineLayer.Upper, ColorType.ColorA, NoteCutDirection.Down), 0f, Vector3.zero, Vector3.one, Vector3.one, 0f, 1f, 0f, NoteVisualModifierType.Normal, 0f, 1f );

                            CreateNote(notePrefab, "L_Arrow", 0);
                            CreateNote(notePrefab, "R_Arrow", 1);
                            CreateNote(notePrefab, "L_Dot", 2);
                            CreateNote(notePrefab, "R_Dot", 3);
                            
                            NoteContainer.SetActive(true);

                            _hasInitialized = true;
                            
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(standardGameplaySceneInfo.sceneName);
                            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(gameCoreSceneInfo.sceneName);
                        };
                };
        }

        protected void OnDisable()
        {
            NoteContainer.SetActive(false);
        }
    }
}