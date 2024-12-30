using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using UnityEngine;

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
        
        public virtual float ColorBoostLeft { get; set; }
        public virtual float ColorBoostRight { get; set; }

        public virtual float GlowIntensity { get; set; } = 1.0f;
        public virtual float ArrowGlowScale { get; set; } = 1.0f;
        public virtual float DotGlowScale { get; set; } = 1.0f;
        public virtual bool EnableChainDots { get; set; } = true;
        public virtual Vector2 ChainDotScale { get; set; } = Vector2.one;
        public virtual bool EnableChainDotGlow { get; set; } = true;
        
        public virtual Color FaceColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        
        public virtual bool EnableAccDot { get; set; } = false;
        public virtual int AccDotSize { get; set; } = 15;
        public virtual Color AccDotColor { get; set; } = new Color(1f, 1f, 1f, 1f);
        public virtual bool RenderAccDotsAboveSymbols { get; set; } = false;
        
        public virtual int DotMeshSides { get; set; } = 16;

        public virtual float FaceColorNoteSkew { get; set; } = 0.04f;
    }
}