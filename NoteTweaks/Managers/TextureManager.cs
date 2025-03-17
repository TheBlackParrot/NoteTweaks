using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !V1_29_1
using System.Reflection;
using System.Threading.Tasks;
#endif
using BeatSaberMarkupLanguage;
using HarmonyLib;
using IPA.Utilities;
#if !V1_29_1
using IPA.Utilities.Async;
#endif
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal static class ColorExtensions
    {
        private static PluginConfig Config => PluginConfig.Instance;
        public static Color CheckForInversion(ref this Color color, bool isBomb = false)
        {
            if (isBomb ? Config.InvertBombTexture : Config.InvertNoteTexture)
            {
                color.r = Mathf.Abs(color.r - 1f);
                color.g = Mathf.Abs(color.g - 1f);
                color.b = Mathf.Abs(color.b - 1f);
            }

            return color;
        }
    }

    internal abstract class GlowTextures
    {
        internal static PluginConfig Config => PluginConfig.Instance;
        
        internal static Texture2D ReplacementArrowGlowTexture;
        internal static Texture2D ReplacementDotGlowTexture;

        protected GlowTextures()
        {
#if V1_29_1
            LoadTextures();
#else
            UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                await LoadTextures();
            });
#endif
        }

#if V1_29_1
        internal static void LoadTextures()
#else
        internal static async Task LoadTextures()
#endif
        {
            Plugin.Log.Info("Loading glow textures...");

#if V1_29_1
            ReplacementArrowGlowTexture = Utilities.FindTextureInAssembly($"NoteTweaks.Resources.Textures.Arrow{Config.ArrowMesh}{Config.GlowTexture}.png");
#else
            ReplacementArrowGlowTexture = await Utilities.LoadTextureFromAssemblyAsync($"NoteTweaks.Resources.Textures.Arrow{Config.ArrowMesh}{Config.GlowTexture}.png");
#endif
            ReplacementArrowGlowTexture = ReplacementArrowGlowTexture.PrepareTexture();
            if (Materials.ArrowGlowMaterial != null)
            {
                Materials.ArrowGlowMaterial.mainTexture = ReplacementArrowGlowTexture;
            }
            Plugin.Log.Info("Loaded replacement arrow glow texture");
            
#if V1_29_1
            ReplacementDotGlowTexture = Utilities.FindTextureInAssembly($"NoteTweaks.Resources.Textures.Circle{Config.GlowTexture}.png");
#else
            ReplacementDotGlowTexture = await Utilities.LoadTextureFromAssemblyAsync($"NoteTweaks.Resources.Textures.Circle{Config.GlowTexture}.png");
#endif
            ReplacementDotGlowTexture = ReplacementDotGlowTexture.PrepareTexture();
            if (Materials.DotGlowMaterial != null)
            {
                Materials.DotGlowMaterial.mainTexture = ReplacementDotGlowTexture;
            }
            Plugin.Log.Info("Loaded replacement dot glow texture");
        }

#if V1_29_1
        internal static void UpdateTextures()
#else
        internal static async Task UpdateTextures()
