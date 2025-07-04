using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
#if !V1_29_1
using System.Threading.Tasks;
#endif
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
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
        
#if DEBUG
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
            
            string[] dirs = filepath.Split(Path.DirectorySeparatorChar);
#if PRE_V1_39_1
            string safePath = String.Join(Path.DirectorySeparatorChar.ToString(), dirs.ToList().GetRange(dirs.Length - 4, 4).ToArray());
#else
            string safePath = String.Join(Path.DirectorySeparatorChar.ToString(), dirs.GetRange(dirs.Length - 4, 4).ToArray());
#endif

            try
            {
                File.WriteAllText(filepath, JsonConvert.SerializeObject(exports, Formatting.Indented));
                SaveStatusValue.text = $"<color=#FFFFFFCC>Exported settings to<color=#FFFFFFFF>\n{safePath}";
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
                SaveStatusValue.text = $"<color=#FFCCCCCC>Error while exporting settings<color=#FFCCCCFF>\n{e.GetType().Name}";
            }
            
            SaveStatusModal.Show(true, true);
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

        protected string NoteMesh
        {
            get => Config.NoteMesh;
            set
            {
                Config.NoteMesh = value;
                NotifyPropertyChanged();

                Meshes.UpdateCustomNoteMesh();
                
                string[] noteNames = { "L_Arrow", "R_Arrow", "L_Dot", "R_Dot" };
                foreach (string noteName in noteNames)
                {
                    string fullName = $"_NoteTweaks_PreviewNote_{noteName}";
                    GameObject previewNote = GameObject.Find(fullName);
                    
                    if (previewNote == null)
                    {
                        Plugin.Log.Warn($"{fullName} is null");
                        continue;
                    }
                    if (!previewNote.TryGetComponent(out MeshFilter meshFilter))
                    {
                        continue;
                    }
                    
                    meshFilter.sharedMesh = Meshes.CurrentNoteMesh;

                    Transform outlineTransform = previewNote.transform.Find("NoteOutline");
                    if (outlineTransform == null)
                    {
                        continue;
                    }
                    
                    if (outlineTransform.TryGetComponent(out MeshFilter outlineMeshFilter))
                    {
                        outlineMeshFilter.sharedMesh = Outlines.InvertedNoteMesh;
                    }
                }
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
        
        [UIComponent("PresetDropdown")]
        #pragma warning disable CS0649
        public DropDownListSetting PresetDropDown;
        #pragma warning restore CS0649

        internal void UpdatePresetDropdown()
        {
            if (PresetDropDown == null)
            {
                return;
            }

            PresetNames = GetPresetNames();
            
#if PRE_V1_39_1
            PresetDropDown.values = PresetNames;
#else
            PresetDropDown.Values = PresetNames;
#endif
            PresetDropDown.UpdateChoices();
        }

        private static List<object> GetPresetNames()
        {
            List<string> files = Directory.GetFiles(ConfigurationPresetManager.PresetPath, "*.json").ToList();
            files.Sort();
            Plugin.Log.Info($"Found {files.Count} presets");

            return files.Select(Path.GetFileNameWithoutExtension).Cast<object>().ToList();
        }

        [UIValue("PresetNames")]
        private List<object> PresetNames = GetPresetNames();

        [UIValue("SelectedPreset")]
        private string SelectedPreset = "Default";

        [UIValue("PresetNameField")]
        private string PresetNameField = "Preset";

        [UIComponent("MeshDropdown")]
#pragma warning disable CS0649
        public DropDownListSetting MeshDropDown;
#pragma warning restore CS0649
        
        internal void UpdateMeshDropdown()
        {
            if (MeshDropDown == null)
            {
                return;
            }

            MeshNames = GetMeshNames();
            MeshDropDown.Values = MeshNames;
            MeshDropDown.UpdateChoices();
        }
        
        private static List<object> GetMeshNames()
        {
            List<string> files = Directory.GetFiles(Meshes.MeshFolder, "*.obj").ToList();
            files.Sort();
            Plugin.Log.Info($"Found {files.Count} note meshes");
            
            files = files.Prepend("Default").ToList();
            return files.Select(Path.GetFileNameWithoutExtension).Cast<object>().ToList();
        }
        
        [UIValue("MeshNames")]
        private List<object> MeshNames = GetMeshNames();
        
        #pragma warning disable CS0649
        [UIComponent("SaveStatusModal")]
        private ModalView SaveStatusModal;
        [UIComponent("SaveStatusValue")]
        private TextMeshProUGUI SaveStatusValue;
        [UIComponent("LoadConfirmationText")]
        private TextMeshProUGUI LoadConfirmationText;
        [UIComponent(("PresetNameInput"))]
        private StringSetting PresetNameInput;
        #pragma warning restore CS0649

        [UIAction("SavePreset")]
        private void SavePreset()
        {
            NotifyPropertyChanged(nameof(PresetNameField));
            SaveStatusValue.text = ConfigurationPresetManager.SavePreset(PresetNameField);
            SaveStatusValue.lineSpacing = -17;
            
            UpdatePresetDropdown();
            PresetDropDown.Value = PresetNames.Find(x => (string)x == PresetNameField);
            SelectedPreset = PresetDropDown.Value.ToString();
            NotifyPropertyChanged(nameof(SelectedPreset));
            
            SaveStatusModal.Show(true, true);
        }

        [UIAction("ShowLoadConfirmation")]
        private void ShowLoadConfirmation()
        {
            NotifyPropertyChanged(nameof(SelectedPreset));
            LoadConfirmationText.lineSpacing = -17; // rider didn't like it in the constructor >:(
            LoadConfirmationText.text = $"Are you sure you want to load <color=#FFFFCC>{SelectedPreset}<color=#FFFFFF>?\n<color=#FFCCCC>This <color=#FF9999><b>WILL OVERWRITE</b> <color=#FFCCCC>your current settings!";
        }
        
        [UIAction("LoadPreset")]
#if V1_29_1
        private void LoadPreset()
#else
        private async Task LoadPreset()
#endif
        {
            NotifyPropertyChanged(nameof(SelectedPreset));
            
            PresetNameField = SelectedPreset;
            NotifyPropertyChanged(nameof(PresetNameField));
            PresetNameInput.Text = SelectedPreset;
            
#if V1_29_1
            ConfigurationPresetManager.LoadPreset(SelectedPreset);
#else
            await ConfigurationPresetManager.LoadPreset(SelectedPreset);
#endif
        }
    }
}