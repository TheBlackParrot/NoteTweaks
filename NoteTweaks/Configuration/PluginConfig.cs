using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using UnityEngine;
// ReSharper disable RedundantDefaultMemberInitializer

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace NoteTweaks.Configuration
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        
        public virtual bool Enabled { get; set; } = true;
        
        public virtual bool EnableFaceGlow { get; set; } = true;

        public virtual Vector2 ArrowScale { get; set; } = Vector2.one;
        [Ignore] public virtual float ArrowScaleX
        {
            get => ArrowScale.x;
            set
            {
                Vector2 vec = ArrowScale;
                vec.x = value;
                ArrowScale = vec;
            }
        }
        [Ignore] public virtual float ArrowScaleY
        {
            get => ArrowScale.y;
            set
            {
                Vector2 vec = ArrowScale;
                vec.y = value;
                ArrowScale = vec;
            }
        }

        public virtual Vector2 ArrowPosition { get; set; } = Vector2.zero;
        [Ignore] public virtual float ArrowPositionX
        {
            get => ArrowPosition.x;
            set
            {
                Vector2 vec = ArrowPosition;
                vec.x = value;
                ArrowPosition = vec;
            }
        }
        [Ignore] public virtual float ArrowPositionY
        {
            get => ArrowPosition.y;
            set
            {
                Vector2 vec = ArrowPosition;
                vec.y = value;
                ArrowPosition = vec;
            }
        }
        
        public virtual bool EnableDots { get; set; } = true;
        public virtual Vector2 DotScale { get; set; } = Vector2.one;
        [Ignore] public virtual float DotScaleX
        {
            get => DotScale.x;
            set
            {
                Vector2 vec = DotScale;
                vec.x = value;
                DotScale = vec;
            }
        }
        [Ignore] public virtual float DotScaleY
        {
            get => DotScale.y;
            set
            {
                Vector2 vec = DotScale;
                vec.y = value;
                DotScale = vec;
            }
        }
        public virtual Vector2 DotPosition { get; set; } = Vector2.zero;
        [Ignore] public virtual float DotPositionX
        {
            get => DotPosition.x;
            set
            {
                Vector2 vec = DotPosition;
                vec.x = value;
                DotPosition = vec;
            }
        }
        [Ignore] public virtual float DotPositionY
        {
            get => DotPosition.y;
            set
            {
                Vector2 vec = DotPosition;
                vec.y = value;
                DotPosition = vec;
            }
        }
        
        public virtual Vector3 NoteScale { get; set; } = Vector3.one;
        [Ignore] public virtual float NoteScaleX
        {
            get => NoteScale.x;
            set
            {
                Vector3 vec = NoteScale;
                vec.x = value;
                NoteScale = vec;
            }
        }
        [Ignore] public virtual float NoteScaleY
        {
            get => NoteScale.y;
            set
            {
                Vector3 vec = NoteScale;
                vec.y = value;
                NoteScale = vec;
            }
        }
        [Ignore] public virtual float NoteScaleZ
        {
            get => NoteScale.z;
            set
            {
                Vector3 vec = NoteScale;
                vec.z = value;
                NoteScale = vec;
            }
        }
        
        public virtual float LinkScale { get; set; } = 1.0f;

        public virtual float ColorBoostLeft { get; set; } = 0.0f;
        public virtual float ColorBoostRight { get; set; } = 0.0f;

        public virtual float LeftGlowIntensity { get; set; } = 1.0f;
        public virtual float RightGlowIntensity { get; set; } = 1.0f;
        public virtual float ArrowGlowScale { get; set; } = 1.0f;
        public virtual float DotGlowScale { get; set; } = 1.0f;
        public virtual bool EnableChainDots { get; set; } = true;
        public virtual Vector2 ChainDotScale { get; set; } = Vector2.one;
        [Ignore] public virtual float ChainDotScaleX
        {
            get => ChainDotScale.x;
            set
            {
                Vector2 vec = ChainDotScale;
                vec.x = value;
                ChainDotScale = vec;
            }
        }
        [Ignore] public virtual float ChainDotScaleY
        {
            get => ChainDotScale.y;
            set
            {
                Vector2 vec = ChainDotScale;
                vec.y = value;
                ChainDotScale = vec;
            }
        }
        public virtual bool EnableChainDotGlow { get; set; } = true;
        
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color LeftFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color RightFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        
        public virtual bool EnableAccDot { get; set; } = false;
        public virtual int AccDotSize { get; set; } = 15;
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color AccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual bool RenderAccDotsAboveSymbols { get; set; } = false;
        
        public virtual int DotMeshSides { get; set; } = 16;

        public virtual float LeftFaceColorNoteSkew { get; set; } = 0.04f;
        public virtual float RightFaceColorNoteSkew { get; set; } = 0.04f;
        public virtual bool DisableIfNoodle { get; set; } = false;
        public virtual bool DisableIfVivify { get; set; } = false;
        public virtual float RotateDot { get; set; } = 0.0f;
        
        public bool NormalizeLeftFaceColor { get; set; } = false;
        public bool NormalizeRightFaceColor { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color LeftFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual float LeftFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public virtual bool NormalizeLeftFaceGlowColor { get; set; } = false;
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color RightFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual float RightFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public virtual bool NormalizeRightFaceGlowColor { get; set; } = false;
        
        public virtual string NoteTexture { get; set; } = "Default";
        public virtual bool InvertNoteTexture { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        public virtual Color BombColor { get; set; } = new Color(0.251f, 0.251f, 0.251f, 1.0f);
        public virtual float BombColorBoost { get; set; } = 0.0f;
        public virtual string BombTexture { get; set; } = "Default";
        public virtual float BombScale { get; set; } = 1.0f;
        public virtual bool InvertBombTexture { get; set; } = false;
        
        public virtual bool EnableRainbowBombs { get; set; } = false;
        public virtual float RainbowBombTimeScale { get; set; } = 6.0f;
        public virtual float RainbowBombSaturation { get; set; } = 0.67f;
        public virtual float RainbowBombValue { get; set; } = 0.9f;
        
        public virtual string GlowTexture { get; set; } = "Glow";
        
        public virtual string ArrowMesh { get; set; } = "Default";
        
        public virtual bool FixDotsIfNoodle { get; set; } = true;

        public virtual float LeftMinBrightness { get; set; } = 0.0f;
        public virtual float LeftMaxBrightness { get; set; } = 1.0f;
        public virtual float RightMinBrightness { get; set; } = 0.0f;
        public virtual float RightMaxBrightness { get; set; } = 1.0f;
        
        public virtual string LeftGlowBlendOp { get; set; } = "Add";
        public virtual string RightGlowBlendOp { get; set; } = "Add";
        
        public virtual Vector2 LeftGlowOffset { get; set; } = Vector2.zero;
        [Ignore] public virtual float LeftGlowOffsetX
        {
            get => LeftGlowOffset.x;
            set
            {
                Vector2 vec = LeftGlowOffset;
                vec.x = value;
                LeftGlowOffset = vec;
            }
        }
        [Ignore] public virtual float LeftGlowOffsetY
        {
            get => LeftGlowOffset.y;
            set
            {
                Vector2 vec = LeftGlowOffset;
                vec.y = value;
                LeftGlowOffset = vec;
            }
        }
        public virtual Vector2 RightGlowOffset { get; set; } = Vector2.zero;
        [Ignore] public virtual float RightGlowOffsetX
        {
            get => RightGlowOffset.x;
            set
            {
                Vector2 vec = RightGlowOffset;
                vec.x = value;
                RightGlowOffset = vec;
            }
        }
        [Ignore] public virtual float RightGlowOffsetY
        {
            get => RightGlowOffset.y;
            set
            {
                Vector2 vec = RightGlowOffset;
                vec.y = value;
                RightGlowOffset = vec;
            }
        }
        
        public bool EnableNoteOutlines { get; set; } = false;
        public bool EnableBombOutlines { get; set; } = false;
        public int NoteOutlineScale { get; set; } = 5;
        public int BombOutlineScale { get; set; } = 5;
        [UseConverter(typeof(HexColorConverter))]
        public Color NoteOutlineLeftColor { get; set; } = Color.black;
        public float NoteOutlineLeftColorSkew { get; set; } = 0.1f;
        public bool NormalizeLeftOutlineColor { get; set; } = false;
        public bool NormalizeRightOutlineColor { get; set; } = false;
        [UseConverter(typeof(HexColorConverter))]
        public Color NoteOutlineRightColor { get; set; } = Color.black;
        public float NoteOutlineRightColorSkew { get; set; } = 0.1f;
        [UseConverter(typeof(HexColorConverter))]
        public Color BombOutlineColor { get; set; } = Color.white;
        
        public bool EnableFog { get; set; } = true;
        public bool EnableHeightFog { get; set; } = true;
        public float FogStartOffset { get; set; } = 100f;
        public float FogScale { get; set; } = 0.5f;
        public float FogHeightOffset { get; set; } = 0f;
        public float FogHeightScale { get; set; } = 2.5f;
        
        public float RimDarkening { get; set; } = 0.2f;
        public float RimOffset { get; set; } = -0.1f;
        public float RimScale { get; set; } = 2f;
        public float Smoothness { get; set; } = 0.95f;
        public float RimCameraDistanceOffset { get; set; } = 5f;
        
        public string RainbowBombMode { get; set; } = "Both";
    }
}