#endif
        {
            Plugin.Log.Info("Updating glow textures...");

#if V1_29_1
            LoadTextures();
#else
            await LoadTextures();
#endif
            
            Materials.DotGlowMaterial.mainTexture = ReplacementDotGlowTexture;
            Materials.ArrowGlowMaterial.mainTexture = ReplacementArrowGlowTexture;
            
            Plugin.Log.Info("Updated glow textures...");
        }
    }
    
    internal abstract class Textures : GlowTextures
    {
        internal static readonly string[] FileExtensions = { ".png", ".jpg", ".tga" };
        internal static readonly string ImagePath = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Textures", "Notes");
        internal static readonly string[] IncludedCubemaps =
        {
            "Aberration A", "Aberration B", "Aberration C", "Aberration D", "Aberration E", "Aberration F", "Aberration G", "Aberration H", "Aberration I",
            "Dimple A", "Dimple B", "Dimple C",
            "Flat", "Flat Black", "Flat Dark", "Flat Mid",
            "Kaleido A", "Kaleido B", "Kaleido C", "Kaleido D", "Kaleido E", "Kaleido F", "Kaleido G", "Kaleido H",
            "Multicolor A", "Multicolor B", "Multicolor C", "Multicolor D", "Multicolor E",
            "Noisy",
            "Radials A", "Radials B", "Radials C", "Radials D", "Radials E", "Radials F", "Radials G", "Radials H", "Radials I", "Radials J", "Radials K",
            "Ripple A", "Ripple B", "Ripple C",
            "Soft Metallic A", "Soft Metallic B"
        };
#if PRE_V1_39_1
        private static Texture2D _originalArrowGlowTexture;
        private static Texture2D _originalDotGlowTexture;

        private static readonly int NoteCubeMapID = Shader.PropertyToID("_EnvironmentReflectionCube");
        private static Cubemap _originalNoteTexture;
        private static Cubemap _noteTexture;
        private static Cubemap _bombTexture;
#else
        private static readonly int NoteCubeMapID = Shader.PropertyToID("_EnvironmentReflectionCube");
        private static readonly Cubemap OriginalNoteTexture = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().First(x => x.name == "NotesReflection");
        private static Cubemap _noteTexture = OriginalNoteTexture;
        private static Cubemap _bombTexture = OriginalNoteTexture;
#endif

        internal static readonly List<KeyValuePair<string, CubemapFace>> FaceNames = new List<KeyValuePair<string, CubemapFace>>
        {
            new KeyValuePair<string, CubemapFace>("px", CubemapFace.PositiveX),
            new KeyValuePair<string, CubemapFace>("py", CubemapFace.PositiveY),
            new KeyValuePair<string, CubemapFace>("nz", CubemapFace.PositiveZ),
            new KeyValuePair<string, CubemapFace>("nx", CubemapFace.NegativeX),
            new KeyValuePair<string, CubemapFace>("ny", CubemapFace.NegativeY),
            new KeyValuePair<string, CubemapFace>("pz", CubemapFace.NegativeZ),
        };

        public static string GetLoadedNoteTexture()
        {
            return _noteTexture.name.Split("_"[0]).Last();
        }
        public static string GetLoadedBombTexture()
        {
            return _bombTexture.name.Split("_"[0]).Last();
        }

        protected Textures()
        {
            if (!Directory.Exists(ImagePath))
            {
                Directory.CreateDirectory(ImagePath);
            }
        }

#if PRE_V1_39_1
        internal static void SetDefaultTextures()
        {
            _originalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
            ReplacementArrowGlowTexture = _originalArrowGlowTexture.PrepareTexture();
            
            _originalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
            ReplacementDotGlowTexture = _originalDotGlowTexture.PrepareTexture();
            
            _originalNoteTexture = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().First(x => x.name == "NotesReflection");
            _noteTexture = _originalNoteTexture;
            _bombTexture = _originalNoteTexture;
        }
#endif
        
        private static void OnNoteImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            _noteTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_NoteCubemap_{Config.NoteTexture}"
            };
            
            if (textures.Any(x => x.Key == "all"))
            {
                Color[] texture = textures.Find(x => x.Key == "all").Value.GetPixels();
                texture = texture.Select(color => color.CheckForInversion()).Reverse().ToArray();
                
                FaceNames.Do(pair => _noteTexture.SetPixels(texture, pair.Value));
            }
            else
            {
                FaceNames.Do(pair =>
                {
                    Color[] texture = textures.Find(x => x.Key == pair.Key).Value.GetPixels();
                    texture = texture.Select(color => color.CheckForInversion()).Reverse().ToArray();

                    _noteTexture.SetPixels(texture, pair.Value);
                });
            }
            _noteTexture.Apply();

            Materials.NoteMaterial.mainTexture = _noteTexture;
            Materials.NoteMaterial.SetTexture(NoteCubeMapID, _noteTexture);
            Materials.DebrisMaterial.mainTexture = _noteTexture;
            Materials.DebrisMaterial.SetTexture(NoteCubeMapID, _noteTexture);
        }
        
        private static void OnBombImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            _bombTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_BombCubemap_{Config.BombTexture}"
            };
            
            if (textures.Any(x => x.Key == "all"))
            {
                Color[] texture = textures.Find(x => x.Key == "all").Value.GetPixels();
                texture = texture.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
                
                FaceNames.Do(pair => _bombTexture.SetPixels(texture, pair.Value));
            }
            else
            {
                FaceNames.Do(pair =>
                {
                    Color[] texture = textures.Find(x => x.Key == pair.Key).Value.GetPixels();
                    texture = texture.Select(color => color.CheckForInversion(true)).Reverse().ToArray();

                    _bombTexture.SetPixels(texture, pair.Value);
                });
            }
            _bombTexture.Apply();

            Materials.BombMaterial.mainTexture = _bombTexture;
            Materials.BombMaterial.SetTexture(NoteCubeMapID, _bombTexture);
        }

        private static void LoadDefaultNoteTexture(bool isBomb = false)
        {
            if (isBomb)
            {
#if PRE_V1_39_1
                _bombTexture = _originalNoteTexture;
#else
                _bombTexture = OriginalNoteTexture;
#endif
                Materials.BombMaterial.SetTexture(NoteCubeMapID, _bombTexture);
            }
            else
            {
#if PRE_V1_39_1
                _noteTexture = _originalNoteTexture;
#else
                _noteTexture = OriginalNoteTexture;
#endif
                Materials.NoteMaterial.SetTexture(NoteCubeMapID, _noteTexture);
                Materials.DebrisMaterial.SetTexture(NoteCubeMapID, _noteTexture);
            }
        }

