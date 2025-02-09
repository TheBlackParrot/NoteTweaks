using System;
using System.ComponentModel;
using BeatSaberMarkupLanguage.GameplaySetup;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using Zenject;

namespace NoteTweaks.UI
{
    [UsedImplicitly]
    internal class ModSettingsViewController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private static PluginConfig _config;
        private readonly GameplaySetup _gameplaySetup;
        private readonly GameplaySetupViewController _gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        
        private ModSettingsViewController(PluginConfig config, GameplaySetup gameplaySetup, GameplaySetupViewController gameplaySetupViewController)
        {
            _config = config;
            _gameplaySetup = gameplaySetup;
            _gameplaySetupViewController = gameplaySetupViewController;
        }
        
        public void Initialize()
        {
            _gameplaySetup.AddTab("NoteTweaks", "NoteTweaks.UI.BSML.SideSettings.bsml", this);
            _gameplaySetupViewController.didActivateEvent += GameplaySetupViewController_DidActivateEvent;
        }

        public void Dispose()
        {
            _gameplaySetup.RemoveTab("NoteTweaks");
            _gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_DidActivateEvent;
        }

        private void ParentControllerActivated()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfNoodle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FixDotsIfNoodle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableAccDot)));
        }
        
        private void GameplaySetupViewController_DidActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ParentControllerActivated();
        }

        protected bool Enabled
        {
            get => _config.Enabled;
            set
            {
                _config.Enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }

        protected bool DisableIfNoodle
        {
            get => _config.DisableIfNoodle;
            set
            {
                _config.DisableIfNoodle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfNoodle)));
            }
        }

        protected bool FixDotsIfNoodle
        {
            get => _config.FixDotsIfNoodle;
            set
            {
                _config.FixDotsIfNoodle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FixDotsIfNoodle)));
            }
        }
        
        protected bool EnableAccDot
        {
            get => _config.EnableAccDot;
            set
            {
                _config.EnableAccDot = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableAccDot)));
            }
        }
    }
}