using System;
using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using Newtonsoft.Json;
using NoteTweaks.Configuration;
using NoteTweaks.Installers;
using SiraUtil.Zenject;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace NoteTweaks
{
    [Plugin(RuntimeOptions.DynamicInit)]
    [NoEnableDisable]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Plugin
    {
        internal class ManifestData
        {
            [JsonProperty("gameVersion")]
            public string GameVersion { get; private set; }
        }
        
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config;
        private static Harmony _harmony;
        internal static ManifestData Manifest;

        internal static void ClampSettings()
        {
            Vector3 noteScale = Config.NoteScale;
            // surely there's a better way :clueless:
            noteScale.x = Mathf.Max(noteScale.x, 0.1f);
            noteScale.y = Mathf.Max(noteScale.y, 0.1f);
            noteScale.z = Mathf.Max(noteScale.z, 0.1f);
            Config.NoteScale = noteScale;
            
            Config.LinkScale = Mathf.Max(Config.LinkScale, 0.1f);
            
            Config.ColorBoostLeft = Mathf.Max(Config.ColorBoostLeft, -0.95f);
            Config.ColorBoostRight = Mathf.Max(Config.ColorBoostRight, -0.95f);
            
            Config.DotMeshSides = Math.Max(Config.DotMeshSides, 4);
            
            Config.FaceColorNoteSkew = Mathf.Max(0, Mathf.Min(Config.FaceColorNoteSkew, 1.0f));
        }

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            Config = config.Generated<PluginConfig>();
            
            byte[] manifestData = SiraUtil.Extras.Utilities.GetResource(Assembly.GetExecutingAssembly(), "NoteTweaks.manifest.json");
            Manifest = JsonConvert.DeserializeObject<ManifestData>(System.Text.Encoding.UTF8.GetString(manifestData));
            
            zenjector.Install<AppInstaller>(Location.App, Config);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.Player);
        }

        [OnEnable]
        public void OnEnable()
        {
            ClampSettings();
            
            _harmony = new Harmony("TheBlackParrot.NoteTweaks");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}