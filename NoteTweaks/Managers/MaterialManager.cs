using System.Linq;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal class Materials
    {
        internal static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        internal static readonly Texture2D ReplacementArrowGlowTexture = OriginalArrowGlowTexture.PrepareTexture();
        internal static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        internal static readonly Texture2D ReplacementDotGlowTexture = OriginalDotGlowTexture.PrepareTexture();
        
        internal static Material _replacementDotMaterial;
        internal static Material _replacementArrowMaterial;
        internal static Material _dotGlowMaterial;
        internal static Material _arrowGlowMaterial;
        
        internal static Material AccDotDepthMaterial = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Custom/ClearDepth"))
        {
            name = "AccDotMaterialDepthClear",
            renderQueue = 1996,
            enableInstancing = true
        };
        internal static Material _accDotMaterial;

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

        internal static void UpdateReplacementDotMaterial()
        {
            if (_replacementDotMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating replacement dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            _replacementDotMaterial = new Material(arrowMat)
            {
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                renderQueue = 2000
            };
        }
        
        internal static void UpdateReplacementArrowMaterial()
        {
            if (_replacementArrowMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating replacement arrow material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            _replacementArrowMaterial = new Material(arrowMat)
            {
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                renderQueue = 2000
            };
        }

        internal static void UpdateDotGlowMaterial()
        {
            if (_dotGlowMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new dot glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            _dotGlowMaterial = new Material(arrowGlowMat)
            {
                mainTexture = ReplacementDotGlowTexture,
                renderQueue = 1999
            };
        }

        internal static void UpdateArrowGlowMaterial()
        {
            if (_arrowGlowMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new arrow glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            _arrowGlowMaterial = new Material(arrowGlowMat)
            {
                mainTexture = ReplacementArrowGlowTexture,
                renderQueue = 1999
            };
        }

        internal static void UpdateAccDotMaterial()
        {
            if (_accDotMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating acc dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            _accDotMaterial = new Material(arrowMat)
            {
                name = "AccDotMaterial",
                renderQueue = 1997,
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray()
            };
            Color _c = Plugin.Config.AccDotColor;
            _c.a = 0f;
            _accDotMaterial.color = _c;
                
            // uncomment later maybe
            // Utils.Materials.RepairShader(AccDotDepthMaterial);
        }

        internal static void UpdateRenderQueues()
        {
            if (Plugin.Config.EnableAccDot)
            {
                _replacementArrowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                _replacementDotMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 2000;
                _dotGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
                _arrowGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 1999;
            }
            else
            {
                _replacementArrowMaterial.renderQueue = 2000;
                _replacementDotMaterial.renderQueue = 2000;
                _dotGlowMaterial.renderQueue = 1999;
                _arrowGlowMaterial.renderQueue = 1999;
            }
            
            if (Plugin.Config.RenderAccDotsAboveSymbols)
            {
                _accDotMaterial.renderQueue = 1999;
                AccDotDepthMaterial.renderQueue = 1998;
            }
            else
            {
                _accDotMaterial.renderQueue = 1997;
                AccDotDepthMaterial.renderQueue = 1996;
            }
        }
    }
}