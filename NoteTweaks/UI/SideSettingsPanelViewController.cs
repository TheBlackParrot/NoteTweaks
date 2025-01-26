using System;
using System.ComponentModel;
using BeatSaberMarkupLanguage.GameplaySetup;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using Zenject;

namespace NoteTweaks.UI
{
    [UsedImplicitly]
    internal class SideSettingsPanelViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static MainMenuViewController _mainMenu;
        private readonly PluginConfig _config = Plugin.Config;

        private SideSettingsPanelViewController(MainMenuViewController mainMenuViewController)
        {
            _mainMenu = mainMenuViewController;
        }

        public void Initialize()
        {
            GameplaySetup.Instance.AddTab("NoteTweaks", "NoteTweaks.UI.BSML.SideSettings.bsml", this);
            _mainMenu.didDeactivateEvent += MainMenu_didDeactivateEvent;
        }
        
        public void Dispose()
        {
            if (GameplaySetup.Instance != null)
            {
                GameplaySetup.Instance.RemoveTab("NoteTweaks");
            }
        }

        private void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfNoodle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FixDotsIfNoodle)));
        }

        private void MainMenu_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            Refresh();
        }

        protected bool Enabled
        {
            get => _config.Enabled;
            set => _config.Enabled = value;
        }
        
        protected bool DisableIfNoodle
        {
            get => _config.DisableIfNoodle;
            set => _config.DisableIfNoodle = value;
        }
        
        protected bool FixDotsIfNoodle
        {
            get => _config.FixDotsIfNoodle;
            set => _config.FixDotsIfNoodle = value;
        }
    }
}