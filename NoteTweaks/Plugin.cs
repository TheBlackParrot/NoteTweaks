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

            // P1b5e
            foreach (var preset in PluginConfig.Instance.Presets.Values)
            {
                preset.NoteScale = Vectors.Max(Vectors.Min(preset.NoteScale, 2.0f), 0.1f);
                preset.BombScale = Mathf.Max(Mathf.Min(preset.BombScale, 2.0f), 0.1f);
                preset.LinkScale = Mathf.Max(Mathf.Min(preset.LinkScale, 2.0f), 0.1f);
                
                preset.ColorBoostLeft = Mathf.Max(preset.ColorBoostLeft, -0.95f);
                preset.ColorBoostRight = Mathf.Max(preset.ColorBoostRight, -0.95f);
                preset.BombColorBoost = Mathf.Max(preset.BombColorBoost, -0.95f);
                
                preset.DotMeshSides = Math.Max(Math.Min(preset.DotMeshSides, 48), 4);
                
                preset.LeftFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.LeftFaceColorNoteSkew, 1.0f));
                preset.RightFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.RightFaceColorNoteSkew, 1.0f));
                preset.LeftFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.LeftFaceGlowColorNoteSkew, 1.0f));
                preset.RightFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.RightFaceGlowColorNoteSkew, 1.0f));
                
                preset.AccDotSize = Math.Max(Math.Min(preset.AccDotSize, 15), 5);

                preset.RainbowBombTimeScale = Mathf.Max(0.1f, preset.RainbowBombTimeScale);
                preset.RainbowBombSaturation = Mathf.Max(0f, preset.RainbowBombSaturation);
                preset.RainbowBombValue = Mathf.Max(0f, preset.RainbowBombValue);
                
                preset.LeftMinBrightness = Mathf.Max(Mathf.Min(preset.LeftMinBrightness, 1.0f), 0.0f);
                preset.LeftMaxBrightness = Mathf.Max(Mathf.Min(preset.LeftMaxBrightness, 1.0f), 0.0f);
                preset.RightMinBrightness = Mathf.Max(Mathf.Min(preset.RightMinBrightness, 1.0f), 0.0f);
                preset.RightMaxBrightness = Mathf.Max(Mathf.Min(preset.RightMaxBrightness, 1.0f), 0.0f);
                
                preset.ArrowPosition = Vectors.Max(Vectors.Min(preset.ArrowPosition, 0.2f), -0.2f);
                preset.DotPosition = Vectors.Max(Vectors.Min(preset.DotPosition, 0.2f), -0.2f);
                preset.LeftGlowOffset = Vectors.Max(Vectors.Min(preset.LeftGlowOffset, 0.2f), -0.2f);
                preset.RightGlowOffset = Vectors.Max(Vectors.Min(preset.RightGlowOffset, 0.2f), -0.2f);
                
                preset.NoteOutlineLeftColorSkew = Mathf.Max(0f, Mathf.Min(preset.NoteOutlineLeftColorSkew, 1.0f));
                preset.NoteOutlineRightColorSkew = Mathf.Max(0f, Mathf.Min(preset.NoteOutlineRightColorSkew, 1.0f));
                preset.NoteOutlineScale = Math.Max(preset.NoteOutlineScale, 0);
                preset.BombOutlineScale = Math.Max(preset.BombOutlineScale, 0);
                
                preset.FogScale = Mathf.Max(preset.FogScale, 0.0f);
                preset.FogHeightScale = Mathf.Max(preset.FogHeightScale, 0.0f);
            }
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

            // P73c1
            foreach (var preset in PluginConfig.Instance.Presets.Values)
            {
                preset.NoteScale = Vectors.Max(Vectors.Min(preset.NoteScale, 2.0f), 0.1f);
                preset.BombScale = Mathf.Max(Mathf.Min(preset.BombScale, 2.0f), 0.1f);
                preset.LinkScale = Mathf.Max(Mathf.Min(preset.LinkScale, 2.0f), 0.1f);
                
                preset.ColorBoostLeft = Mathf.Max(preset.ColorBoostLeft, -0.95f);
                preset.ColorBoostRight = Mathf.Max(preset.ColorBoostRight, -0.95f);
                preset.BombColorBoost = Mathf.Max(preset.BombColorBoost, -0.95f);
                
                preset.DotMeshSides = Math.Max(Math.Min(preset.DotMeshSides, 48), 4);
                
                preset.LeftFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.LeftFaceColorNoteSkew, 1.0f));
                preset.RightFaceColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.RightFaceColorNoteSkew, 1.0f));
                preset.LeftFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.LeftFaceGlowColorNoteSkew, 1.0f));
                preset.RightFaceGlowColorNoteSkew = Mathf.Max(0f, Mathf.Min(preset.RightFaceGlowColorNoteSkew, 1.0f));
                
                preset.AccDotSize = Math.Max(Math.Min(preset.AccDotSize, 15), 5);

                preset.RainbowBombTimeScale = Mathf.Max(0.1f, preset.RainbowBombTimeScale);
                preset.RainbowBombSaturation = Mathf.Max(0f, preset.RainbowBombSaturation);
                preset.RainbowBombValue = Mathf.Max(0f, preset.RainbowBombValue);
                
                preset.LeftMinBrightness = Mathf.Max(Mathf.Min(preset.LeftMinBrightness, 1.0f), 0.0f);
                preset.LeftMaxBrightness = Mathf.Max(Mathf.Min(preset.LeftMaxBrightness, 1.0f), 0.0f);
                preset.RightMinBrightness = Mathf.Max(Mathf.Min(preset.RightMinBrightness, 1.0f), 0.0f);
                preset.RightMaxBrightness = Mathf.Max(Mathf.Min(preset.RightMaxBrightness, 1.0f), 0.0f);
                
                preset.ArrowPosition = Vectors.Max(Vectors.Min(preset.ArrowPosition, 0.2f), -0.2f);
                preset.DotPosition = Vectors.Max(Vectors.Min(preset.DotPosition, 0.2f), -0.2f);
                preset.LeftGlowOffset = Vectors.Max(Vectors.Min(preset.LeftGlowOffset, 0.2f), -0.2f);
                preset.RightGlowOffset = Vectors.Max(Vectors.Min(preset.RightGlowOffset, 0.2f), -0.2f);
                
                preset.NoteOutlineLeftColorSkew = Mathf.Max(0f, Mathf.Min(preset.NoteOutlineLeftColorSkew, 1.0f));
                preset.NoteOutlineRightColorSkew = Mathf.Max(0f, Mathf.Min(preset.NoteOutlineRightColorSkew, 1.0f));
                preset.NoteOutlineScale = Math.Max(preset.NoteOutlineScale, 0);
                preset.BombOutlineScale = Math.Max(preset.BombOutlineScale, 0);
                
                preset.FogScale = Mathf.Max(preset.FogScale, 0.0f);
                preset.FogHeightScale = Mathf.Max(preset.FogHeightScale, 0.0f);
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}
