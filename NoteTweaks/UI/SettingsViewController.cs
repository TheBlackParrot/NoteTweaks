using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using UnityEngine;
using Zenject;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Settings.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Settings.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private static PluginConfig _config;
        public string PercentageFormatter(float x) => x.ToString("0%");
        public string PreciseFloatFormatter(float x) => x.ToString("F3");
        public string AccFormatter(int x) => (x + 100).ToString();
        public string DegreesFormatter(float x) => $"{x:0.#}\u00b0";

        internal static bool LoadTextures = false;

        [Inject]
        [UsedImplicitly]
        private void Construct(PluginConfig config)
        {
            _config = config;
        }
        
        [UIValue("EnableFaceGlow")]
        protected bool EnableFaceGlow
        {
            get => _config.EnableFaceGlow;
            set
            {
                _config.EnableFaceGlow = value;
                NotePreviewViewController.UpdateVisibility();

                NotifyPropertyChanged();
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
            set
            {
                _config.LinkScale = value;
                NotePreviewViewController.UpdateNoteScale();
            }
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

        protected float LeftGlowIntensity
        {
            get => _config.LeftGlowIntensity;
            set
            {
                _config.LeftGlowIntensity = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightGlowIntensity
        {
            get => _config.RightGlowIntensity;
            set
            {
                _config.RightGlowIntensity = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool EnableChainDots
        {
            get => _config.EnableChainDots;
            set
            {
                _config.EnableChainDots = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected float ChainDotScaleX
        {
            get => _config.ChainDotScale.x;
            set
            {
                Vector3 scale = _config.ChainDotScale;
                scale.x = value;
                _config.ChainDotScale = scale;
                
                NotePreviewViewController.UpdateDotScale();
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
                
                NotePreviewViewController.UpdateDotScale();
            }
        }
        
        protected bool EnableChainDotGlow
        {
            get => _config.EnableChainDotGlow;
            set
            {
                _config.EnableChainDotGlow = value;
                NotePreviewViewController.UpdateVisibility();
            }
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

        protected string NoteTexture
        {
            get => _config.NoteTexture;
            set
            {
                _config.NoteTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(value);
                }
            }
        }
        
        protected Color BombColor
        {
            get => _config.BombColor;
            set
            {
                _config.BombColor = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float BombColorBoost
        {
            get => _config.BombColorBoost;
            set
            {
                _config.BombColorBoost = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }
        
        protected string BombTexture
        {
            get => _config.BombTexture;
            set
            {
                _config.BombTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(value, true);
                }
            }
        }
        
        protected float BombScale
        {
            get => _config.BombScale;
            set
            {
                _config.BombScale = value;
                NotePreviewViewController.UpdateBombScale();
            }
        }
        
        protected bool InvertBombTexture
        {
            get => _config.InvertBombTexture;
            set
            {
                _config.InvertBombTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(Plugin.Config.BombTexture, true);
                }
            }
        }
        
        protected bool InvertNoteTexture
        {
            get => _config.InvertNoteTexture;
            set
            {
                _config.InvertNoteTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(Plugin.Config.NoteTexture);
                }
            }
        }
        
        [UIValue("EnableRainbowBombs")]
        protected bool EnableRainbowBombs
        {
            get => _config.EnableRainbowBombs;
            set
            {
                _config.EnableRainbowBombs = value;
                NotePreviewViewController.UpdateBombColors();

                NotifyPropertyChanged();
            }
        }
        
        protected float RainbowBombTimeScale
        {
            get => _config.RainbowBombTimeScale;
            set
            {
                _config.RainbowBombTimeScale = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float RainbowBombSaturation
        {
            get => _config.RainbowBombSaturation;
            set
            {
                _config.RainbowBombSaturation = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float RainbowBombValue
        {
            get => _config.RainbowBombValue;
            set
            {
                _config.RainbowBombValue = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected string GlowTexture
        {
            get => _config.GlowTexture;
            set
            {
                _config.GlowTexture = value;
                _ = GlowTextures.UpdateTextures();
            }
        }

        protected string ArrowMesh
        {
            get => _config.ArrowMesh;
            set
            {
                _config.ArrowMesh = value;
                NotePreviewViewController.UpdateArrowMeshes();
                _ = GlowTextures.UpdateTextures();
            }
        }

        protected float LeftMinBrightness
        {
            get => _config.LeftMinBrightness;
            set
            {
                _config.LeftMinBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float LeftMaxBrightness
        {
            get => _config.LeftMaxBrightness;
            set
            {
                _config.LeftMaxBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightMinBrightness
        {
            get => _config.RightMinBrightness;
            set
            {
                _config.RightMinBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightMaxBrightness
        {
            get => _config.RightMaxBrightness;
            set
            {
                _config.RightMaxBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
            }
        }
        
        [UIValue("glowTextureChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> GlowTextureChoices = new List<object> { "Glow", "GlowInterlaced", "Solid" };
        
        [UIValue("arrowMeshChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> ArrowMeshChoices = new List<object> { "Default", "Line", "Triangle" };

        [UIComponent("selectedNoteTexture")]
        public DropDownListSetting noteTextureDropDown;

        [UIValue("noteTextureChoices")]
        private List<object> NoteTextureChoices => LoadTextureChoices();
        
        [UIAction("#post-parse")]
        public void UpdateTextureList()
        {
            UpdateTextureChoices();
        }

        private void UpdateTextureChoices()
        {
            if (noteTextureDropDown == null)
            {
                return;
            }
            
            noteTextureDropDown.Values = NoteTextureChoices;
            noteTextureDropDown.UpdateChoices();
        }

        private List<object> LoadTextureChoices()
        {
            Plugin.Log.Info("Setting texture filenames for dropdown...");
            List<object> choices = new List<object>();

            if (!Directory.Exists(Textures.ImagePath))
            {
                Directory.CreateDirectory(Textures.ImagePath);
            }
            
            string[] dirs = Directory.GetDirectories(Textures.ImagePath);
            foreach (string dir in dirs)
            {
                if (Textures.IncludedCubemaps.Contains(Path.GetDirectoryName(dir)))
                {
                    Plugin.Log.Info($"{dir} shares a name with an included cubemap, skipping");
                    continue;
                }
                
                int count = 0;
                
                Textures.FaceNames.ForEach(pair =>
                {
                    foreach (string extension in Textures.FileExtensions)
                    {
                        string path = $"{dir}/{pair.Key}{extension}";
                        if (File.Exists(path))
                        {
                            count++;
                            break;
                        }
                    }
                });

                if (count == 6)
                {
                    choices.Add(dir.Split('\\').Last());
                }
            }
            
            string[] files = Directory.GetFiles(Textures.ImagePath);
            foreach (string file in files)
            {
                if (Textures.FileExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    if (Textures.IncludedCubemaps.Contains(Path.GetFileNameWithoutExtension(file)))
                    {
                        Plugin.Log.Info($"{file} shares a name with an included cubemap, skipping");
                        continue;
                    }
                    
                    choices.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            
            choices.AddRange(Textures.IncludedCubemaps);
            choices.Sort();
            choices = choices.Prepend("Default").ToList();

            Plugin.Log.Info("Set texture filenames");

            return choices;
        }
    }
}