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
#if !PRE_V1_37_1
        private static MenuButton _instance;
#endif

        private MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, SettingsFlowCoordinator settingsFlowCoordinator, MenuButtons menuButtons)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _settingsFlowCoordinator = settingsFlowCoordinator;
            _menuButtons = menuButtons;
            _menuButton = new MenuButton(nameof(NoteTweaks), null, HandleMenuButtonOnClick);
#if !PRE_V1_37_1
            _instance = _menuButton;
#endif
        }

        public void Initialize()
        {
#if PRE_V1_37_1
            MenuButtons.instance.RegisterButton(_menuButton);
#else
            _menuButtons.RegisterButton(_menuButton);
#endif
        }

        public void Dispose()
        {
            _menuButtons.UnregisterButton(_menuButton);
        }

        private void HandleMenuButtonOnClick()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_settingsFlowCoordinator);
        }

#if !PRE_V1_37_1
        public static void ColorizeButtonOnUpdateAvailable()
        {
            _instance.Text = $"<color=#AAFFAA>{nameof(NoteTweaks)}";
        }
#endif
    }
}