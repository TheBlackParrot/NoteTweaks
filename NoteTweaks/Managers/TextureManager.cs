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
        private static readonly string[] CubemapFaceFilenames = { "px", "py", "pz", "nx", "ny", "nz" };
        private static readonly string ImagePath = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Textures", "Notes");
        
        private static readonly Texture2D OriginalArrowGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "ArrowGlow");
        internal static readonly Texture2D ReplacementArrowGlowTexture = OriginalArrowGlowTexture.PrepareTexture();
        
        private static readonly Texture2D OriginalDotGlowTexture = Resources.FindObjectsOfTypeAll<Texture2D>().ToList().First(x => x.name == "NoteCircleBakedGlow");
        internal static readonly Texture2D ReplacementDotGlowTexture = OriginalDotGlowTexture.PrepareTexture();

        private static readonly int NoteCubeMapID = Shader.PropertyToID("_EnvironmentReflectionCube");
        private static readonly Cubemap OriginalNoteTexture = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().First(x => x.name == "NotesReflection");
        private static Cubemap NoteTexture = OriginalNoteTexture;
        private static Cubemap BombTexture = OriginalNoteTexture;

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
            SettingsViewController.NoteTextureChoices.Add("Default");

            if (!Directory.Exists(ImagePath))
            {
                Directory.CreateDirectory(ImagePath);
            }
            
            string[] dirs = Directory.GetDirectories(ImagePath);
            foreach (string dir in dirs)
            {
                int count = 0;
                
                foreach (string wantedFilename in CubemapFaceFilenames)
                {
                    foreach (string extension in FileExtensions)
                    {
                        string path = $"{dir}/{wantedFilename}{extension}";
                        if (File.Exists(path))
                        {
                            count++;
                            break;
                        }
                    }
                }

                if (count == 6)
                {
                    SettingsViewController.NoteTextureChoices.Add(dir.Split('\\').Last());
                }
            }

            Plugin.Log.Info("Set texture filenames");
        }

        private static void OnNoteImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            Color[] px = textures.Find(x => x.Key == "px").Value.GetPixels();
            px = px.Select(color => color.CheckForInversion()).Reverse().ToArray();
            Color[] py = textures.Find(x => x.Key == "py").Value.GetPixels();
            py = py.Select(color => color.CheckForInversion()).Reverse().ToArray();
            Color[] pz = textures.Find(x => x.Key == "pz").Value.GetPixels();
            pz = pz.Select(color => color.CheckForInversion()).Reverse().ToArray();
            Color[] nx = textures.Find(x => x.Key == "nx").Value.GetPixels();
            nx = nx.Select(color => color.CheckForInversion()).Reverse().ToArray();
            Color[] ny = textures.Find(x => x.Key == "ny").Value.GetPixels();
            ny = ny.Select(color => color.CheckForInversion()).Reverse().ToArray();
            Color[] nz = textures.Find(x => x.Key == "nz").Value.GetPixels();
            nz = nz.Select(color => color.CheckForInversion()).Reverse().ToArray();
            
            NoteTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_NoteCubemap_{Plugin.Config.NoteTexture}"
            };
            NoteTexture.SetPixels(px, CubemapFace.PositiveX);
            NoteTexture.SetPixels(py, CubemapFace.PositiveY);
            NoteTexture.SetPixels(nz, CubemapFace.PositiveZ);
            NoteTexture.SetPixels(nx, CubemapFace.NegativeX);
            NoteTexture.SetPixels(ny, CubemapFace.NegativeY);
            NoteTexture.SetPixels(pz, CubemapFace.NegativeZ);
            NoteTexture.Apply();

            Managers.Materials.NoteMaterial.mainTexture = NoteTexture;
            Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
            Managers.Materials.DebrisMaterial.mainTexture = NoteTexture;
            Managers.Materials.DebrisMaterial.SetTexture(NoteCubeMapID, NoteTexture);
        }
        
        private static void OnBombImageLoaded(List<KeyValuePair<string, Texture2D>> textures)
        {
            Color[] px = textures.Find(x => x.Key == "px").Value.GetPixels();
            px = px.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            Color[] py = textures.Find(x => x.Key == "py").Value.GetPixels();
            py = py.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            Color[] pz = textures.Find(x => x.Key == "pz").Value.GetPixels();
            pz = pz.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            Color[] nx = textures.Find(x => x.Key == "nx").Value.GetPixels();
            nx = nx.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            Color[] ny = textures.Find(x => x.Key == "ny").Value.GetPixels();
            ny = ny.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            Color[] nz = textures.Find(x => x.Key == "nz").Value.GetPixels();
            nz = nz.Select(color => color.CheckForInversion(true)).Reverse().ToArray();
            
            BombTexture = new Cubemap(512, textures.First().Value.format, 0)
            {
                name = $"NoteTweaks_BombCubemap_{Plugin.Config.BombTexture}"
            };
            BombTexture.SetPixels(px, CubemapFace.PositiveX);
            BombTexture.SetPixels(py, CubemapFace.PositiveY);
            BombTexture.SetPixels(nz, CubemapFace.PositiveZ);
            BombTexture.SetPixels(nx, CubemapFace.NegativeX);
            BombTexture.SetPixels(ny, CubemapFace.NegativeY);
            BombTexture.SetPixels(pz, CubemapFace.NegativeZ);
            BombTexture.Apply();

            Managers.Materials.BombMaterial.mainTexture = BombTexture;
            Managers.Materials.BombMaterial.SetTexture(NoteCubeMapID, BombTexture);
        }

        internal static async Task LoadNoteTexture(string dirname, bool isBomb = false)
        {
            if (dirname == "Default" || !Directory.Exists(Path.Combine(ImagePath, dirname)))
            {
                Plugin.Log.Info("Using default note texture...");
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
            else
            {
                Plugin.Log.Info($"Loading textures in directory {dirname}...");

                List<KeyValuePair<string, Texture2D>> textures = new List<KeyValuePair<string, Texture2D>>();
                foreach (string faceFilename in CubemapFaceFilenames)
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
                        break;
                    }
                    
                    //Plugin.Log.Info($"Loading texture {path}...");
                    Texture2D loadedImage = await Utilities.LoadImageAsync(path, true, false);
                    //Plugin.Log.Info($"Loaded texture {path}");
                    
                    textures.Add(new KeyValuePair<string, Texture2D>(Path.GetFileNameWithoutExtension(path), loadedImage));
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