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
            
            string[] files = Directory.GetFiles(ImagePath);
            foreach (string file in files)
            {
                if (FileExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    Log.Info("\tAdding " + Path.GetFileName(file));
                    SettingsViewController.NoteTextureChoices.Add(Path.GetFileName(file));
                }
            }

            Plugin.Log.Info("Set texture filenames");
        }

        private static void OnNoteImageLoaded(Texture2D tempTex)
        {
            NoteTexture = new Cubemap(tempTex.width, tempTex.format, 0);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.PositiveX);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.PositiveY);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.PositiveZ);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.NegativeX);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.NegativeY);
            NoteTexture.SetPixels(tempTex.GetPixels(), CubemapFace.NegativeZ);
            NoteTexture.Apply();

            Managers.Materials.NoteMaterial.mainTexture = NoteTexture;
            Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
        }

        internal static async Task LoadNoteTexture(string filename)
        {
            if (filename == "Default" || !File.Exists(Path.Combine(ImagePath, filename)))
            {
                Plugin.Log.Info("Using default note texture...");
                NoteTexture = OriginalNoteTexture;
                Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
            }
            else
            {
                Plugin.Log.Info($"Loading texture {filename}...");

                Texture2D loadedImage = await Utilities.LoadImageAsync(Path.Combine(ImagePath, filename), true, false);
                Plugin.Log.Info("Texture loaded");
                OnNoteImageLoaded(loadedImage);
            }
        }
    }
}