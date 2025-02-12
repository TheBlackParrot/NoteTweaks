using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using NoteTweaks.Configuration;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Materials
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        internal static Material ReplacementDotMaterial;
        internal static Material ReplacementArrowMaterial;
        internal static Material DotGlowMaterial;
        internal static Material ArrowGlowMaterial;
        internal static Material NoteMaterial;
        internal static Material DebrisMaterial;
        internal static Material BombMaterial;
        internal static Material OutlineMaterial;
        
        internal static readonly Material AccDotDepthMaterial = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Custom/ClearDepth"))
        {
            name = "AccDotMaterialDepthClear",
            enableInstancing = true
        };
        internal static Material AccDotMaterial;

        private static readonly int MainEffectContainerID = Resources.FindObjectsOfTypeAll<BoolSO>().First(x => x.name.StartsWith("MainEffectContainer.")).GetInstanceID();
        internal static BoolSO MainEffectContainer => Resources.InstanceIDToObject(MainEffectContainerID) as BoolSO;
        internal static float SaneAlphaValue => MainEffectContainer.value ? 1f : 0f;
        
        private static readonly int Color0 = Shader.PropertyToID("_Color");
        internal static readonly int BlendOpID = Shader.PropertyToID("_BlendOp");
        internal static readonly int SrcFactorID = Shader.PropertyToID("_BlendSrcFactor");
        internal static int SrcFactor => 1;
        internal static readonly int DstFactorID = Shader.PropertyToID("_BlendDstFactor");
        internal static int DstFactor => 0;
        internal static readonly int SrcFactorAlphaID = Shader.PropertyToID("_BlendSrcFactorA");
        internal static int SrcFactorAlpha => 0;
        internal static readonly int DstFactorAlphaID = Shader.PropertyToID("_BlendDstFactorA");
        internal static int DstFactorAlpha => 0;
        
        private static readonly List<KeyValuePair<string, int>> FogKeywords = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("FogHeightOffset", Shader.PropertyToID("_FogHeightOffset")),
            new KeyValuePair<string, int>("FogHeightScale", Shader.PropertyToID("_FogHeightScale")),
            new KeyValuePair<string, int>("FogScale", Shader.PropertyToID("_FogScale")),
            new KeyValuePair<string, int>("FogStartOffset", Shader.PropertyToID("_FogStartOffset"))
        };
        private static readonly List<KeyValuePair<string, int>> RimKeywords = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("RimDarkening", Shader.PropertyToID("_RimDarkening")),
            new KeyValuePair<string, int>("RimOffset", Shader.PropertyToID("_RimOffset")),
            new KeyValuePair<string, int>("RimScale", Shader.PropertyToID("_RimScale")),
            new KeyValuePair<string, int>("Smoothness", Shader.PropertyToID("_Smoothness")),
            new KeyValuePair<string, int>("RimCameraDistanceOffset", Shader.PropertyToID("_RimCameraDistanceOffset"))
        };

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
            UpdateFogValues();
            UpdateRimValues();
        }

        internal static void UpdateFogValues(string which = null)
        {
            if (NoteMaterial == null)
            {
                return;
            }
            
            List<KeyValuePair<string, int>> fogKeywords = which == null ? FogKeywords : FogKeywords.Where(x => x.Key == which).ToList();
            
            fogKeywords.Do(keyword =>
            {
                PropertyInfo prop = Config.GetType().GetProperty(keyword.Key);
                if (prop != null)
                {
                    float value = (float)prop.GetValue(Config, null);

                    if (keyword.Key == "FogStartOffset")
                    {
                        value = Config.EnableFog ? value : 999999f;
                    }
                    if (keyword.Key == "FogHeightOffset")
                    {
                        value = Config.EnableHeightFog ? value : 999999f;
                    }
                    
                    ReplacementDotMaterial.SetFloat(keyword.Value, value);
                    ReplacementArrowMaterial.SetFloat(keyword.Value, value);
                    DotGlowMaterial.SetFloat(keyword.Value, value);
                    ArrowGlowMaterial.SetFloat(keyword.Value, value);
                    NoteMaterial.SetFloat(keyword.Value, value);
                    DebrisMaterial.SetFloat(keyword.Value, value);
                    BombMaterial.SetFloat(keyword.Value, value);
                    OutlineMaterial.SetFloat(keyword.Value, value);
                    AccDotMaterial.SetFloat(keyword.Value, value);
                }
            });
        }
        
        internal static void UpdateRimValues(string which = null)
        {
            if (NoteMaterial == null)
            {
                return;
            }
            
            List<KeyValuePair<string, int>> rimKeywords = which == null ? RimKeywords : RimKeywords.Where(x => x.Key == which).ToList();
            
            rimKeywords.Do(keyword =>
            {
                PropertyInfo prop = Config.GetType().GetProperty(keyword.Key);
                if (prop != null)
                {
                    float value = (float)prop.GetValue(Config, null);
                    
                    NoteMaterial.SetFloat(keyword.Value, value);
                    DebrisMaterial.SetFloat(keyword.Value, value);
                }
            });
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
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE" || x != "_EMISSION").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE" || x.name != "_EMISSION").ToArray()
            };
            
            ReplacementDotMaterial.SetInt(SrcFactorID, SrcFactor);
            ReplacementDotMaterial.SetInt(DstFactorID, DstFactor);
            ReplacementDotMaterial.SetInt(SrcFactorAlphaID, SrcFactorAlpha);
            ReplacementDotMaterial.SetInt(DstFactorAlphaID, DstFactorAlpha);
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
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE" || x != "_EMISSION").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE" || x.name != "_EMISSION").ToArray()
            };
            
            ReplacementDotMaterial.SetInt(SrcFactorID, SrcFactor);
            ReplacementDotMaterial.SetInt(DstFactorID, DstFactor);
            ReplacementDotMaterial.SetInt(SrcFactorAlphaID, SrcFactorAlpha);
            ReplacementDotMaterial.SetInt(DstFactorAlphaID, DstFactorAlpha);
        }

        private static async Task UpdateDotGlowMaterial()
        {
            if (GlowTextures.ReplacementDotGlowTexture == null)
            {
                await GlowTextures.LoadTextures();
            }
            
            if (DotGlowMaterial != null)
            {
                await GlowTextures.UpdateTextures();
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
                await GlowTextures.UpdateTextures();
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
                AccDotMaterial.SetColor(Color0, Config.AccDotColor.ColorWithAlpha(0f));
                return;
            }

            Plugin.Log.Info("Creating acc dot material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            AccDotMaterial = new Material(arrowMat)
            {
                name = "NoteTweaks_AccDotMaterial",
                globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive,
                enableInstancing = true,
                shaderKeywords = arrowMat.shaderKeywords.Where(x => x != "_ENABLE_COLOR_INSTANCING").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords.Where(x => x.name != "_ENABLE_COLOR_INSTANCING").ToArray()
            };
            AccDotMaterial.SetColor(Color0, Config.AccDotColor.ColorWithAlpha(0f));

            // uncomment later maybe
            // Utils.Materials.RepairShader(AccDotDepthMaterial);
        }
        
        private static void UpdateOutlineMaterial()
        {
            if (OutlineMaterial != null)
            {
                return;
            }

            Plugin.Log.Info("Creating outline material");
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
            OutlineMaterial = new Material(arrowMat)
            {
                name = "NoteTweaks_OutlineMaterialHD",
                color = Color.black,
                renderQueue = 1990,
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE" || x != "_EMISSION").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE" || x.name != "_EMISSION").ToArray()
            };
        }
        
        private static async Task UpdateNoteMaterial()
        {
            if (NoteMaterial != null)
            {
                await Textures.LoadNoteTexture(Config.NoteTexture);
                return;
            }
            Plugin.Log.Info("Creating new note material");
            Material noteMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteHD");
            NoteMaterial = new Material(noteMat)
            {
                name = "NoteTweaks_NoteMaterial",
                renderQueue = 1995
            };

            /*Plugin.Log.Info("--- FLOATS ---");
            NoteMaterial.GetPropertyNames(MaterialPropertyType.Float).Do(x =>
            {
                Plugin.Log.Info($"{x} : {NoteMaterial.GetFloat(x)}");
            });
            Plugin.Log.Info("--- INTS ---");
            NoteMaterial.GetPropertyNames(MaterialPropertyType.Int).Do(x =>
            {
                Plugin.Log.Info($"{x} : {NoteMaterial.GetInt(x)}");
            });
            Plugin.Log.Info("--- VECTORS ---");
            NoteMaterial.GetPropertyNames(MaterialPropertyType.Vector).Do(x =>
            {
                Plugin.Log.Info($"{x} : {NoteMaterial.GetVector(x).ToString()}");
            });*/
            
            await Textures.LoadNoteTexture(Config.NoteTexture);
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
                await Textures.LoadNoteTexture(Config.BombTexture, true);
                return;
            }
            Plugin.Log.Info("Creating new bomb material");
            Material bombMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "BombNoteHD");
            BombMaterial = new Material(bombMat)
            {
                name = "NoteTweaks_BombMaterial"
            };
            
            await Textures.LoadNoteTexture(Config.BombTexture, true);
        }

        private static void UpdateRenderQueues()
        {
            if (Config.EnableAccDot)
            {
                ReplacementArrowMaterial.renderQueue = Config.RenderAccDotsAboveSymbols ? 1998 : 2000;
                ReplacementDotMaterial.renderQueue = Config.RenderAccDotsAboveSymbols ? 1998 : 2000;
                DotGlowMaterial.renderQueue = Config.RenderAccDotsAboveSymbols ? 1997 : 1999;
                ArrowGlowMaterial.renderQueue = Config.RenderAccDotsAboveSymbols ? 1997 : 1999;
            }
            else
            {
                ReplacementArrowMaterial.renderQueue = 2000;
                ReplacementDotMaterial.renderQueue = 2000;
                DotGlowMaterial.renderQueue = 1999;
                ArrowGlowMaterial.renderQueue = 1999;
            }
            
            if (Config.RenderAccDotsAboveSymbols)
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