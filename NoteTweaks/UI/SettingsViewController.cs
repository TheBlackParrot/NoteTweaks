using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Managers;
using UnityEngine;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Settings.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Settings.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private static PluginConfig Config => PluginConfig.Instance;
        public string PercentageFormatter(float x) => x.ToString("0%");
        public string PreciseFloatFormatter(float x) => x.ToString("F3");
        public string AccFormatter(int x) => (x + 100).ToString();
        public string DegreesFormatter(float x) => $"{x:0.#}\u00b0";
        public string FakeIntFormatter(float x) => x.ToString("N0");

        internal static bool LoadTextures = false;
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            NotifyPropertyChanged(nameof(EnableAccDot));
        }
        
        [UIValue("EnableFaceGlow")]
        protected bool EnableFaceGlow
        {
            get => Config.EnableFaceGlow;
            set
            {
                Config.EnableFaceGlow = value;
                NotePreviewViewController.UpdateVisibility();

                NotifyPropertyChanged();
            }
        }

        protected float ArrowScaleX
        {
            get => Config.ArrowScale.x;
            set
            {
                Vector2 scale = Config.ArrowScale;
                scale.x = value;
                Config.ArrowScale = scale;
                
                NotePreviewViewController.UpdateArrowScale();
            }
        }
        
        protected float ArrowScaleY
        {
            get => Config.ArrowScale.y;
            set
            {
                Vector2 scale = Config.ArrowScale;
                scale.y = value;
                Config.ArrowScale = scale;
                
                NotePreviewViewController.UpdateArrowScale();
            }
        }
        
        protected float ArrowOffsetX
        {
            get => Config.ArrowPosition.x;
            set
            {
                Vector3 position = Config.ArrowPosition;
                position.x = value;
                Config.ArrowPosition = position;
                
                NotePreviewViewController.UpdateArrowPosition();
            }
        }
        
        protected float ArrowOffsetY
        {
            get => Config.ArrowPosition.y;
            set
            {
                Vector3 position = Config.ArrowPosition;
                position.y = value;
                Config.ArrowPosition = position;
                
                NotePreviewViewController.UpdateArrowPosition();
            }
        }
        
        protected float DotScaleX
        {
            get => Config.DotScale.x;
            set
            {
                Vector2 scale = Config.DotScale;
                scale.x = value;
                Config.DotScale = scale;
                
                NotePreviewViewController.UpdateDotScale();
            }
        }
        
        protected float DotScaleY
        {
            get => Config.DotScale.y;
            set
            {
                Vector2 scale = Config.DotScale;
                scale.y = value;
                Config.DotScale = scale;
                
                NotePreviewViewController.UpdateDotScale();
            }
        }
        
        protected float DotOffsetX
        {
            get => Config.DotPosition.x;
            set
            {
                Vector3 position = Config.DotPosition;
                position.x = value;
                Config.DotPosition = position;
                
                NotePreviewViewController.UpdateDotPosition();
            }
        }
        
        protected float DotOffsetY
        {
            get => Config.DotPosition.y;
            set
            {
                Vector3 position = Config.DotPosition;
                position.y = value;
                Config.DotPosition = position;
                
                NotePreviewViewController.UpdateDotPosition();
            }
        }

        protected bool EnableDots
        {
            get => Config.EnableDots;
            set
            {
                Config.EnableDots = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected float NoteScaleX
        {
            get => Config.NoteScale.x;
            set
            {
                Vector3 scale = Config.NoteScale;
                scale.x = value;
                Config.NoteScale = scale;
                
                NotePreviewViewController.UpdateNoteScale();
            }
        }
        
        protected float NoteScaleY
        {
            get => Config.NoteScale.y;
            set
            {
                Vector3 scale = Config.NoteScale;
                scale.y = value;
                Config.NoteScale = scale;
                
                NotePreviewViewController.UpdateNoteScale();
            }
        }
        
        protected float NoteScaleZ
        {
            get => Config.NoteScale.z;
            set
            {
                Vector3 scale = Config.NoteScale;
                scale.z = value;
                Config.NoteScale = scale;
                
                NotePreviewViewController.UpdateNoteScale();
            }
        }
        
        protected float LinkScale
        {
            get => Config.LinkScale;
            set
            {
                Config.LinkScale = value;
                NotePreviewViewController.UpdateNoteScale();
            }
        }

        protected float ColorBoostLeft
        {
            get => Config.ColorBoostLeft;
            set
            {
                Config.ColorBoostLeft = value;
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected float ColorBoostRight
        {
            get => Config.ColorBoostRight;
            set
            {
                Config.ColorBoostRight = value;
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected float ArrowGlowScale
        {
            get => Config.ArrowGlowScale;
            set
            {
                Config.ArrowGlowScale = value;
                NotePreviewViewController.UpdateArrowScale();
            }
        }
        
        protected float DotGlowScale
        {
            get => Config.DotGlowScale;
            set
            {
                Config.DotGlowScale = value;
                NotePreviewViewController.UpdateDotScale();
            }
        }

        protected float LeftGlowIntensity
        {
            get => Config.LeftGlowIntensity;
            set
            {
                Config.LeftGlowIntensity = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightGlowIntensity
        {
            get => Config.RightGlowIntensity;
            set
            {
                Config.RightGlowIntensity = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool EnableChainDots
        {
            get => Config.EnableChainDots;
            set
            {
                Config.EnableChainDots = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected float ChainDotScaleX
        {
            get => Config.ChainDotScale.x;
            set
            {
                Vector3 scale = Config.ChainDotScale;
                scale.x = value;
                Config.ChainDotScale = scale;
                
                NotePreviewViewController.UpdateDotScale();
            }
        }
        
        protected float ChainDotScaleY
        {
            get => Config.ChainDotScale.y;
            set
            {
                Vector3 scale = Config.ChainDotScale;
                scale.y = value;
                Config.ChainDotScale = scale;
                
                NotePreviewViewController.UpdateDotScale();
            }
        }
        
        protected bool EnableChainDotGlow
        {
            get => Config.EnableChainDotGlow;
            set
            {
                Config.EnableChainDotGlow = value;
                NotePreviewViewController.UpdateVisibility();
            }
        }

        protected Color LeftFaceColor
        {
            get => Config.LeftFaceColor;
            set
            {
                Config.LeftFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected Color RightFaceColor
        {
            get => Config.RightFaceColor;
            set
            {
                Config.RightFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool EnableAccDot
        {
            get => Config.EnableAccDot;
            set
            {
                Config.EnableAccDot = value;
                NotePreviewViewController.UpdateAccDots();
                NotifyPropertyChanged();
            }
        }

        protected int AccDotSize
        {
            get => Config.AccDotSize;
            set
            {
                Config.AccDotSize = value;
                NotePreviewViewController.UpdateAccDots();
            }
        }

        protected Color LeftAccDotColor
        {
            get => Config.LeftAccDotColor;
            set
            {
                Config.LeftAccDotColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected Color RightAccDotColor
        {
            get => Config.RightAccDotColor;
            set
            {
                Config.RightAccDotColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float LeftAccDotColorNoteSkew
        {
            get => Config.LeftAccDotColorNoteSkew;
            set
            {
                Config.LeftAccDotColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float RightAccDotColorNoteSkew
        {
            get => Config.RightAccDotColorNoteSkew;
            set
            {
                Config.RightAccDotColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeLeftAccDotColor
        {
            get => Config.NormalizeLeftAccDotColor;
            set
            {
                Config.NormalizeLeftAccDotColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeRightAccDotColor
        {
            get => Config.NormalizeRightAccDotColor;
            set
            {
                Config.NormalizeRightAccDotColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool RenderAccDotsAboveSymbols
        {
            get => Config.RenderAccDotsAboveSymbols;
            set
            {
                Config.RenderAccDotsAboveSymbols = value;
                
                try
                {
                    Materials.UpdateRenderQueues();
                }
                catch
                {
                    // i... ok??
                }

                NotePreviewViewController.UpdateAccDots();
            }
        }

        protected int DotMeshSides
        {
            get => Config.DotMeshSides;
            set
            {
                Config.DotMeshSides = value;
                NotePreviewViewController.UpdateDotMesh();
            }
        }

        protected float LeftFaceColorNoteSkew
        {
            get => Config.LeftFaceColorNoteSkew;
            set
            {
                Config.LeftFaceColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float RightFaceColorNoteSkew
        {
            get => Config.RightFaceColorNoteSkew;
            set
            {
                Config.RightFaceColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float RotateDot
        {
            get => Config.RotateDot;
            set
            {
                Config.RotateDot = value;
                NotePreviewViewController.UpdateDotRotation();
            }
        }
        
        protected bool NormalizeLeftFaceColor
        {
            get => Config.NormalizeLeftFaceColor;
            set
            {
                Config.NormalizeLeftFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected bool NormalizeRightFaceColor
        {
            get => Config.NormalizeRightFaceColor;
            set
            {
                Config.NormalizeRightFaceColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected Color LeftFaceGlowColor
        {
            get => Config.LeftFaceGlowColor;
            set
            {
                Config.LeftFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float LeftFaceGlowColorNoteSkew
        {
            get => Config.LeftFaceGlowColorNoteSkew;
            set
            {
                Config.LeftFaceGlowColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeLeftFaceGlowColor
        {
            get => Config.NormalizeLeftFaceGlowColor;
            set
            {
                Config.NormalizeLeftFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected Color RightFaceGlowColor
        {
            get => Config.RightFaceGlowColor;
            set
            {
                Config.RightFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected float RightFaceGlowColorNoteSkew
        {
            get => Config.RightFaceGlowColorNoteSkew;
            set
            {
                Config.RightFaceGlowColorNoteSkew = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected bool NormalizeRightFaceGlowColor
        {
            get => Config.NormalizeRightFaceGlowColor;
            set
            {
                Config.NormalizeRightFaceGlowColor = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected string NoteTexture
        {
            get => Config.NoteTexture;
            set
            {
                Config.NoteTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(value, false, true);
                }
            }
        }
        
        protected Color BombColor
        {
            get => Config.BombColor;
            set
            {
                Config.BombColor = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float BombColorBoost
        {
            get => Config.BombColorBoost;
            set
            {
                Config.BombColorBoost = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }
        
        protected string BombTexture
        {
            get => Config.BombTexture;
            set
            {
                Config.BombTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(value, true, true);
                }
            }
        }
        
        protected float BombScale
        {
            get => Config.BombScale;
            set
            {
                Config.BombScale = value;
                NotePreviewViewController.UpdateBombScale();
            }
        }
        
        protected bool InvertBombTexture
        {
            get => Config.InvertBombTexture;
            set
            {
                Config.InvertBombTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(Config.BombTexture, true, true);
                }
            }
        }
        
        protected bool InvertNoteTexture
        {
            get => Config.InvertNoteTexture;
            set
            {
                Config.InvertNoteTexture = value;
                if (LoadTextures)
                {
                    _ = Textures.LoadNoteTexture(Config.NoteTexture, false, true);
                }
            }
        }
        
        [UIValue("EnableRainbowBombs")]
        protected bool EnableRainbowBombs
        {
            get => Config.EnableRainbowBombs;
            set
            {
                Config.EnableRainbowBombs = value;
                NotePreviewViewController.UpdateBombColors();

                NotifyPropertyChanged();
            }
        }
        
        protected float RainbowBombTimeScale
        {
            get => Config.RainbowBombTimeScale;
            set
            {
                Config.RainbowBombTimeScale = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float RainbowBombSaturation
        {
            get => Config.RainbowBombSaturation;
            set
            {
                Config.RainbowBombSaturation = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected float RainbowBombValue
        {
            get => Config.RainbowBombValue;
            set
            {
                Config.RainbowBombValue = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }

        protected string GlowTexture
        {
            get => Config.GlowTexture;
            set
            {
                Config.GlowTexture = value;
                _ = ForceAsyncUpdateForGlowTexture();
            }
        }
        
        private static async Task ForceAsyncUpdateForGlowTexture()
        {
            await GlowTextures.UpdateTextures();
            NotePreviewViewController.UpdateColors();
        }

        protected string ArrowMesh
        {
            get => Config.ArrowMesh;
            set
            {
                Config.ArrowMesh = value;
                NotePreviewViewController.UpdateArrowMeshes();
                _ = ForceAsyncUpdateForGlowTexture();
            }
        }

        protected float LeftMinBrightness
        {
            get => Config.LeftMinBrightness;
            set
            {
                Config.LeftMinBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float LeftMaxBrightness
        {
            get => Config.LeftMaxBrightness;
            set
            {
                Config.LeftMaxBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float RightMinBrightness
        {
            get => Config.RightMinBrightness;
            set
            {
                Config.RightMinBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float RightMaxBrightness
        {
            get => Config.RightMaxBrightness;
            set
            {
                Config.RightMaxBrightness = Mathf.Clamp(value, 0.0f, 1.0f);
                NotePreviewViewController.UpdateColors();
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected string LeftGlowBlendOp
        {
            get => Config.LeftGlowBlendOp;
            set
            {
                Config.LeftGlowBlendOp = value;
                _ = ForceAsyncUpdateForGlowTexture();
            }
        }
        
        protected string RightGlowBlendOp
        {
            get => Config.RightGlowBlendOp;
            set
            {
                Config.RightGlowBlendOp = value;
                _ = ForceAsyncUpdateForGlowTexture();
            }
        }

        protected float LeftGlowOffsetX
        {
            get => Config.LeftGlowOffset.x;
            set
            {
                var pos = Config.LeftGlowOffset;
                pos.x = value;
                Config.LeftGlowOffset = pos;
                
                NotePreviewViewController.UpdateDotPosition();
                NotePreviewViewController.UpdateArrowPosition();
            }
        }
        protected float LeftGlowOffsetY
        {
            get => Config.LeftGlowOffset.y;
            set
            {
                var pos = Config.LeftGlowOffset;
                pos.y = value;
                Config.LeftGlowOffset = pos;
                
                NotePreviewViewController.UpdateDotPosition();
                NotePreviewViewController.UpdateArrowPosition();
            }
        }
        
        protected float RightGlowOffsetX
        {
            get => Config.RightGlowOffset.x;
            set
            {
                var pos = Config.RightGlowOffset;
                pos.x = value;
                Config.RightGlowOffset = pos;
                
                NotePreviewViewController.UpdateDotPosition();
                NotePreviewViewController.UpdateArrowPosition();
            }
        }
        protected float RightGlowOffsetY
        {
            get => Config.RightGlowOffset.y;
            set
            {
                var pos = Config.RightGlowOffset;
                pos.y = value;
                Config.RightGlowOffset = pos;
                
                NotePreviewViewController.UpdateDotPosition();
                NotePreviewViewController.UpdateArrowPosition();
            }
        }

        protected bool EnableNoteOutlines
        {
            get => Config.EnableNoteOutlines;
            set
            {
                Config.EnableNoteOutlines = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected int NoteOutlineScale
        {
            get => Config.NoteOutlineScale;
            set
            {
                Config.NoteOutlineScale = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected Color NoteOutlineLeftColor
        {
            get => Config.NoteOutlineLeftColor;
            set
            {
                Config.NoteOutlineLeftColor = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected Color NoteOutlineRightColor
        {
            get => Config.NoteOutlineRightColor;
            set
            {
                Config.NoteOutlineRightColor = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected Color BombOutlineColor
        {
            get => Config.BombOutlineColor;
            set
            {
                Config.BombOutlineColor = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected float NoteOutlineLeftColorSkew
        {
            get => Config.NoteOutlineLeftColorSkew;
            set
            {
                Config.NoteOutlineLeftColorSkew = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float NoteOutlineRightColorSkew
        {
            get => Config.NoteOutlineRightColorSkew;
            set
            {
                Config.NoteOutlineRightColorSkew = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected bool NormalizeLeftOutlineColor
        {
            get => Config.NormalizeLeftOutlineColor;
            set
            {
                Config.NormalizeLeftOutlineColor = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected bool NormalizeRightOutlineColor
        {
            get => Config.NormalizeRightOutlineColor;
            set
            {
                Config.NormalizeRightOutlineColor = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected bool EnableBombOutlines
        {
            get => Config.EnableBombOutlines;
            set
            {
                Config.EnableBombOutlines = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }

        protected int BombOutlineScale
        {
            get => Config.BombOutlineScale;
            set
            {
                Config.BombOutlineScale = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float FogStartOffset
        {
            get => Config.FogStartOffset;
            set
            {
                Config.FogStartOffset = value;
                Materials.UpdateFogValues("FogStartOffset");
            }
        }
        protected float FogScale
        {
            get => Config.FogScale;
            set
            {
                Config.FogScale = value;
                Materials.UpdateFogValues("FogScale");
            }
        }
        protected float FogHeightOffset
        {
            get => Config.FogHeightOffset;
            set
            {
                Config.FogHeightOffset = value;
                Materials.UpdateFogValues("FogHeightOffset");
            }
        }
        protected float FogHeightScale
        {
            get => Config.FogHeightScale;
            set
            {
                Config.FogHeightScale = value;
                Materials.UpdateFogValues("FogHeightScale");
            }
        }

        protected bool EnableFog
        {
            get => Config.EnableFog;
            set
            {
                Config.EnableFog = value;
                Materials.UpdateFogValues("FogStartOffset");
            }
        }
        
        protected bool EnableHeightFog
        {
            get => Config.EnableHeightFog;
            set
            {
                Config.EnableHeightFog = value;
                Materials.UpdateFogValues("FogHeightOffset");
            }
        }
        
        protected float RimDarkening
        {
            get => Config.RimDarkening;
            set
            {
                Config.RimDarkening = value;
                Materials.UpdateRimValues("RimDarkening");
            }
        }
        
        protected float RimOffset
        {
            get => Config.RimOffset;
            set
            {
                Config.RimOffset = value;
                Materials.UpdateRimValues("RimOffset");
            }
        }
        
        protected float RimScale
        {
            get => Config.RimScale;
            set
            {
                Config.RimScale = value;
                Materials.UpdateRimValues("RimScale");
            }
        }
        
        protected float Smoothness
        {
            get => Config.Smoothness;
            set
            {
                Config.Smoothness = value;
                Materials.UpdateRimValues("Smoothness");
            }
        }
        
        protected float RimCameraDistanceOffset
        {
            get => Config.RimCameraDistanceOffset;
            set
            {
                Config.RimCameraDistanceOffset = value;
                Materials.UpdateRimValues("RimCameraDistanceOffset");
            }
        }

        protected string RainbowBombMode
        {
            get => Config.RainbowBombMode;
            set
            {
                Config.RainbowBombMode = value;
                NotePreviewViewController.UpdateBombColors();
            }
        }
        
        protected bool AddBloomForOutlines
        {
            get => Config.AddBloomForOutlines;
            set
            {
                Config.AddBloomForOutlines = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected bool AddBloomForFaceSymbols
        {
            get => Config.AddBloomForFaceSymbols;
            set
            {
                Config.AddBloomForFaceSymbols = value;
                NotePreviewViewController.UpdateColors();
            }
        }
        
        protected float OutlineBloomAmount
        {
            get => Config.OutlineBloomAmount;
            set
            {
                Config.OutlineBloomAmount = value;
                NotePreviewViewController.UpdateOutlines();
            }
        }
        
        protected float FaceSymbolBloomAmount
        {
            get => Config.FaceSymbolBloomAmount;
            set
            {
                Config.FaceSymbolBloomAmount = value;
                NotePreviewViewController.UpdateColors();
            }
        }

        protected string BombMesh
        {
            get => Config.BombMesh;
            set
            {
                Config.BombMesh = value;
                
                NotePreviewViewController.UpdateBombMeshes();
                NotifyPropertyChanged(nameof(BombMeshIsSphere));
            }
        }

        protected int BombMeshSlices
        {
            get => Config.BombMeshSlices;
            set
            {
                Config.BombMeshSlices = value;
                NotePreviewViewController.UpdateBombMeshes();
            }
        }
        
        protected int BombMeshStacks
        {
            get => Config.BombMeshStacks;
            set
            {
                Config.BombMeshStacks = value;
                NotePreviewViewController.UpdateBombMeshes();
            }
        }
        
        protected bool BombMeshSmoothNormals
        {
            get => Config.BombMeshSmoothNormals;
            set
            {
                Config.BombMeshSmoothNormals = value;
                NotePreviewViewController.UpdateBombMeshes();
            }
        }

        protected bool BombMeshWorldNormals
        {
            get => Config.BombMeshWorldNormals;
            set
            {
                Config.BombMeshWorldNormals = value;
                NotePreviewViewController.UpdateBombMeshes();
            }
        }

        protected float NoteTextureBrightness
        {
            get => Config.NoteTextureBrightness;
            set => Config.NoteTextureBrightness = Mathf.Clamp(value, 0.0f, 2.0f);
        }

        protected float BombTextureBrightness
        {
            get => Config.BombTextureBrightness;
            set => Config.BombTextureBrightness = Mathf.Clamp(value, 0.0f, 2.0f);
        }
        
        protected float NoteTextureContrast
        {
            get => Config.NoteTextureContrast;
            set => Config.NoteTextureContrast = Mathf.Clamp(value, 0.0f, 2.0f);
        }

        protected float BombTextureContrast
        {
            get => Config.BombTextureContrast;
            set => Config.BombTextureContrast = Mathf.Clamp(value, 0.0f, 2.0f);
        }

        [UIAction("ReloadNoteTexture")]
        public void ReloadNoteTexture()
        {
            if (LoadTextures)
            {
                _ = Textures.LoadNoteTexture(Config.NoteTexture, false, true);
            }
        }
        
        [UIAction("ReloadBombTexture")]
        public void ReloadBombTexture()
        {
            if (LoadTextures)
            {
                _ = Textures.LoadNoteTexture(Config.BombTexture, true, true);
            }
        }

        [UIValue("BombMeshIsSphere")]
        [UsedImplicitly]
        private bool BombMeshIsSphere => BombMesh == "Sphere";
        
        [UIValue("rainbowBombModeChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> RainbowBombModeChoices = new List<object> { "Both", "Only Outlines", "Only Bombs" };
        
        [UIValue("glowBlendOperationChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> BlendOperationChoices = new List<object> { "Add", "ReverseSubtract" };
        
        [UIValue("glowTextureChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> GlowTextureChoices = new List<object> { "Glow", "GlowInterlaced", "Solid" };
        
        [UIValue("arrowMeshChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> ArrowMeshChoices = new List<object> { "Default", "Chevron", "Line", "Oval", "Pentagon", "Pointy", "Triangle" };
        
        [UIValue("bombMeshChoices")]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private List<object> BombMeshChoices = new List<object> { "Default", "Sphere" };

        [UIComponent("selectedNoteTexture")]
        #pragma warning disable CS0649
        // ReSharper disable once InconsistentNaming
        public DropDownListSetting noteTextureDropDown;
        #pragma warning restore CS0649

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
        
        internal async Task RefreshAll()
        {
            foreach (PropertyInfo propertyInfo in GetType()
                         .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                         .Where(c => c.GetMethod != null && c.GetMethod.IsFamily || c.SetMethod != null && c.SetMethod.IsFamily))
            {
                Plugin.Log.Info($"Calling changed event on {propertyInfo.Name}");
                NotifyPropertyChanged(propertyInfo.Name);
            }
            
            NotifyPropertyChanged(nameof(BombMeshIsSphere));
            
            await NotePreviewViewController.RefreshEverything();
        }
    }
}