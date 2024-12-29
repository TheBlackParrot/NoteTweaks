using System;
using AssetBundleLoadingTools.Models.Shaders;
using AssetBundleLoadingTools.Utilities;
using UnityEngine;

namespace NoteTweaks.Utils
{
    internal static class Textures
    {
        // thanks qq
        // https://github.com/qqrz997/CustomSabersLite/blob/efb2921f5cb8c4a69e111bdb3606b9f3f91fc06f/CustomSabers/Utilities/Extensions/TextureExtensions.cs#L10-L22
        // discovered this was pretty much what Graphics.ConvertTexture was doing, so ty for pointing me in the right direction uwu
        public static Texture2D PrepareTexture(this Texture2D source)
        {
            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, false);
            Graphics.ConvertTexture(source, readableTexture);
            readableTexture.name = source.name + "-Duplicate";
            return readableTexture;
        }
    }

    internal static class Materials
    {
        public static void RepairShader(Material material)
        {
            ShaderReplacementInfo info = ShaderRepair.FixShaderOnMaterial(material);
            
            if (!info.AllShadersReplaced)
            {
                Plugin.Log.Info("Could not repair shaders:");
                foreach (String name in info.MissingShaderNames)
                {
                    Plugin.Log.Info($"\t - {name}");
                }
            }
        }
    }
}