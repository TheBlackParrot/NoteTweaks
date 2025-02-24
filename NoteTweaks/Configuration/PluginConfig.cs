using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using UnityEngine;
using System.Collections.Generic;
// ReSharper disable RedundantDefaultMemberInitializer

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace NoteTweaks.Configuration
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        
        public bool Enabled { get; set; } = true;
        
        public bool EnableFaceGlow { get; set; } = true;

        public Vector2 ArrowScale { get; set; } = Vector2.one;
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

        public Vector2 ArrowPosition { get; set; } = Vector2.zero;
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
        
        public bool EnableDots { get; set; } = true;
        public Vector2 DotScale { get; set; } = Vector2.one;
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
        public Vector2 DotPosition { get; set; } = Vector2.zero;
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
        
        public Vector3 NoteScale { get; set; } = Vector3.one;
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
        
        public float LinkScale { get; set; } = 1.0f;

        public float ColorBoostLeft { get; set; } = 0.0f;
        public float ColorBoostRight { get; set; } = 0.0f;

        public float LeftGlowIntensity { get; set; } = 1.0f;
        public float RightGlowIntensity { get; set; } = 1.0f;
        public float ArrowGlowScale { get; set; } = 1.0f;
        public float DotGlowScale { get; set; } = 1.0f;
        public bool EnableChainDots { get; set; } = true;
        public Vector2 ChainDotScale { get; set; } = Vector2.one;
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
        public bool EnableChainDotGlow { get; set; } = true;
        
        [UseConverter(typeof(HexColorConverter))]
        public Color LeftFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [UseConverter(typeof(HexColorConverter))]
        public Color RightFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        
        public bool EnableAccDot { get; set; } = false;
        public int AccDotSize { get; set; } = 15;
        [UseConverter(typeof(HexColorConverter))]
        public Color AccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public bool RenderAccDotsAboveSymbols { get; set; } = false;
        
        public int DotMeshSides { get; set; } = 16;

        public float LeftFaceColorNoteSkew { get; set; } = 0.04f;
        public float RightFaceColorNoteSkew { get; set; } = 0.04f;
        public bool DisableIfNoodle { get; set; } = false;
        public bool DisableIfVivify { get; set; } = false;
        public float RotateDot { get; set; } = 0.0f;
        
        public bool NormalizeLeftFaceColor { get; set; } = false;
        public bool NormalizeRightFaceColor { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        public Color LeftFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public float LeftFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public bool NormalizeLeftFaceGlowColor { get; set; } = false;
        [UseConverter(typeof(HexColorConverter))]
        public Color RightFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public float RightFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public bool NormalizeRightFaceGlowColor { get; set; } = false;
        
        public string NoteTexture { get; set; } = "Default";
        public bool InvertNoteTexture { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        public Color BombColor { get; set; } = new Color(0.251f, 0.251f, 0.251f, 1.0f);
        public float BombColorBoost { get; set; } = 0.0f;
        public string BombTexture { get; set; } = "Default";
        public float BombScale { get; set; } = 1.0f;
        public bool InvertBombTexture { get; set; } = false;
        
        public bool EnableRainbowBombs { get; set; } = false;
        public float RainbowBombTimeScale { get; set; } = 6.0f;
        public float RainbowBombSaturation { get; set; } = 0.67f;
        public float RainbowBombValue { get; set; } = 0.9f;
        
        public string GlowTexture { get; set; } = "Glow";
        
        public string ArrowMesh { get; set; } = "Default";
        
        public bool FixDotsIfNoodle { get; set; } = true;

        public float LeftMinBrightness { get; set; } = 0.0f;
        public float LeftMaxBrightness { get; set; } = 1.0f;
        public float RightMinBrightness { get; set; } = 0.0f;
        public float RightMaxBrightness { get; set; } = 1.0f;
        
        public string LeftGlowBlendOp { get; set; } = "Add";
        public string RightGlowBlendOp { get; set; } = "Add";
        
        public Vector2 LeftGlowOffset { get; set; } = Vector2.zero;
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
        public Vector2 RightGlowOffset { get; set; } = Vector2.zero;
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
        
        public bool AddBloomForOutlines { get; set; } = false;
        public bool AddBloomForFaceSymbols { get; set; } = false;
        
        public float OutlineBloomAmount { get; set; } = 0.1f;
        public float FaceSymbolBloomAmount { get; set; } = 0.1f;

        public virtual Dictionary<string, PluginConfig> Presets { get; set; } = new Dictionary<string, PluginConfig>();

        public void SavePreset(string presetName)
        {
            Presets[presetName] = (PluginConfig)MemberwiseClone();
        }

        public void LoadPreset(string presetName)
        {
            if (Presets.TryGetValue(presetName, out var preset))
            {
                foreach (var property in GetType().GetProperties())
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(this, property.GetValue(preset));
                    }
                }
            }
        }

        public void DeletePreset(string presetName)
        {
            Presets.Remove(presetName);
        public virtual void Changed()
        {
        }
    }
}
