﻿using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using NoteTweaks.Configuration;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Settings.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Settings.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private PluginConfig _config;
        public string PercentageFormatter(float x) => x.ToString("0%");
        public string PreciseFloatFormatter(float x) => x.ToString("F3");
        public string AccFormatter(int x) => (x + 100).ToString();
        
        readonly string version = $"<smallcaps><alpha=#CC>NoteTweaks</smallcaps>  <alpha=#FF><b>v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}</b>";

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
        
        protected bool EnableFaceGlow
        {
            get => _config.EnableFaceGlow;
            set => _config.EnableFaceGlow = value;
        }

        protected float ArrowScaleX
        {
            get => _config.ArrowScale.x;
            set
            {
                Vector2 scale = _config.ArrowScale;
                scale.x = value;
                _config.ArrowScale = scale;
            }
        }
        
        protected float ArrowScaleY
        {
            get => _config.ArrowScale.y;
            set
            {
                Vector2 scale = _config.ArrowScale;
                scale.y = value;
                _config.ArrowScale = scale;
            }
        }
        
        protected float ArrowOffsetX
        {
            get => _config.ArrowPosition.x;
            set
            {
                Vector3 position = _config.ArrowPosition;
                position.x = value;
                _config.ArrowPosition = position;
            }
        }
        
        protected float ArrowOffsetY
        {
            get => _config.ArrowPosition.y;
            set
            {
                Vector3 position = _config.ArrowPosition;
                position.y = value;
                _config.ArrowPosition = position;
            }
        }
        
        protected float DotScaleX
        {
            get => _config.DotScale.x;
            set
            {
                Vector2 scale = _config.DotScale;
                scale.x = value;
                _config.DotScale = scale;
            }
        }
        
        protected float DotScaleY
        {
            get => _config.DotScale.y;
            set
            {
                Vector2 scale = _config.DotScale;
                scale.y = value;
                _config.DotScale = scale;
            }
        }
        
        protected float DotOffsetX
        {
            get => _config.DotPosition.x;
            set
            {
                Vector3 position = _config.DotPosition;
                position.x = value;
                _config.DotPosition = position;
            }
        }
        
        protected float DotOffsetY
        {
            get => _config.DotPosition.y;
            set
            {
                Vector3 position = _config.DotPosition;
                position.y = value;
                _config.DotPosition = position;
            }
        }

        protected bool EnableDots
        {
            get => _config.EnableDots;
            set => _config.EnableDots = value;
        }
        
        protected float NoteScaleX
        {
            get => _config.NoteScale.x;
            set
            {
                Vector3 scale = _config.NoteScale;
                scale.x = value;
                _config.NoteScale = scale;
            }
        }
        
        protected float NoteScaleY
        {
            get => _config.NoteScale.y;
            set
            {
                Vector3 scale = _config.NoteScale;
                scale.y = value;
                _config.NoteScale = scale;
            }
        }
        
        protected float NoteScaleZ
        {
            get => _config.NoteScale.z;
            set
            {
                Vector3 scale = _config.NoteScale;
                scale.z = value;
                _config.NoteScale = scale;
            }
        }
        
        protected float LinkScale
        {
            get => _config.LinkScale;
            set => _config.LinkScale = value;
        }
        
        protected float ColorBoostLeft
        {
            get => _config.ColorBoostLeft;
            set => _config.ColorBoostLeft = value;
        }
        
        protected float ColorBoostRight
        {
            get => _config.ColorBoostRight;
            set => _config.ColorBoostRight = value;
        }
        
        protected float GlowScale
        {
            get => _config.GlowScale;
            set => _config.GlowScale = value;
        }
        
        protected float GlowIntensity
        {
            get => _config.GlowIntensity;
            set => _config.GlowIntensity = value;
        }

        protected bool EnableChainDots
        {
            get => _config.EnableChainDots;
            set => _config.EnableChainDots = value;
        }
        protected float ChainDotScaleX
        {
            get => _config.ChainDotScale.x;
            set
            {
                Vector3 scale = _config.ChainDotScale;
                scale.x = value;
                _config.ChainDotScale = scale;
            }
        }
        
        protected float ChainDotScaleY
        {
            get => _config.ChainDotScale.y;
            set
            {
                Vector3 scale = _config.ChainDotScale;
                scale.y = value;
                _config.ChainDotScale = scale;
            }
        }
        
        protected bool EnableChainDotGlow
        {
            get => _config.EnableChainDotGlow;
            set => _config.EnableChainDotGlow = value;
        }

        protected Color FaceColor
        {
            get => _config.FaceColor;
            set => _config.FaceColor = value;
        }
        
        protected bool EnableAccDot
        {
            get => _config.EnableAccDot;
            set => _config.EnableAccDot = value;
        }

        protected int AccDotSize
        {
            get => _config.AccDotSize;
            set => _config.AccDotSize = value;
        }

        protected Color AccDotColor
        {
            get => _config.AccDotColor;
            set => _config.AccDotColor = value;
        }

        protected bool RenderAccDotsAboveSymbols
        {
            get => _config.RenderAccDotsAboveSymbols;
            set => _config.RenderAccDotsAboveSymbols = value;
        }
    }
}