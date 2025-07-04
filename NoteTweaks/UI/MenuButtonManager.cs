using System;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace NoteTweaks.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly SettingsFlowCoordinator _settingsFlowCoordinator;
        private readonly MenuButtons _menuButtons;
        private readonly MenuButton _menuButton;
        private static MenuButton _instance;
        
        private MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, SettingsFlowCoordinator settingsFlowCoordinator, MenuButtons menuButtons)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _settingsFlowCoordinator = settingsFlowCoordinator;
            _menuButtons = menuButtons;
            _menuButton = new MenuButton(nameof(NoteTweaks), null, HandleMenuButtonOnClick);
            _instance = _menuButton;
        }

        public void Initialize()
        {
            _menuButtons.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            _menuButtons.UnregisterButton(_menuButton);
        }

        private void HandleMenuButtonOnClick()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_settingsFlowCoordinator);
        }
        
        public static void ColorizeButtonOnUpdateAvailable()
        {
            _instance.Text = $"<color=#AAFFAA>{nameof(NoteTweaks)}";
        }
    }
}