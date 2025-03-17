using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if !V1_29_1
using System.Threading.Tasks;
#endif
using HarmonyLib;
using NoteTweaks.Configuration;
using UnityEngine;
#if PRE_V1_39_1 && !V1_29_1
using UnityEngine.Rendering;
#endif

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

#if !V1_29_1
        private static readonly int MainEffectContainerID = Resources.FindObjectsOfTypeAll<BoolSO>().First(x => x.name.StartsWith("MainEffectContainer.")).GetInstanceID();
        internal static BoolSO MainEffectContainer => Resources.InstanceIDToObject(MainEffectContainerID) as BoolSO;
#else
        internal static BoolSO MainEffectContainer = Resources.FindObjectsOfTypeAll<BoolSO>().First(x => x.name.StartsWith("MainEffectContainer."));
#endif
        
        internal static float SaneAlphaValue => MainEffectContainer.value ? 1f : 0f;
        
#if PRE_V1_39_1
        private static string MaterialIdentifier => MainEffectContainer.value ? "HD" : "LW";
#endif
        
        internal static readonly int BlendOpID = Shader.PropertyToID("_BlendOp");
        private static readonly int CutoutTexScaleID = Shader.PropertyToID("_CutoutTexScale");
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
        
#if V1_29_1
        internal static void UpdateMainEffectContainerWorkaroundThing()
        {
            // Unity 2019 doesn't have Resources.InstanceIDToObject, so we just. do it this way. i guess
            MainEffectContainer = Resources.FindObjectsOfTypeAll<BoolSO>().First(x => x.name.StartsWith("MainEffectContainer."));
        }
#endif

#if V1_29_1
        internal static void UpdateAll()
#else
        internal static async Task UpdateAll()
#endif
        {
            UpdateDebrisMaterial();
            UpdateReplacementDotMaterial();
            UpdateReplacementArrowMaterial();
            UpdateAccDotMaterial();
            UpdateOutlineMaterial();
            
#if V1_29_1
            UpdateDotGlowMaterial();
            UpdateArrowGlowMaterial();
            UpdateNoteMaterial();
            UpdateBombMaterial();
#else
            await UpdateDotGlowMaterial();
            await UpdateArrowGlowMaterial();
            await UpdateNoteMaterial();
            await UpdateBombMaterial();
#endif
            
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
#if PRE_V1_39_1
            string wantedMaterialName = $"NoteTweaks_ReplacementDotMaterial{MaterialIdentifier}";
            
            if (ReplacementDotMaterial != null)
            {
                if (Resources.FindObjectsOfTypeAll<Material>().Any(x => x.name == wantedMaterialName))
                {
                    return;   
                }
            }
#else
            if (ReplacementDotMaterial != null)
            {
                return;
            }
#endif

            Plugin.Log.Info("Creating replacement dot material");
#if PRE_V1_39_1
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == $"NoteArrow{MaterialIdentifier}");
#else
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
#endif
            
            ReplacementDotMaterial = new Material(arrowMat)
            {
#if PRE_V1_39_1
                name = wantedMaterialName,
#else
                name = "NoteTweaks_ReplacementDotMaterial",
#endif
                color = Color.white,
#if V1_29_1
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE" || x != "_EMISSION").ToArray()
#else
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE").ToArray()
#endif
            };
            
            ReplacementDotMaterial.SetInt(SrcFactorID, SrcFactor);
            ReplacementDotMaterial.SetInt(DstFactorID, DstFactor);
            ReplacementDotMaterial.SetInt(SrcFactorAlphaID, SrcFactorAlpha);
            ReplacementDotMaterial.SetInt(DstFactorAlphaID, DstFactorAlpha);
        }

        private static void UpdateReplacementArrowMaterial()
        {
#if PRE_V1_39_1
            string wantedMaterialName = $"NoteTweaks_ReplacementArrowMaterial{MaterialIdentifier}";
            
            if (ReplacementArrowMaterial != null)
            {
                if (Resources.FindObjectsOfTypeAll<Material>().Any(x => x.name == wantedMaterialName))
                {
                    return;   
                }
            }
#else
            if (ReplacementArrowMaterial != null)
            {
                return;
            }
#endif

            Plugin.Log.Info("Creating replacement arrow material");
#if PRE_V1_39_1
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == $"NoteArrow{MaterialIdentifier}");
#else
            Material arrowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowHD");
