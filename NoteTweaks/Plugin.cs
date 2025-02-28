using System;
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
        private static Harmony _harmony;

        internal static void ClampSettings()
        {
            PluginConfig.Instance.NoteScale = Vectors.Max(Vectors.Min(PluginConfig.Instance.NoteScale, 2.0f), 0.1f);
            PluginConfig.Instance.BombScale = Mathf.Max(Mathf.Min(PluginConfig.Instance.BombScale, 2.0f), 0.1f);
            PluginConfig.Instance.LinkScale = Mathf.Max(Mathf.Min(PluginConfig.Instance.LinkScale, 2.0f), 0.1f);
            
            PluginConfig.Instance.ColorBoostLeft = Mathf.Max(PluginConfig.Instance.ColorBoostLeft, -0.95f);
            PluginConfig.Instance.ColorBoostRight = Mathf.Max(PluginConfig.Instance.ColorBoostRight, -0.95f);
            PluginConfig.Instance.BombColorBoost = Mathf.Max(PluginConfig.Instance.BombColorBoost, -0.95f);
            
            PluginConfig.Instance.DotMeshSides = Math.Max(Math.Min(PluginConfig.Instance.DotMeshSides, 48), 4);
            
            PluginConfig.Instance.LeftFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.LeftFaceColorNoteSkew, 1.0f));
            PluginConfig.Instance.RightFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.RightFaceColorNoteSkew, 1.0f));
            PluginConfig.Instance.LeftFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.LeftFaceGlowColorNoteSkew, 1.0f));
            PluginConfig.Instance.RightFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.RightFaceGlowColorNoteSkew, 1.0f));
            
            PluginConfig.Instance.AccDotSize = Math.Max(Math.Min(PluginConfig.Instance.AccDotSize, 15), 5);

            PluginConfig.Instance.RainbowBombTimeScale = Mathf.Max(0.1f, PluginConfig.Instance.RainbowBombTimeScale);
            PluginConfig.Instance.RainbowBombSaturation = Mathf.Max(0f, PluginConfig.Instance.RainbowBombSaturation);
            PluginConfig.Instance.RainbowBombValue = Mathf.Max(0f, PluginConfig.Instance.RainbowBombValue);
            
            PluginConfig.Instance.LeftMinBrightness = Mathf.Max(Mathf.Min(PluginConfig.Instance.LeftMinBrightness, 1.0f), 0.0f);
            PluginConfig.Instance.LeftMaxBrightness = Mathf.Max(Mathf.Min(PluginConfig.Instance.LeftMaxBrightness, 1.0f), 0.0f);
            PluginConfig.Instance.RightMinBrightness = Mathf.Max(Mathf.Min(PluginConfig.Instance.RightMinBrightness, 1.0f), 0.0f);
            PluginConfig.Instance.RightMaxBrightness = Mathf.Max(Mathf.Min(PluginConfig.Instance.RightMaxBrightness, 1.0f), 0.0f);
            
            PluginConfig.Instance.ArrowPosition = Vectors.Max(Vectors.Min(PluginConfig.Instance.ArrowPosition, 0.2f), -0.2f);
            PluginConfig.Instance.DotPosition = Vectors.Max(Vectors.Min(PluginConfig.Instance.DotPosition, 0.2f), -0.2f);
            PluginConfig.Instance.LeftGlowOffset = Vectors.Max(Vectors.Min(PluginConfig.Instance.LeftGlowOffset, 0.2f), -0.2f);
            PluginConfig.Instance.RightGlowOffset = Vectors.Max(Vectors.Min(PluginConfig.Instance.RightGlowOffset, 0.2f), -0.2f);
            
            PluginConfig.Instance.NoteOutlineLeftColorSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.NoteOutlineLeftColorSkew, 1.0f));
            PluginConfig.Instance.NoteOutlineRightColorSkew = Mathf.Max(0f, Mathf.Min(PluginConfig.Instance.NoteOutlineRightColorSkew, 1.0f));
            PluginConfig.Instance.NoteOutlineScale = Math.Max(PluginConfig.Instance.NoteOutlineScale, 0);
            PluginConfig.Instance.BombOutlineScale = Math.Max(PluginConfig.Instance.BombOutlineScale, 0);
            
            PluginConfig.Instance.FogScale = Mathf.Max(PluginConfig.Instance.FogScale, 0.0f);
            PluginConfig.Instance.FogHeightScale = Mathf.Max(PluginConfig.Instance.FogHeightScale, 0.0f);

            PluginConfig.Instance.BombMeshStacks = Math.Max(Math.Min(PluginConfig.Instance.BombMeshStacks, 48), 1);
            PluginConfig.Instance.BombMeshSlices = Math.Max(Math.Min(PluginConfig.Instance.BombMeshSlices, 48), 1);
        }

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            PluginConfig c = config.Generated<PluginConfig>();
            PluginConfig.Instance = c;
            
            zenjector.Install<AppInstaller>(Location.App, PluginConfig.Instance);
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