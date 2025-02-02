using System.Runtime.CompilerServices;
using IPA.Config.Stores;
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
        public virtual Vector2 ArrowPosition { get; set; } = Vector2.zero;
        
        public virtual bool EnableDots { get; set; } = true;
        public virtual Vector2 DotScale { get; set; } = Vector2.one;
        public virtual Vector2 DotPosition { get; set; } = Vector2.zero;
        
        public virtual Vector3 NoteScale { get; set; } = Vector3.one;
        
        public virtual float LinkScale { get; set; } = 1.0f;

        public virtual float ColorBoostLeft { get; set; } = 0.0f;
        public virtual float ColorBoostRight { get; set; } = 0.0f;

        public virtual float LeftGlowIntensity { get; set; } = 1.0f;
        public virtual float RightGlowIntensity { get; set; } = 1.0f;
        public virtual float ArrowGlowScale { get; set; } = 1.0f;
        public virtual float DotGlowScale { get; set; } = 1.0f;
        public virtual bool EnableChainDots { get; set; } = true;
        public virtual Vector2 ChainDotScale { get; set; } = Vector2.one;
        public virtual bool EnableChainDotGlow { get; set; } = true;
        
        public virtual Color LeftFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual Color RightFaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        
        public virtual bool EnableAccDot { get; set; } = false;
        public virtual int AccDotSize { get; set; } = 15;
        public virtual Color AccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual bool RenderAccDotsAboveSymbols { get; set; } = false;
        
        public virtual int DotMeshSides { get; set; } = 16;

        public virtual float LeftFaceColorNoteSkew { get; set; } = 0.04f;
        public virtual float RightFaceColorNoteSkew { get; set; } = 0.04f;
        public virtual bool DisableIfNoodle { get; set; } = false;
        public virtual float RotateDot { get; set; } = 0.0f;
        
        public bool NormalizeLeftFaceColor { get; set; } = false;
        public bool NormalizeRightFaceColor { get; set; } = false;
        
        public virtual Color LeftFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual float LeftFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public virtual bool NormalizeLeftFaceGlowColor { get; set; } = false;
        public virtual Color RightFaceGlowColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual float RightFaceGlowColorNoteSkew { get; set; } = 1.0f;
        public virtual bool NormalizeRightFaceGlowColor { get; set; } = false;
        
        public virtual string NoteTexture { get; set; } = "Default";
        public virtual bool InvertNoteTexture { get; set; } = false;
        
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
        
        public virtual bool FixDotsIfNoodle { get; set; } = false;

        public virtual float LeftMinBrightness { get; set; } = 0.0f;
        public virtual float LeftMaxBrightness { get; set; } = 1.0f;
        public virtual float RightMinBrightness { get; set; } = 0.0f;
        public virtual float RightMaxBrightness { get; set; } = 1.0f;
        
        public virtual string LeftGlowBlendOp { get; set; } = "Add";
        public virtual string RightGlowBlendOp { get; set; } = "Add";
        
        public virtual Vector2 LeftGlowOffset { get; set; } = Vector2.zero;
        public virtual Vector2 RightGlowOffset { get; set; } = Vector2.zero;
        
        public bool EnableNoteOutlines { get; set; } = false;
        public bool EnableBombOutlines { get; set; } = false;
        public int NoteOutlineScale { get; set; } = 5;
        public int BombOutlineScale { get; set; } = 5;
        public Color NoteOutlineLeftColor { get; set; } = Color.black;
        public float NoteOutlineLeftColorSkew { get; set; } = 0.1f;
        public bool NormalizeLeftOutlineColor { get; set; } = false;
        public bool NormalizeRightOutlineColor { get; set; } = false;
        public Color NoteOutlineRightColor { get; set; } = Color.black;
        public float NoteOutlineRightColorSkew { get; set; } = 0.1f;
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
    }
}