#endif
            
            ReplacementArrowMaterial = new Material(arrowMat)
            {
#if PRE_V1_39_1
                name = wantedMaterialName,
#else
                name = "NoteTweaks_ReplacementArrowMaterial",
#endif
                color = Color.white,
#if V1_29_1
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE" || x != "_EMISSION").ToArray()
#else
                shaderKeywords = arrowMat.shaderKeywords
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE").ToArray()
#endif
            };
            
            ReplacementDotMaterial.SetInt(SrcFactorID, SrcFactor);
            ReplacementDotMaterial.SetInt(DstFactorID, DstFactor);
            ReplacementDotMaterial.SetInt(SrcFactorAlphaID, MainEffectContainer.value && Config.AddBloomForOutlines ? 1 : SrcFactorAlpha);
            ReplacementDotMaterial.SetInt(DstFactorAlphaID, DstFactorAlpha);
        }

#if V1_29_1
        private static void UpdateDotGlowMaterial()
        {
            if (GlowTextures.ReplacementDotGlowTexture == null)
            {
                GlowTextures.LoadTextures();
            }
            
            if (DotGlowMaterial != null)
            {
                GlowTextures.UpdateTextures();
                DotGlowMaterial.mainTexture = GlowTextures.ReplacementDotGlowTexture;
                return;
            }
#else
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
#endif
            
            Plugin.Log.Info("Creating new dot glow material");
            Material arrowGlowMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteArrowGlow");
            DotGlowMaterial = new Material(arrowGlowMat)
            {
                name = "NoteTweaks_DotGlowMaterial",
                mainTexture = GlowTextures.ReplacementDotGlowTexture
            };
        }

#if V1_29_1
        private static void UpdateArrowGlowMaterial()
        {
            if (GlowTextures.ReplacementArrowGlowTexture == null)
            {
                GlowTextures.LoadTextures();
            }
            
            if (ArrowGlowMaterial != null)
            {
                GlowTextures.UpdateTextures();
                return;
            }
#else
        private static async Task UpdateArrowGlowMaterial()
        {
            if (GlowTextures.ReplacementArrowGlowTexture == null)
            {
                await GlowTextures.LoadTextures();
            }
            
            if (ArrowGlowMaterial != null)
            {
                await GlowTextures.UpdateTextures();
                return;
            }
#endif
            
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
#if !V1_29_1
                enabledKeywords = arrowMat.enabledKeywords.Where(x => x.name != "_ENABLE_COLOR_INSTANCING").ToArray()
#endif
            };

            // uncomment later maybe
            // Utils.Materials.RepairShader(AccDotDepthMaterial);
        }
        
#if PRE_V1_39_1
        private static Cubemap _blankCubemap;
        private static readonly int EnvironmentReflectionCubeID = Shader.PropertyToID("_EnvironmentReflectionCube");
        internal static readonly int FinalColorMul = Shader.PropertyToID("_FinalColorMul");
#endif

        private static void UpdateOutlineMaterial()
        {
#if PRE_V1_39_1
            string wantedMaterialName = $"NoteTweaks_OutlineMaterial{MaterialIdentifier}";
            if (OutlineMaterial != null)
            {
                if (Resources.FindObjectsOfTypeAll<Material>().Any(x => x.name == wantedMaterialName))
                {
                    return;   
                }
            }
            
            if (_blankCubemap == null)
            {
                _blankCubemap = new Cubemap(512, TextureFormat.RGBA32, false);
                
                Color[] pixels = new Color[262144];
                for (int i = 0; i < 262144; i++)
                {
                    pixels[i] = Color.white;
                }
                
                for (int i = 0; i < 6; i++)
                {
                    _blankCubemap.SetPixels(pixels, (CubemapFace)i);
                }
                
                _blankCubemap.Apply();
            }

            Plugin.Log.Info("Creating outline material");
            
            Material noteMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == $"Note{MaterialIdentifier}");
            string[] keywords = noteMat.shaderKeywords
                .Where(x => x != "_CUTOUT_NONE" ||
                            x != "_ENABLE_RIM_DIM" || x != "_ENABLE_RIM_COLOR").ToArray();
    #if !V1_29_1
            LocalKeyword[] enabledKeywords = noteMat.enabledKeywords
                .Where(x => x.name != "_CUTOUT_NONE" ||
                            x.name != "_ENABLE_RIM_DIM" || x.name != "_ENABLE_RIM_COLOR")
                .ToArray();
    #endif
            
            OutlineMaterial = new Material(noteMat)
            {
                name = wantedMaterialName,
                color = Color.white,
                shaderKeywords = keywords,
    #if !V1_29_1
                enabledKeywords = enabledKeywords,
    #endif
                renderQueue = 1990
            };
            
            new List<KeyValuePair<string, float>>
            {
                new KeyValuePair<string, float>("_CullMode", 1),
                new KeyValuePair<string, float>("_EnableRimDim", 0),
                new KeyValuePair<string, float>("_RimDarkening", 0),
                new KeyValuePair<string, float>("_RimScale", 0),
                new KeyValuePair<string, float>("_Smoothness", 0),
                new KeyValuePair<string, float>("_WhiteBoostType", 0)
            }.Do(pair => OutlineMaterial.SetFloat(pair.Key, pair.Value));
            
            OutlineMaterial.SetInt(FinalColorMul, -1);
            OutlineMaterial.SetTexture(EnvironmentReflectionCubeID, _blankCubemap);
