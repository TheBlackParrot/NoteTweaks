using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    internal class SettingsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private SettingsViewController _settingsViewController;
        private NotePreviewViewController _notePreviewViewController;
        private ExtraPanelViewController _extraPanelViewController;
        
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle(nameof(NoteTweaks));
                showBackButton = true;
                ProvideInitialViewControllers(_settingsViewController, _notePreviewViewController, _extraPanelViewController);
            }

            if (NotePreviewViewController.NoteContainer == null && NotePreviewViewController.HasInitialized)
            {
                NotePreviewViewController.HasInitialized = false;
                NotePreviewViewController.NoteContainer = new GameObject("_NoteTweaks_NoteContainer");
            }
            NotePreviewViewController.UpdateColors();
            NotePreviewViewController.NoteContainer.SetActive(true);
        }

        // ReSharper disable once ParameterHidesMember
        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
            
            NotePreviewViewController.CutoutFadeOut();
        }
    }
}