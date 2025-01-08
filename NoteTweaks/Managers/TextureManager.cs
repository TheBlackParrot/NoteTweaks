using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using ModestTree;
using NoteTweaks.UI;
using NoteTweaks.Utils;
using UnityEngine;

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
            Plugin.Log.Info("Loaded texture");

            Color[] pixels = tempTex.GetPixels();
            NoteTexture.SetPixels(pixels, CubemapFace.PositiveX);
            NoteTexture.SetPixels(pixels, CubemapFace.NegativeX);
            NoteTexture.SetPixels(pixels, CubemapFace.PositiveY);
            NoteTexture.SetPixels(pixels, CubemapFace.NegativeY);
            NoteTexture.SetPixels(pixels, CubemapFace.PositiveZ);
            NoteTexture.SetPixels(pixels, CubemapFace.NegativeZ);

            Managers.Materials.NoteMaterial.mainTexture = NoteTexture;
            Managers.Materials.NoteMaterial.SetTexture(NoteCubeMapID, NoteTexture);
            
            Transform noteContainer = NotePreviewViewController.NoteContainer.transform;
            for (int i = 0; i < noteContainer.childCount; i++)
            {
                GameObject noteCube = noteContainer.GetChild(i).gameObject;
                foreach (MaterialPropertyBlockController controller in noteCube.GetComponents<MaterialPropertyBlockController>())
                {
                    controller.materialPropertyBlock.SetTexture(NoteCubeMapID, NoteTexture);
                    controller.ApplyChanges();
                }
            }
        }

        internal static void LoadNoteTexture(string filename)
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

                Utilities.LoadImageAsync(Path.Combine(ImagePath, filename), true, false).ContinueWith(task =>
                {
                    OnNoteImageLoaded(task.Result);
                });
            }
        }
    }
}