﻿using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Materials
    {
        internal static Material ReplacementDotMaterial;
        internal static Material ReplacementArrowMaterial;
        internal static Material DotGlowMaterial;
        internal static Material ArrowGlowMaterial;
        internal static Material NoteMaterial;
        internal static Material DebrisMaterial;
        internal static Material BombMaterial;
        internal static Material OutlineMaterial0;
        internal static Material OutlineMaterial1;
        
        internal static readonly Material AccDotDepthMaterial = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Custom/ClearDepth"))
        {
            name = "AccDotMaterialDepthClear",
            enableInstancing = true
        };
        internal static Material AccDotMaterial;
        private static readonly int Color0 = Shader.PropertyToID("_Color");
        internal static readonly int BlendOpID = Shader.PropertyToID("_BlendOp");
        //private static readonly int ZTestID = Shader.PropertyToID("_ZTest");

        internal static async Task UpdateAll()
        {
            UpdateDebrisMaterial();
            UpdateReplacementDotMaterial();
            UpdateReplacementArrowMaterial();
            UpdateAccDotMaterial();
            UpdateOutlineMaterial();
            
            await UpdateDotGlowMaterial();
            await UpdateArrowGlowMaterial();
            
            await UpdateNoteMaterial();
            await UpdateBombMaterial();
            
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
                name = "NoteTweaks_ReplacementDotMaterial",
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray()
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
                name = "NoteTweaks_ReplacementArrowMaterial",
                color = Color.white,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray()
            };
        }

        private static async Task UpdateDotGlowMaterial()
        {
            if (GlowTextures.ReplacementDotGlowTexture == null)
            {
                await GlowTextures.LoadTextures();
            }
            
            if (DotGlowMaterial != null)
            {
                DotGlowMaterial.mainTexture = GlowTextures.ReplacementDotGlowTexture;
                return;
            }
            
            Plugin.Log.Info("Creating new dot glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            DotGlowMaterial = new Material(arrowGlowMat)
            {
                name = "NoteTweaks_DotGlowMaterial",
                mainTexture = GlowTextures.ReplacementDotGlowTexture
            };
        }

        private static async Task UpdateArrowGlowMaterial()
        {
            if (GlowTextures.ReplacementArrowGlowTexture == null)
            {
                await GlowTextures.LoadTextures();
            }
            
            if (ArrowGlowMaterial != null)
            {
                ArrowGlowMaterial.mainTexture = GlowTextures.ReplacementArrowGlowTexture;
                return;
            }
            
            Plugin.Log.Info("Creating new arrow glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            ArrowGlowMaterial = new Material(arrowGlowMat)
            {
                name = "NoteTweaks_ArrowGlowMaterial",
                mainTexture = GlowTextures.ReplacementArrowGlowTexture
            };
        }

        private static void UpdateAccDotMaterial()
        {
            if (AccDotMaterial != null)
            {
                AccDotMaterial.SetColor(Color0, Plugin.Config.AccDotColor.ColorWithAlpha(0f));
                return;
            }

            Plugin.Log.Info("Creating acc dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            AccDotMaterial = new Material(arrowMat)
            {
                name = "NoteTweaks_AccDotMaterial",
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray()
            };
            AccDotMaterial.SetColor(Color0, Plugin.Config.AccDotColor.ColorWithAlpha(0f));

            // uncomment later maybe
            // Utils.Materials.RepairShader(AccDotDepthMaterial);
        }
        
        private static void UpdateOutlineMaterial()
        {
            if (OutlineMaterial0 != null)
            {
                return;
            }

            Plugin.Log.Info("Creating outline materials");
            Material arrowMat0 = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowLW");
            OutlineMaterial0 = new Material(arrowMat0)
            {
                name = "NoteTweaks_OutlineMaterialLW",
                color = Color.black,
                shaderKeywords = arrowMat0.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray(),
                renderQueue = 1990
            };
            
            Material arrowMat1 = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            OutlineMaterial1 = new Material(arrowMat1)
            {
                name = "NoteTweaks_OutlineMaterialHD",
                color = Color.black,
                shaderKeywords = arrowMat1.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray(),
                renderQueue = 1990
            };
            
            //arrowMat0.SetInt(ZTestID, (int)UnityEngine.Rendering.CompareFunction.GreaterEqual);
            //arrowMat1.SetInt(ZTestID, (int)UnityEngine.Rendering.CompareFunction.GreaterEqual);
            //arrowMat0.SetInt("_CustomZWrite", 0);
            //arrowMat1.SetInt("_CustomZWrite", 0);
        }
        
        private static async Task UpdateNoteMaterial()
        {
            if (NoteMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new note material");
            Material noteMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteHD");
            NoteMaterial = new Material(noteMat)
            {
                name = "NoteTweaks_NoteMaterial",
                renderQueue = 1995
            };
            
            if (Textures.GetLoadedNoteTexture() != Plugin.Config.NoteTexture)
            {
                await Textures.LoadNoteTexture(Plugin.Config.NoteTexture);
            }
        }
        
        private static void UpdateDebrisMaterial()
        {
            if (DebrisMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new debris material");
            Material debrisMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteDebrisHD");
            DebrisMaterial = new Material(debrisMat)
            {
                name = "NoteTweaks_DebrisMaterial",
                renderQueue = 1995
            };
        }
        
        private static async Task UpdateBombMaterial()
        {
            if (BombMaterial != null)
            {
                return;
            }
            Plugin.Log.Info("Creating new bomb material");
            Material bombMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "BombNoteHD");
            BombMaterial = new Material(bombMat)
            {
                name = "NoteTweaks_BombMaterial"
            };
            
            if (Textures.GetLoadedBombTexture() != Plugin.Config.BombTexture)
            {
                await Textures.LoadNoteTexture(Plugin.Config.BombTexture, true);
            }
        }

        private static void UpdateRenderQueues()
        {
            if (Plugin.Config.EnableAccDot)
            {
                ReplacementArrowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 2000;
                ReplacementDotMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1998 : 2000;
                DotGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 1999;
                ArrowGlowMaterial.renderQueue = Plugin.Config.RenderAccDotsAboveSymbols ? 1997 : 1999;
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