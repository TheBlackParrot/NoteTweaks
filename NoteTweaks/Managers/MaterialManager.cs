using System.Linq;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Materials
    {
        private static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        private static readonly Texture2D ReplacementArrowGlowTexture = OriginalArrowGlowTexture.PrepareTexture();
        private static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        private static readonly Texture2D ReplacementDotGlowTexture = OriginalDotGlowTexture.PrepareTexture();
        
        internal static Material ReplacementDotMaterial;
        internal static Material ReplacementArrowMaterial;
        internal static Material DotGlowMaterial;
        internal static Material ArrowGlowMaterial;
        
        internal static readonly Material AccDotDepthMaterial = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Custom/ClearDepth"))
        {
            name = "AccDotMaterialDepthClear",
            renderQueue = 1996,
            enableInstancing = true
        };
        internal static Material AccDotMaterial;

        internal static void UpdateAll()
        {
            Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "NoteHD").renderQueue = 1995;
            Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "BurstSliderNoteHD").renderQueue = 1995;
            Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "BurstSliderHeadNoteHD").renderQueue = 1995;
            
            UpdateReplacementDotMaterial();
            UpdateReplacementArrowMaterial();
            UpdateDotGlowMaterial();
            UpdateArrowGlowMaterial();
            UpdateAccDotMaterial();
            
            UpdateRenderQueues();
        }

        private static void UpdateReplacementDotMaterial()
        {
            if (ReplacementDotMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating replacement dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            ReplacementDotMaterial = new Material(arrowMat)
            {
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                renderQueue = 2000
            };
        }

        private static void UpdateReplacementArrowMaterial()
        {
            if (ReplacementArrowMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating replacement arrow material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            ReplacementArrowMaterial = new Material(arrowMat)
            {
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                renderQueue = 2000
            };
        }

        private static void UpdateDotGlowMaterial()
        {
            if (DotGlowMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new dot glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            DotGlowMaterial = new Material(arrowGlowMat)
            {
                mainTexture = ReplacementDotGlowTexture,
                renderQueue = 1999
            };
        }

        private static void UpdateArrowGlowMaterial()
        {
            if (ArrowGlowMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new arrow glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            ArrowGlowMaterial = new Material(arrowGlowMat)
            {
                mainTexture = ReplacementArrowGlowTexture,
                renderQueue = 1999
            };
        }

        private static void UpdateAccDotMaterial()
        {
            if (AccDotMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating acc dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            AccDotMaterial = new Material(arrowMat)
            {
                name = "AccDotMaterial",
                renderQueue = 1997,
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray()
            };
            Color _c = Plugin.Config.AccDotColor;
            _c.a = 0f;
            AccDotMaterial.color = _c;
                
            // uncomment later maybe
            // Utils.Materials.RepairShader(AccDotDepthMaterial);
        }

        private static void UpdateRenderQueues()
        {
            if (Plugin.Config.EnableAccDot)
            {
                ReplacementArrowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                ReplacementDotMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                DotGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
                ArrowGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
            }
            else
            {
                ReplacementArrowMaterial.renderQueue = 2000;
                ReplacementDotMaterial.renderQueue = 2000;
                DotGlowMaterial.renderQueue = 1999;
                ArrowGlowMaterial.renderQueue = 1999;
            }
            
            if (Plugin.Config.RenderAccDotsAboveSymbols)
            {
                AccDotMaterial.renderQueue = 1999;
                AccDotDepthMaterial.renderQueue = 1998;
            }
            else
            {
                AccDotMaterial.renderQueue = 1997;
                AccDotDepthMaterial.renderQueue = 1996;
            }
        }
    }
}