#else
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
                    .Where(x => x != "_ENABLE_COLOR_INSTANCING" || x != "_CUTOUT_NONE").ToArray(),
                enabledKeywords = arrowMat.enabledKeywords
                    .Where(x => x.name != "_ENABLE_COLOR_INSTANCING" || x.name != "_CUTOUT_NONE").ToArray()
            };
#endif
            
            OutlineMaterial.SetFloat(CutoutTexScaleID, 0.5f);
            OutlineMaterial.SetInt(SrcFactorAlphaID, MainEffectContainer.value && Config.AddBloomForOutlines ? 1 : 0);
        }
        
#if V1_29_1
        internal static void UpdateNoteMaterial()
#else
        private static async Task UpdateNoteMaterial()
#endif
        {
#if PRE_V1_39_1
            string wantedMaterialName = $"NoteTweaks_NoteMaterial{MaterialIdentifier}";
            
            if (NoteMaterial != null)
            {
                if (Resources.FindObjectsOfTypeAll<Material>().Any(x => x.name == wantedMaterialName))
                {
    #if V1_29_1
                    Textures.LoadNoteTexture(Config.NoteTexture);
    #else
                    await Textures.LoadNoteTexture(Config.NoteTexture);
    #endif
                    return;   
                }
            }
            
            Plugin.Log.Info("Creating new note material");
            Material noteMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == $"Note{MaterialIdentifier}");
            NoteMaterial = new Material(noteMat)
            {
                name = wantedMaterialName,
                renderQueue = 1995
            };
#else
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
#endif
            
#if V1_29_1
            if (Textures.GetLoadedNoteTexture() != Config.NoteTexture)
            {
                Textures.LoadNoteTexture(Config.NoteTexture);
            }
#else
            await Textures.LoadNoteTexture(Config.NoteTexture);
#endif
        }
        
        private static void UpdateDebrisMaterial()
        {
            if (DebrisMaterial != null)
            {
                return;
            }
            
            Plugin.Log.Info("Creating new debris material");
            
            // there's no NoteDebrisLW. nice one beat games
            Material debrisMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "NoteDebrisHD");
            DebrisMaterial = new Material(debrisMat)
            {
                name = "NoteTweaks_DebrisMaterial",
                renderQueue = 1995
            };
        }
        
#if V1_29_1
        internal static void UpdateBombMaterial()
#else
        private static async Task UpdateBombMaterial()
#endif
        {
#if PRE_V1_39_1
            string wantedMaterialName = $"NoteTweaks_BombMaterial{MaterialIdentifier}";
            
            if (BombMaterial != null)
            {
                if (Resources.FindObjectsOfTypeAll<Material>().Any(x => x.name == wantedMaterialName))
                {
    #if V1_29_1
                    Textures.LoadNoteTexture(Config.BombTexture, true);
    #else
                    await Textures.LoadNoteTexture(Config.BombTexture, true);
    #endif
                    return;   
                }
            }
#else
            if (BombMaterial != null)
            {
                await Textures.LoadNoteTexture(Config.BombTexture, true);
                return;
            }
#endif
            
            Plugin.Log.Info("Creating new bomb material");
#if PRE_V1_39_1
            Material bombMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == $"BombNote{MaterialIdentifier}");
#else
            Material bombMat = Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "BombNoteHD");
#endif
            
            BombMaterial = new Material(bombMat)
            {
#if PRE_V1_39_1
                name = wantedMaterialName
#else
                name = "NoteTweaks_BombMaterial"
#endif
            };

#if V1_29_1
            Textures.LoadNoteTexture(Config.BombTexture, true);
#else
            await Textures.LoadNoteTexture(Config.BombTexture, true);
#endif
        }

        internal static void UpdateRenderQueues()
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