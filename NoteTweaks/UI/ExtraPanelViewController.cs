using System.Diagnostics.CodeAnalysis;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.ExtraPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.ExtraPanel.bsml")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ExtraPanelViewController : BSMLAutomaticViewController
    {
        private PluginConfig _config;
        
        private readonly VersionManager VersionData = new VersionManager();
        
        [UsedImplicitly] private readonly string version;
        [UsedImplicitly] private readonly string gameVersion;
        [UsedImplicitly] private readonly string author;
        [UsedImplicitly] private readonly string projectHome;
        [UsedImplicitly] private string latestVersion => $"(<alpha=#CC>{VersionManager.LatestVersion?.ToString(3)}<alpha=#FF>)";

        public ExtraPanelViewController()
        {
            version = $"<size=80%><smallcaps><alpha=#CC>NoteTweaks</smallcaps></size> <alpha=#FF><b>v{VersionData.ModVersion.ToString(3)}</b>";
            gameVersion = $"<alpha=#CC>(<alpha=#77><size=80%>for</size> <b><alpha=#FF>{VersionData.GameVersion.ToString(3)}<alpha=#CC></b>)";
            author = $"<alpha=#77><size=80%>developed by</size> <b><alpha=#FF>{VersionData.Manifest.Author}</b>";
            projectHome = VersionData.Manifest.Links.ProjectHome;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            NotifyPropertyChanged(nameof(Enabled));
            NotifyPropertyChanged(nameof(DisableIfNoodle));
            NotifyPropertyChanged(nameof(FixDotsIfNoodle));
        }

        [UIAction("openProjectHome")]
        private void OpenProjectHomeURL()
        {
            Application.OpenURL(VersionData.Manifest.Links.ProjectHome);
        }
        
        [Inject]
        private void Construct(PluginConfig config)
        {
            _config = config;
        }

        protected bool Enabled
        {
            get => _config.Enabled;
            set
            {
                _config.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        protected bool DisableIfNoodle
        {
            get => _config.DisableIfNoodle;
            set
            {
                _config.DisableIfNoodle = value;
                NotifyPropertyChanged();
            }
        }

        protected bool FixDotsIfNoodle
        {
            get => _config.FixDotsIfNoodle;
            set
            {
                _config.FixDotsIfNoodle = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("UpdateIsAvailable")]
        internal bool UpdateIsAvailable
        {
            get
            {
                if (VersionManager.LatestVersion == null)
                {
                    return false;
                }

                return VersionManager.LatestVersion > VersionData.ModVersion;
            }
        }
    }
}