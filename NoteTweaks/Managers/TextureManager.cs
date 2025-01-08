using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
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
            NoteTexture = new Cubemap(512, textures.First().Value.format, 0);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "px").Value.GetPixels().Reverse().ToArray(), CubemapFace.PositiveX);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "py").Value.GetPixels().Reverse().ToArray(), CubemapFace.PositiveY);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "nz").Value.GetPixels().Reverse().ToArray(), CubemapFace.PositiveZ);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "nx").Value.GetPixels().Reverse().ToArray(), CubemapFace.NegativeX);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "ny").Value.GetPixels().Reverse().ToArray(), CubemapFace.NegativeY);
            NoteTexture.SetPixels(textures.Find(x => x.Key == "pz").Value.GetPixels().Reverse().ToArray(), CubemapFace.NegativeZ);
            NoteTexture.Apply();

            Managers.Materials.NoteMaterial.mainTexture = NoteTexture;
            Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
        }

        internal static async Task LoadNoteTexture(string dirname)
        {
            if (dirname == "Default" || !Directory.Exists(Path.Combine(ImagePath, dirname)))
            {
                Plugin.Log.Info("Using default note texture...");
                NoteTexture = OriginalNoteTexture;
                Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
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
                    
                    Plugin.Log.Info($"Loading texture {path}...");
                    Texture2D loadedImage = await Utilities.LoadImageAsync(path, true, false);
                    Plugin.Log.Info($"Loaded texture {path}");
                    
                    textures.Add(new KeyValuePair<string, Texture2D>(Path.GetFileNameWithoutExtension(path), loadedImage));
                }
                
                Plugin.Log.Info("Texture loaded");
                OnNoteImageLoaded(textures);
            }
        }
    }
}