#if V1_29_1
        internal static void LoadNoteTexture(string dirname, bool isBomb = false, bool forceRefresh = false)
#else
        internal static async Task LoadNoteTexture(string dirname, bool isBomb = false, bool forceRefresh = false)
#endif
        {
            if (!forceRefresh)
            {
                // wait... you can do that? thanks rider for teaching me things
                switch (isBomb)
                {
                    case false when GetLoadedNoteTexture() == Config.NoteTexture:
                    case true when GetLoadedBombTexture() == Config.BombTexture:
                        return;
                }
            }

            if (dirname == "Default")
            {
                Plugin.Log.Info("Using default note texture...");
                LoadDefaultNoteTexture(isBomb);
            }
            else
            {
                Plugin.Log.Info($"Loading texture {dirname}...");

                List<KeyValuePair<string, Texture2D>> textures = new List<KeyValuePair<string, Texture2D>>();
                
                if (IncludedCubemaps.Contains(dirname))
                {
#if V1_29_1
                    Texture2D loadedImage = Utilities.FindTextureInAssembly($"NoteTweaks.Resources.Textures.CubemapSingles.{dirname}.png");
#else
                    // LoadTextureFromAssemblyAsync doesn't have a flag to allow this to continue to be readable, so we have to load it this way instead
                    Texture2D loadedImage = await Utilities.LoadImageAsync(Utilities.GetResourceAsync(Assembly.GetExecutingAssembly(), $"NoteTweaks.Resources.Textures.CubemapSingles.{dirname}.png").Result, false, false);
#endif
                    textures.Add(new KeyValuePair<string, Texture2D>("all", loadedImage));

                    goto done;
                }

                string singleFileFilename = null;
                foreach (string extension in FileExtensions)
                {
                    string path = Path.Combine(ImagePath, dirname + extension);
                    if (File.Exists(path))
                    {
                        singleFileFilename = path;
                        break;
                    }
                }
                
                if (singleFileFilename != null)
                {
#if V1_29_1
                    Texture2D loadedImage = Utilities.LoadTextureRaw(File.ReadAllBytes(singleFileFilename));
#else
                    Texture2D loadedImage = await Utilities.LoadImageAsync(singleFileFilename, true, false);
#endif
                    textures.Add(new KeyValuePair<string, Texture2D>("all", loadedImage));

                    goto done;
                }
                    
                foreach (string faceFilename in FaceNames.Select(pair => pair.Key))
                {
                    string path = null;
                    bool foundImage = false;
                    foreach (string extension in FileExtensions)
                    {
                        path = Path.Combine(ImagePath, dirname, faceFilename + extension);
                        if (File.Exists(path))
                        {
                            foundImage = true;
                            break;
                        }
                    }

                    if (!foundImage)
                    {
                        Plugin.Log.Info($"{dirname} does not have all required images (px, py, pz, nx, ny, nz)");
                        LoadDefaultNoteTexture(isBomb);
                        return;
                    }
                    
#if V1_29_1
                    Texture2D loadedImage = Utilities.LoadTextureRaw(File.ReadAllBytes(path));
#else
                    Texture2D loadedImage = await Utilities.LoadImageAsync(path, true, false);
#endif
                    
                    textures.Add(new KeyValuePair<string, Texture2D>(Path.GetFileNameWithoutExtension(path), loadedImage));
                }   
                
                done:
                    Plugin.Log.Info("Textures loaded");

                    if (isBomb)
                    {
                        OnBombImageLoaded(textures);
                    }
                    else
                    {
                        OnNoteImageLoaded(textures);
                    }
            }
        }
    }
}