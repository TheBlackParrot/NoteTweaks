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
            Config.NoteScale = Vectors.Max(Vectors.Min(Config.NoteScale, 2.0f), 0.1f);
            Config.BombScale = Mathf.Max(Mathf.Min(Config.BombScale, 2.0f), 0.1f);
            Config.LinkScale = Mathf.Max(Mathf.Min(Config.LinkScale, 2.0f), 0.1f);
            
            Config.ColorBoostLeft = Mathf.Max(Config.ColorBoostLeft, -0.95f);
            Config.ColorBoostRight = Mathf.Max(Config.ColorBoostRight, -0.95f);
            Config.BombColorBoost = Mathf.Max(Config.BombColorBoost, -0.95f);
            
            Config.DotMeshSides = Math.Max(Math.Min(Config.DotMeshSides, 48), 4);
            
            Config.LeftFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(Config.LeftFaceColorNoteSkew, 1.0f));
            Config.RightFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(Config.RightFaceColorNoteSkew, 1.0f));
            Config.LeftFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(Config.LeftFaceGlowColorNoteSkew, 1.0f));
            Config.RightFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(Config.RightFaceGlowColorNoteSkew, 1.0f));
            
            Config.AccDotSize = Math.Max(Math.Min(Config.AccDotSize, 15), 5);

            Config.RainbowBombTimeScale = Mathf.Max(0.1f, Config.RainbowBombTimeScale);
            Config.RainbowBombSaturation = Mathf.Max(0f, Config.RainbowBombSaturation);
            Config.RainbowBombValue = Mathf.Max(0f, Config.RainbowBombValue);
            
            Config.LeftMinBrightness = Mathf.Max(Mathf.Min(Config.LeftMinBrightness, 1.0f), 0.0f);
            Config.LeftMaxBrightness = Mathf.Max(Mathf.Min(Config.LeftMaxBrightness, 1.0f), 0.0f);
            Config.RightMinBrightness = Mathf.Max(Mathf.Min(Config.RightMinBrightness, 1.0f), 0.0f);
            Config.RightMaxBrightness = Mathf.Max(Mathf.Min(Config.RightMaxBrightness, 1.0f), 0.0f);
            
            Config.ArrowPosition = Vectors.Max(Vectors.Min(Config.ArrowPosition, 0.2f), -0.2f);
            Config.DotPosition = Vectors.Max(Vectors.Min(Config.DotPosition, 0.2f), -0.2f);
            Config.LeftGlowOffset = Vectors.Max(Vectors.Min(Config.LeftGlowOffset, 0.2f), -0.2f);
            Config.RightGlowOffset = Vectors.Max(Vectors.Min(Config.RightGlowOffset, 0.2f), -0.2f);
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