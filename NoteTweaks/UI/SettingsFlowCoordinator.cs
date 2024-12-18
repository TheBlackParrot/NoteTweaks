﻿using HMUI;
using Zenject;

namespace NoteTweaks.UI
{
    internal class SettingsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private SettingsViewController _settingsViewController;

        [Inject]
        private void Construct(MainFlowCoordinator mainFlowCoordinator, SettingsViewController settingsViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle(nameof(NoteTweaks));
                showBackButton = true;
                ProvideInitialViewControllers(_settingsViewController);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}