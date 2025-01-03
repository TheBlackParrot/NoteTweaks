using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Newtonsoft.Json;
using NoteTweaks.Configuration;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.ExtraPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.ExtraPanel.bsml")]
    internal class ExtraPanelViewController : BSMLAutomaticViewController
    {
        private PluginConfig _config;

        public class ManifestData
        {
            public class LinksData
            {
                [JsonProperty("project-home")]
                public string ProjectHome { get; private set; }
            }
            
            [JsonProperty("gameVersion")]
            public string GameVersion { get; private set; }
            
            [JsonProperty("author")]
            public string Author { get; private set; }

            [JsonProperty("links")]
            public LinksData Links { get; private set; }
        }

        private static ManifestData Manifest;
        readonly string version = $"<size=80%><smallcaps><alpha=#CC>NoteTweaks</smallcaps></size> <alpha=#FF><b>v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}</b>";
        readonly string gameVersion;
        readonly string author;
        readonly string projectHome;

        public ExtraPanelViewController()
        {
            byte[] manifestData = SiraUtil.Extras.Utilities.GetResource(Assembly.GetExecutingAssembly(), "NoteTweaks.manifest.json");
            Manifest = JsonConvert.DeserializeObject<ManifestData>(System.Text.Encoding.UTF8.GetString(manifestData));

            gameVersion = $"<alpha=#CC>(<alpha=#77><size=80%>for</size> <b><alpha=#FF>{Manifest.GameVersion}<alpha=#CC></b>)";
            author = $"<alpha=#77><size=80%>developed by</size> <b><alpha=#FF>{Manifest.Author}</b>";
            projectHome = Manifest.Links.ProjectHome;
        }

        [UIAction("openProjectHome")]
        private void OpenProjectHomeURL()
        {
            Application.OpenURL(Manifest.Links.ProjectHome);
        }
        
        [Inject]
        private void Construct(PluginConfig config)
        {
            _config = config;
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
    }
}