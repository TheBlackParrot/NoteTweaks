using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using IPA.Utilities;
using ModestTree;
using NoteTweaks.UI;
using NoteTweaks.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace NoteTweaks.Managers
{
    internal static class ColorExtensions
    {
        public static Color CheckForInversion(ref this Color color, bool isBomb = false)
        {
            if (isBomb ? Plugin.Config.InvertBombTexture : Plugin.Config.InvertNoteTexture)
            {
                color.r = Mathf.Abs(color.r - 1f);
                color.g = Mathf.Abs(color.g - 1f);
                color.b = Mathf.Abs(color.b - 1f);
            }

            return color;
        }
    }
    
    internal abstract class Textures
    {
        private static readonly string[] FileExtensions = { ".png", ".jpg", ".tga" };
        private static readonly string ImagePath = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Textures", "Notes");
        
        private static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        internal static readonly Texture2D ReplacementArrowGlowTexture = OriginalArrowGlowTexture.PrepareTexture();
        
        private static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        internal static readonly Texture2D ReplacementDotGlowTexture = OriginalDotGlowTexture.PrepareTexture();

        private static readonly int NoteCubeMapID = Shader.PropertyToID("_EnvironmentReflectionCube");
        private static readonly Cubemap OriginalNoteTexture = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().First(x => x.name == "NotesReflection");
        private static Cubemap NoteTexture = OriginalNoteTexture;
        private static Cubemap BombTexture = OriginalNoteTexture;
        
        private static readonly List<KeyValuePair<string, CubemapFace>> FaceNames = new List<KeyValuePair<string, CubemapFace>>
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
            return NoteTexture.name.Split("_"[0]).Last();
        }
        public static string GetLoadedBombTexture()
        {
            return BombTexture.name.Split("_"[0]).Last();
        }

        internal static void LoadTextureChoices()
        {
            Plugin.Log.Info("Setting texture filenames for dropdown...");
            SettingsViewController.NoteTextureChoices.Clear();

            if (!Directory.Exists(ImagePath))
            {
                Directory.CreateDirectory(ImagePath);
            }
            
            string[] dirs = Directory.GetDirectories(ImagePath);
            foreach (string dir in dirs)
            {
                int count = 0;
                
                FaceNames.ForEach(pair =>
                {
                    foreach (string extension in FileExtensions)
                    {
                        string path = $"{dir}/{pair.Key}{extension}";
                        if (File.Exists(path))
                        {
                            count++;
                            break;
                        }
                    }
                });

                if (count == 6)
                {
                    SettingsViewController.NoteTextureChoices.Add(dir.Split('\\').Last());
                }
            }
            
            string[] files = Directory.GetFiles(ImagePath);
            foreach (string file in files)
            {
                if (FileExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    SettingsViewController.NoteTextureChoices.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            
            SettingsViewController.NoteTextureChoices.Sort();
            SettingsViewController.NoteTextureChoices = SettingsViewController.NoteTextureChoices.Prepend("Default").ToList();

            Plugin.Log.Info("Set texture filenames");
        }
        
        private static void OnNoteImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            NoteTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_NoteCubemap_{Plugin.Config.NoteTexture}"
            };
            
            if (textures.Any(x => x.Key == "all"))
            {
                Color[] texture = textures.Find(x => x.Key == "all").Value.GetPixels();
                texture = texture.Select(color => color.CheckForInversion()).Reverse().ToArray();
                
                FaceNames.Do(pair => NoteTexture.SetPixels(texture, pair.Value));
            }
            else
            {
                FaceNames.Do(pair =>
                {
                    Color[] texture = textures.Find(x => x.Key == pair.Key).Value.GetPixels();
                    texture = texture.Select(color => color.CheckForInversion()).Reverse().ToArray();

                    NoteTexture.SetPixels(texture, pair.Value);
                });
            }
            NoteTexture.Apply();

            Managers.Materials.NoteMaterial.mainTexture = NoteTexture;
            Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
            Managers.Materials.DebrisMaterial.mainTexture = NoteTexture;
            Managers.Materials.DebrisMaterial.SetTexture(NoteCubeMapID, NoteTexture);
        }
        
        private static void OnBombImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            BombTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_BombCubemap_{Plugin.Config.BombTexture}"
            };
            
            if (textures.Any(x => x.Key == "all"))
            {
                Color[] texture = textures.Find(x => x.Key == "all").Value.GetPixels();
                texture = texture.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
                
                FaceNames.Do(pair => BombTexture.SetPixels(texture, pair.Value));
            }
            else
            {
                FaceNames.Do(pair =>
                {
                    Color[] texture = textures.Find(x => x.Key == pair.Key).Value.GetPixels();
                    texture = texture.Select(color => color.CheckForInversion(true)).Reverse().ToArray();

                    BombTexture.SetPixels(texture, pair.Value);
                });
            }
            BombTexture.Apply();

            Managers.Materials.BombMaterial.mainTexture = BombTexture;
            Managers.Materials.BombMaterial.SetTexture(NoteCubeMapID, BombTexture);
        }

        private static void LoadDefaultNoteTexture(bool isBomb = false)
        {
            if (isBomb)
            {
                BombTexture = OriginalNoteTexture;
                Managers.Materials.BombMaterial.SetTexture(NoteCubeMapID, BombTexture);
            }
            else
            {
                NoteTexture = OriginalNoteTexture;
                Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
                Managers.Materials.DebrisMaterial.SetTexture(NoteCubeMapID, NoteTexture);
            }
        }

        internal static async Task LoadNoteTexture(string dirname, bool isBomb = false)
        {
            if (dirname == "Default")
            {
                Plugin.Log.Info("Using default note texture...");
                LoadDefaultNoteTexture(isBomb);
            }
            else
            {
                Plugin.Log.Info($"Loading texture {dirname}...");

                List<KeyValuePair<string, Texture2D>> textures = new List<KeyValuePair<string, Texture2D>>();

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
                }
                else
                {
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
                }
                
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