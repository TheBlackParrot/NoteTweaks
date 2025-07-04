using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable RedundantDefaultMemberInitializer

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace NoteTweaks.Configuration
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [JsonObject(MemberSerialization.OptIn)]
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        [Ignore]
        public static ConfigurationPresetManager PresetManager { get; set; } = new ConfigurationPresetManager();
        
        public bool Enabled { get; set; } = true;
        
        [JsonProperty] public bool EnableFaceGlow { get; set; } = true;

        [JsonProperty] public Vector2 ArrowScale { get; set; } = Vector2.one;
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

        [JsonProperty] public Vector2 ArrowPosition { get; set; } = Vector2.zero;
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
        
        [JsonProperty] public bool EnableDots { get; set; } = true;
        [JsonProperty] public Vector2 DotScale { get; set; } = Vector2.one;
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
        [JsonProperty] public Vector2 DotPosition { get; set; } = Vector2.zero;
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
        
        [JsonProperty] public Vector3 NoteScale { get; set; } = Vector3.one;
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
        
        [JsonProperty] public float LinkScale { get; set; } = 1.0f;

        [JsonProperty] public float ColorBoostLeft { get; set; } = 0.0f;
        [JsonProperty] public float ColorBoostRight { get; set; } = 0.0f;

        [JsonProperty] public float LeftGlowIntensity { get; set; } = 1.0f;
        [JsonProperty] public float RightGlowIntensity { get; set; } = 1.0f;
        [JsonProperty] public float ArrowGlowScale { get; set; } = 1.0f;
        [JsonProperty] public float DotGlowScale { get; set; } = 1.0f;
        [JsonProperty] public bool EnableChainDots { get; set; } = true;
        [JsonProperty] public Vector2 ChainDotScale { get; set; } = Vector2.one;
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
        [JsonProperty] public bool EnableChainDotGlow { get; set; } = true;
        
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color LeftFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color RightFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        
        public bool EnableAccDot { get; set; } = false;
        public int AccDotSize { get; set; } = 15;
        [UseConverter(typeof(HexColorConverter))]
        public Color LeftAccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [UseConverter(typeof(HexColorConverter))]
        public Color RightAccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public float LeftAccDotColorNoteSkew { get; set; } = 0.0f;
        public float RightAccDotColorNoteSkew { get; set; } = 0.0f;
        public bool NormalizeLeftAccDotColor { get; set; } = false;
        public bool NormalizeRightAccDotColor { get; set; } = false;
        public bool RenderAccDotsAboveSymbols { get; set; } = false;
        
        [JsonProperty] public int DotMeshSides { get; set; } = 16;

        [JsonProperty] public float LeftFaceColorNoteSkew { get; set; } = 0.04f;
        [JsonProperty] public float RightFaceColorNoteSkew { get; set; } = 0.04f;
        public bool DisableIfNoodle { get; set; } = false;
        public bool DisableIfVivify { get; set; } = false;
        [JsonProperty] public float RotateDot { get; set; } = 0.0f;
        
        [JsonProperty] public bool NormalizeLeftFaceColor { get; set; } = false;
        [JsonProperty] public bool NormalizeRightFaceColor { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color LeftFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [JsonProperty] public float LeftFaceGlowColorNoteSkew { get; set; } = 1.0f;
        [JsonProperty] public bool NormalizeLeftFaceGlowColor { get; set; } = false;
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color RightFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        [JsonProperty] public float RightFaceGlowColorNoteSkew { get; set; } = 1.0f;
        [JsonProperty] public bool NormalizeRightFaceGlowColor { get; set; } = false;
        
        [JsonProperty] public string NoteTexture { get; set; } = "Default";
        [JsonProperty] public bool InvertNoteTexture { get; set; } = false;
        
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color BombColor { get; set; } = new Color(0.251f, 0.251f, 0.251f, 1.0f);
        [JsonProperty] public float BombColorBoost { get; set; } = 0.0f;
        [JsonProperty] public string BombTexture { get; set; } = "Default";
        [JsonProperty] public float BombScale { get; set; } = 1.0f;
        [JsonProperty] public bool InvertBombTexture { get; set; } = false;
        
        [JsonProperty] public bool EnableRainbowBombs { get; set; } = false;
        [JsonProperty] public float RainbowBombTimeScale { get; set; } = 6.0f;
        [JsonProperty] public float RainbowBombSaturation { get; set; } = 0.67f;
        [JsonProperty] public float RainbowBombValue { get; set; } = 0.9f;
        
        [JsonProperty] public string GlowTexture { get; set; } = "Glow";
        
        [JsonProperty] public string ArrowMesh { get; set; } = "Default";
        
        public bool FixDotsIfNoodle { get; set; } = true;

        [JsonProperty] public float LeftMinBrightness { get; set; } = 0.0f;
        [JsonProperty] public float LeftMaxBrightness { get; set; } = 1.0f;
        [JsonProperty] public float RightMinBrightness { get; set; } = 0.0f;
        [JsonProperty] public float RightMaxBrightness { get; set; } = 1.0f;
        
        [JsonProperty] public string LeftGlowBlendOp { get; set; } = "Add";
        [JsonProperty] public string RightGlowBlendOp { get; set; } = "Add";
        
        [JsonProperty] public Vector2 LeftGlowOffset { get; set; } = Vector2.zero;
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
        [JsonProperty] public Vector2 RightGlowOffset { get; set; } = Vector2.zero;
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
        
        [JsonProperty] public bool EnableNoteOutlines { get; set; } = false;
        [JsonProperty] public bool EnableBombOutlines { get; set; } = false;
        [JsonProperty] public int NoteOutlineScale { get; set; } = 5;
        [JsonProperty] public int BombOutlineScale { get; set; } = 5;
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color NoteOutlineLeftColor { get; set; } = Color.black;
        [JsonProperty] public float NoteOutlineLeftColorSkew { get; set; } = 0.1f;
        [JsonProperty] public bool NormalizeLeftOutlineColor { get; set; } = false;
        [JsonProperty] public bool NormalizeRightOutlineColor { get; set; } = false;
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color NoteOutlineRightColor { get; set; } = Color.black;
        [JsonProperty] public float NoteOutlineRightColorSkew { get; set; } = 0.1f;
        [UseConverter(typeof(HexColorConverter))]
        [JsonProperty, JsonConverter(typeof(ColorConverter))] public Color BombOutlineColor { get; set; } = Color.white;
        
        // leaving fog out of presets, think this should *always* be up to the user
        public bool EnableFog { get; set; } = true;
        public bool EnableHeightFog { get; set; } = true;
        public float FogStartOffset { get; set; } = 100f;
        public float FogScale { get; set; } = 0.5f;
        public float FogHeightOffset { get; set; } = 0f;
        public float FogHeightScale { get; set; } = 2.5f;
        
        [JsonProperty] public float RimDarkening { get; set; } = 0.2f;
        [JsonProperty] public float RimOffset { get; set; } = -0.1f;
        [JsonProperty] public float RimScale { get; set; } = 2f;
        [JsonProperty] public float Smoothness { get; set; } = 0.95f;
        [JsonProperty] public float RimCameraDistanceOffset { get; set; } = 5f;
        
        [JsonProperty] public string RainbowBombMode { get; set; } = "Both";
        
#if PRE_V1_39_1
        [JsonProperty] public float LeftOutlineFinalColorMultiplier { get; set; } = 1f;
        [JsonProperty] public float RightOutlineFinalColorMultiplier { get; set; } = 1f;
        [JsonProperty] public float BombOutlineFinalColorMultiplier { get; set; } = 1f;
#endif
        
        [JsonProperty] public bool AddBloomForOutlines { get; set; } = false;
        [JsonProperty] public bool AddBloomForFaceSymbols { get; set; } = false;
        
        [JsonProperty] public float OutlineBloomAmount { get; set; } = 0.1f;
        [JsonProperty] public float FaceSymbolBloomAmount { get; set; } = 0.1f;
        
        [JsonProperty] public string BombMesh { get; set; } = "Default";
        [JsonProperty] public int BombMeshStacks { get; set; } = 6;
        [JsonProperty] public int BombMeshSlices { get; set; } = 8;
        [JsonProperty] public bool BombMeshSmoothNormals { get; set; } = false;
        [JsonProperty] public bool BombMeshWorldNormals { get; set; } = false;

        [JsonProperty] public float NoteTextureBrightness { get; set; } = 1.0f;
        [JsonProperty] public float BombTextureBrightness { get; set; } = 1.0f;
        [JsonProperty] public float NoteTextureContrast { get; set; } = 1.0f;
        [JsonProperty] public float BombTextureContrast { get; set; } = 1.0f;
        
        // meshes, also, should be up to the user imo
        public string NoteMesh { get; set; } = "Default";

        internal PluginConfig ShallowCopy()
        {
            return (PluginConfig)MemberwiseClone();
        }
        
        internal string GetSerializedJson([CanBeNull] JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(this, settings);
        }

        public void Changed()
        {
        }
    }
}