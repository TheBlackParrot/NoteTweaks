using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Config.Stores.Converters;
using IPA.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using TMPro;
using UnityEngine;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.ExtraPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.ExtraPanel.bsml")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ExtraPanelViewController : BSMLAutomaticViewController
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        private static readonly string ExportRoot = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Export");
        
        private readonly VersionManager VersionData = new VersionManager();
        
        [UsedImplicitly] private readonly string version;
        [UsedImplicitly] private readonly string originalGameVersion;
        [UsedImplicitly] private readonly string author;
        [UsedImplicitly] private readonly string projectHome;
        [UsedImplicitly] private string latestVersion => $"(<alpha=#CC>{VersionData.ModVersion.ToString(3)} <alpha=#88>-> <alpha=#CC>{VersionManager.LatestVersion?.ToString(3)}<alpha=#FF>)";
        
#if PREREL
        private const string isPreRelease = " <alpha=#77><size=80%>(Pre-release)";
#else
        private const string isPreRelease = "";
#endif
        
        [UsedImplicitly] private string gameVersion => $"{originalGameVersion}{isPreRelease}";
        
        // shut the hekc
        [UIComponent("gameVersionText")]
        #pragma warning disable CS0649
        private TextMeshProUGUI gameVersionText;
        #pragma warning restore CS0649

        public ExtraPanelViewController()
        {
            version = $"<size=80%><smallcaps><alpha=#CC>NoteTweaks</smallcaps></size> <alpha=#FF><b>v{VersionData.ModVersion.ToString(3)}</b>";
            originalGameVersion = $"<alpha=#CC>(<alpha=#77><size=80%>for</size> <b><alpha=#FF>{VersionData.GameVersion.ToString(3)}<alpha=#CC></b>)";
            author = $"<alpha=#77><size=80%>developed by</size> <b><alpha=#FF>{VersionData.Manifest.Author}</b>";
            projectHome = VersionData.Manifest.Links.ProjectHome;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            NotifyPropertyChanged(nameof(Enabled));
            NotifyPropertyChanged(nameof(DisableIfNoodle));
            NotifyPropertyChanged(nameof(DisableIfVivify));
            NotifyPropertyChanged(nameof(FixDotsIfNoodle));
            
            NotifyPropertyChanged(nameof(PresetNames));
            NotifyPropertyChanged(nameof(SelectedPreset));

            if (VersionManager.LatestVersion != null)
            {
                if (VersionData.ModVersion > VersionManager.LatestVersion)
                {
                    gameVersionText.text = gameVersion;
                }
            }
        }

        [UIAction("openProjectHome")]
        private void OpenProjectHomeURL()
        {
            Application.OpenURL(VersionData.Manifest.Links.ProjectHome);
        }

        [UIAction("openNewReleaseTag")]
        private void OpenNewReleaseTag()
        {
            Application.OpenURL($"https://github.com/TheBlackParrot/NoteTweaks/releases/tag/{VersionManager.LatestVersion.ToString(3)}");
        }
        
        [UIComponent("export-text")]
        #pragma warning disable CS0649
        private TextMeshProUGUI exportText;
        #pragma warning restore CS0649

        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        [UIAction("export-settings-setter")]
        private void ExportAsSettingsSetter()
        {
            if (!Directory.Exists(ExportRoot))
            {
                Directory.CreateDirectory(ExportRoot);
            }
            
            HexColorConverter converter = new HexColorConverter();
            char[] trimChars = "\"\\".ToCharArray();
            
            string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
            string filepath = Path.Combine(ExportRoot, filename);

            Dictionary<string, object> exports = new Dictionary<string, object>();
            
            PropertyInfo[] properties = typeof(PluginConfig).GetProperties(BindingFlags)
                .Where(x => x.CanWrite && x.CanRead).ToArray();
            
            foreach (PropertyInfo property in properties)
            {
                string propName = property.Name;
                Type propType = property.PropertyType;
                
                if (NoteTweaksSettableSettings.BlockedSettings.Contains(propName))
                {
                    continue;
                }

                // wtf, for some reason these need to be... down here? instead of in the .Where call? i'm so confused
                if (propType == typeof(Vector2) || propType == typeof(Vector3))
                {
                    continue;
                }
                
                object value = null;
                if (propType == typeof(float) || propType == typeof(int))
                {
                    object originalValue = property.GetValue(Config) as float?;
                    if (originalValue != null)
                    {
                        // good fucking lord dude
                        decimal impreciseDecimal = decimal.Round((decimal)(float)originalValue, 4);
                        decimal parsedDecimal = decimal.Parse(impreciseDecimal.ToString("G", CultureInfo.InvariantCulture), NumberStyles.Float);
                        value = parsedDecimal;
                        
                        int roundedIntValue = decimal.ToInt32(parsedDecimal);
                        if (parsedDecimal == roundedIntValue)
                        {
                            value = roundedIntValue;
                        }
                    }
                }
                if (propType == typeof(string))
                {
                    value = property.GetValue(Config).ToString();
                }
                if (propType == typeof(Color))
                {
                    // uh. is that last parameter even used?
                    value = converter.ToValue((Color)property.GetValue(Config), property).ToString();
                    foreach (char trimChar in trimChars)
                    {
                        value = ((string)value).Replace(trimChar.ToString(), string.Empty);
                    }
                }
                if (propType == typeof(bool))
                {
                    value = (bool)property.GetValue(Config);
                }

                if (value != null)
                {
                    exports.Add($"_{propName[0].ToString().ToLower() + propName.Substring(1)}", value);
                }
            }

            try
            {
                File.WriteAllText(filepath, JsonConvert.SerializeObject(exports, Formatting.Indented));
            }
            catch (Exception e)
            {
                exportText.text = $"<color=#FF7777>Error while exporting: \n<color=#FFCCCC>{e.GetType().Name}";
                return;
            }
            
            exportText.text = $"<color=#77FF77>Exported settings to: \n<color=#CCFFCC>{filepath}";
        }

        protected bool Enabled
        {
            get => Config.Enabled;
            set
            {
                Config.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        protected bool DisableIfNoodle
        {
            get => Config.DisableIfNoodle;
            set
            {
                Config.DisableIfNoodle = value;
                NotifyPropertyChanged();
            }
        }
        
        protected bool DisableIfVivify
        {
            get => Config.DisableIfVivify;
            set
            {
                Config.DisableIfVivify = value;
                NotifyPropertyChanged();
            }
        }

        protected bool FixDotsIfNoodle
        {
            get => Config.FixDotsIfNoodle;
            set
            {
                Config.FixDotsIfNoodle = value;
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

        [UIValue("PresetNames")]
        private List<object> PresetNames
        {
            get
            {
                string[] files = Directory.GetFiles(ConfigurationPresetManager.PresetPath, "*.json");
                Plugin.Log.Info($"Found {files.Length} presets");

                return files.Select(Path.GetFileNameWithoutExtension).Cast<object>().ToList();
            }
        }

        [UIValue("SelectedPreset")]
        private string SelectedPreset = "";

        [UIValue("presetNameField")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        private string PresetNameField = "Preset";

        [UIAction("SavePreset")]
        private void SavePreset()
        {
            NotifyPropertyChanged(nameof(PresetNameField));
            ConfigurationPresetManager.SavePreset(PresetNameField);
            
            NotifyPropertyChanged(nameof(PresetNames));
        }

        [UIAction("LoadPreset")]
        private void LoadPreset()
        {
            NotifyPropertyChanged(nameof(SelectedPreset));
            ConfigurationPresetManager.LoadPreset(SelectedPreset);
        }
    }
}
