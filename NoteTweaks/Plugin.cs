﻿using System;
using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using NoteTweaks.Configuration;
using NoteTweaks.Installers;
using NoteTweaks.Utils;
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
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config;
        private static Harmony _harmony;

        internal static void ClampSettings()
        {
            Config.NoteScale = Vectors.Max(Config.NoteScale, 0.1f);
            
            Config.LinkScale = Mathf.Max(Config.LinkScale, 0.1f);
            
            Config.ColorBoostLeft = Mathf.Max(Config.ColorBoostLeft, -0.95f);
            Config.ColorBoostRight = Mathf.Max(Config.ColorBoostRight, -0.95f);
            
            Config.DotMeshSides = Math.Max(Config.DotMeshSides, 4);
            
            Config.LeftFaceColorNoteSkew = Mathf.Max(0, Mathf.Min(Config.LeftFaceColorNoteSkew, 1.0f));
            Config.RightFaceColorNoteSkew = Mathf.Max(0, Mathf.Min(Config.RightFaceColorNoteSkew, 1.0f));
        }

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            Config = config.Generated<PluginConfig>();
            
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