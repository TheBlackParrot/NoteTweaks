using HMUI;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    internal class SettingsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        internal static SettingsViewController _settingsViewController;
        private NotePreviewViewController _notePreviewViewController;
        private ExtraPanelViewController _extraPanelViewController;
        
        private static PluginConfig Config => PluginConfig.Instance;
        
        [Inject]
        [UsedImplicitly]
        private void Construct(
            MainFlowCoordinator mainFlowCoordinator,
            SettingsViewController settingsViewController,
            NotePreviewViewController notePreviewViewController,
            ExtraPanelViewController extraPanelViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
            _notePreviewViewController = notePreviewViewController;
            _extraPanelViewController = extraPanelViewController;
        }

#if V1_29_1
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
#else
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
#endif
        {
            SetTitle(nameof(NoteTweaks), ViewController.AnimationType.None);
            GameObject.Find("TitleViewController").GetComponent<RectTransform>().anchoredPosition = new Vector2(-137.5f, 0f);
            
            if (firstActivation)
            {
                showBackButton = true;
                ProvideInitialViewControllers(_notePreviewViewController, _settingsViewController, _extraPanelViewController);
            }

            if (NotePreviewViewController.NoteContainer == null && NotePreviewViewController.HasInitialized)
            {
                NotePreviewViewController.HasInitialized = false;
                NotePreviewViewController.NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
            }
            NotePreviewViewController.UpdateColors();
            NotePreviewViewController.UpdateOutlines();
#if V1_29_1
            NotePreviewViewController.NoteContainer?.SetActive(true); // god rider shut UP
#else
            NotePreviewViewController.NoteContainer.SetActive(true);
#endif
            
            _extraPanelViewController.UpdatePresetDropdown();
        }

        // ReSharper disable once ParameterHidesMember
#if V1_29_1
        public override void BackButtonWasPressed(ViewController topViewController)
#else
        protected override void BackButtonWasPressed(ViewController topViewController)
#endif
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
            Config.Changed();
            
            _ = NotePreviewViewController.CutoutFadeOut();
        }
    }
}