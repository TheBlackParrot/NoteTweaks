using System;
using System.ComponentModel;
using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
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

        private void ParentControllerActivated(bool firstActivation)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfNoodle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfVivify)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FixDotsIfNoodle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableAccDot)));
            
            if (VersionManager.LatestVersion != null && firstActivation)
            {
                Version modVersion = Assembly.GetExecutingAssembly().GetName().Version;
                
                UpdateIsAvailable = VersionManager.LatestVersion > modVersion;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdateIsAvailable)));
                
                LatestVersion = $"(<alpha=#CC>{modVersion.ToString(3)} <alpha=#88>-> <alpha=#CC>{VersionManager.LatestVersion?.ToString(3)}<alpha=#FF>)";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatestVersion)));
                
                // this... looks like a bad idea. but it works? how tf else do i refresh this? at least firstActivation only triggers it once. idk
                _gameplaySetup.RemoveTab("NoteTweaks");
                _gameplaySetup.AddTab("NoteTweaks", "NoteTweaks.UI.BSML.SideSettings.bsml", this);
            }
        }
        
        private void GameplaySetupViewController_DidActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            ParentControllerActivated(firstActivation);
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
        
        protected bool DisableIfVivify
        {
            get => _config.DisableIfVivify;
            set
            {
                _config.DisableIfVivify = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableIfVivify)));
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

        [UIValue("UpdateIsAvailable")]
        protected bool UpdateIsAvailable;
        
        [UIValue("LatestVersion")]
        protected string LatestVersion = "";
    }
}