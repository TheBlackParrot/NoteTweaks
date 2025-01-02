using System.Reflection;
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
        public string DegreesFormatter(float x) => $"{x:0.#}\u00b0";
        
        readonly string version = $"<size=80%><smallcaps><alpha=#CC>NoteTweaks</smallcaps></size> <alpha=#FF><b>v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}</b>";
        readonly string gameVersion = $"<alpha=#CC>(<alpha=#77><size=80%>for</size> <b><alpha=#FF>{Plugin.Manifest.GameVersion}<alpha=#CC></b>)";

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
            set
            {
                _config.EnableFaceGlow = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected float ArrowScaleX
        {
            get => _config.ArrowScale.x;
            set
            {
                Vector2 scale = _config.ArrowScale;
                scale.x = value;
                _config.ArrowScale = scale;
                
                NotePreviewViewController.UpdateArrowScale();
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
                
                NotePreviewViewController.UpdateArrowScale();
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
                
                NotePreviewViewController.UpdateArrowPosition();
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
                
                NotePreviewViewController.UpdateArrowPosition();
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
                
                NotePreviewViewController.UpdateDotScale();
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
                
                NotePreviewViewController.UpdateDotScale();
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
                
                NotePreviewViewController.UpdateDotPosition();
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
                
                NotePreviewViewController.UpdateDotPosition();
            }
        }

        protected bool EnableDots
        {
            get => _config.EnableDots;
            set
            {
                _config.EnableDots = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected float NoteScaleX
        {
            get => _config.NoteScale.x;
            set
            {
                Vector3 scale = _config.NoteScale;
                scale.x = value;
                _config.NoteScale = scale;
                
                NotePreviewViewController.UpdateNoteScale();
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
                
                NotePreviewViewController.UpdateNoteScale();
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
                
                NotePreviewViewController.UpdateNoteScale();
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
            set
            {
                _config.ColorBoostLeft = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float ColorBoostRight
        {
            get => _config.ColorBoostRight;
            set
            {
                _config.ColorBoostRight = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float ArrowGlowScale
        {
            get => _config.ArrowGlowScale;
            set
            {
                _config.ArrowGlowScale = value;
                NotePreviewViewController.UpdateArrowScale();
            }
        }
        
        protected float DotGlowScale
        {
            get => _config.DotGlowScale;
            set
            {
                _config.DotGlowScale = value;
                NotePreviewViewController.UpdateDotScale();
            }
        }

        protected float GlowIntensity
        {
            get => _config.GlowIntensity;
            set
            {
                _config.GlowIntensity = value;
                NotePreviewViewController.UpdateColors();
            }
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

        protected Color LeftFaceColor
        {
            get => _config.LeftFaceColor;
            set
            {
                _config.LeftFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected Color RightFaceColor
        {
            get => _config.RightFaceColor;
            set
            {
                _config.RightFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
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

        protected int DotMeshSides
        {
            get => _config.DotMeshSides;
            set
            {
                _config.DotMeshSides = value;
                NotePreviewViewController.UpdateDotMesh();
            }
        }

        protected float LeftFaceColorNoteSkew
        {
            get => _config.LeftFaceColorNoteSkew;
            set
            {
                _config.LeftFaceColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightFaceColorNoteSkew
        {
            get => _config.RightFaceColorNoteSkew;
            set
            {
                _config.RightFaceColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool DisableIfNoodle
        {
            get => _config.DisableIfNoodle;
            set => _config.DisableIfNoodle = value;
        }

        protected float RotateDot
        {
            get => _config.RotateDot;
            set
            {
                _config.RotateDot = value;
                NotePreviewViewController.UpdateDotRotation();
            }
        }
        
        protected bool NormalizeLeftFaceColor
        {
            get => _config.NormalizeLeftFaceColor;
            set
            {
                _config.NormalizeLeftFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected bool NormalizeRightFaceColor
        {
            get => _config.NormalizeRightFaceColor;
            set
            {
                _config.NormalizeRightFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected Color LeftFaceGlowColor
        {
            get => _config.LeftFaceGlowColor;
            set
            {
                _config.LeftFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float LeftFaceGlowColorNoteSkew
        {
            get => _config.LeftFaceGlowColorNoteSkew;
            set
            {
                _config.LeftFaceGlowColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeLeftFaceGlowColor
        {
            get => _config.NormalizeLeftFaceGlowColor;
            set
            {
                _config.NormalizeLeftFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected Color RightFaceGlowColor
        {
            get => _config.RightFaceGlowColor;
            set
            {
                _config.RightFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float RightFaceGlowColorNoteSkew
        {
            get => _config.RightFaceGlowColorNoteSkew;
            set
            {
                _config.RightFaceGlowColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeRightFaceGlowColor
        {
            get => _config.NormalizeRightFaceGlowColor;
            set
            {
                _config.NormalizeRightFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
    }
}