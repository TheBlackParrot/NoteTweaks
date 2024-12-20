﻿using BeatSaberMarkupLanguage.Attributes;
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
            get => _config.EnableArrowGlow;
            set => _config.EnableArrowGlow = value;
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
        
        protected float NoteScale
        {
            get => _config.NoteScale;
            set => _config.NoteScale = value;
        }
        
        protected float LinkScale
        {
            get => _config.LinkScale;
            set => _config.LinkScale = value;
        }
    }
}