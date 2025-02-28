﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using IPA.Utilities;
using IPA.Utilities.Async;
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
            UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                await LoadTextures();
            });
        }

        internal static async Task LoadTextures()
        {
            Plugin.Log.Info("Loading glow textures...");
            
            ReplacementArrowGlowTexture = await Utilities.LoadTextureFromAssemblyAsync($"NoteTweaks.Resources.Textures.Arrow{Config.ArrowMesh}{Config.GlowTexture}.png");
            ReplacementArrowGlowTexture.PrepareTexture();
            if (Materials.ArrowGlowMaterial != null)
            {
                Materials.ArrowGlowMaterial.mainTexture = ReplacementArrowGlowTexture;
            }
            Plugin.Log.Info("Loaded replacement arrow glow texture");
            
            ReplacementDotGlowTexture = await Utilities.LoadTextureFromAssemblyAsync($"NoteTweaks.Resources.Textures.Circle{Config.GlowTexture}.png");
            ReplacementDotGlowTexture.PrepareTexture();
            if (Materials.DotGlowMaterial != null)
            {
                Materials.DotGlowMaterial.mainTexture = ReplacementDotGlowTexture;
            }
            Plugin.Log.Info("Loaded replacement dot glow texture");
        }

        internal static async Task UpdateTextures()
        {
            Plugin.Log.Info("Updating glow textures...");

            await LoadTextures();
            
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
            "Abberation A", "Abberation B", "Abberation C", "Abberation D", "Abberation E", "Abberation F", "Abberation G", "Abberation H", "Abberation I",
            "Dimple A", "Dimple B", "Dimple C",
            "Flat", "Flat Black", "Flat Dark", "Flat Mid",
            "Kaleido A", "Kaleido B", "Kaleido C", "Kaleido D", "Kaleido E", "Kaleido F", "Kaleido G", "Kaleido H",
            "Multicolor A", "Multicolor B", "Multicolor C", "Multicolor D", "Multicolor E",
            "Noisy",
            "Radials A", "Radials B", "Radials C", "Radials D", "Radials E", "Radials F", "Radials G", "Radials H", "Radials I", "Radials J", "Radials K",
            "Ripple A", "Ripple B", "Ripple C",
            "Soft Metallic A", "Soft Metallic B"
        };

        private static readonly int NoteCubeMapID = Shader.PropertyToID("_EnvironmentReflectionCube");
        private static readonly Cubemap OriginalNoteTexture = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().First(x => x.name == "NotesReflection");
        private static Cubemap _noteTexture = OriginalNoteTexture;
        private static Cubemap _bombTexture = OriginalNoteTexture;

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
                _bombTexture = OriginalNoteTexture;
                Materials.BombMaterial.SetTexture(NoteCubeMapID, _bombTexture);
            }
            else
            {
                _noteTexture = OriginalNoteTexture;
                Materials.NoteMaterial.SetTexture(NoteCubeMapID, _noteTexture);
                Materials.DebrisMaterial.SetTexture(NoteCubeMapID, _noteTexture);
            }
        }

        internal static async Task LoadNoteTexture(string dirname, bool isBomb = false, bool forceRefresh = false)
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
                    // LoadTextureFromAssemblyAsync doesn't have a flag to allow this to continue to be readable, so we have to load it this way instead
                    Texture2D loadedImage = await Utilities.LoadImageAsync(Utilities.GetResourceAsync(Assembly.GetExecutingAssembly(), $"NoteTweaks.Resources.Textures.CubemapSingles.{dirname}.png").Result, false, false);
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
                    Texture2D loadedImage = await Utilities.LoadImageAsync(singleFileFilename, true, false);
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
                    
                    //Plugin.Log.Info($"Loading texture {path}...");
                    Texture2D loadedImage = await Utilities.LoadImageAsync(path, true, false);
                    //Plugin.Log.Info($"Loaded texture {path}");
                    
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