using System;
using System.Collections.Generic;
//using AssetBundleLoadingTools.Models.Shaders;
//using AssetBundleLoadingTools.Utilities;
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

    /*internal static class Materials
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
    }*/

    internal static class Meshes
    {
        private static Vector2 PointOnCircle(float radius, float angle, Vector2 origin)
        {
            float x = (radius * Mathf.Cos(angle * Mathf.PI / 180f)) + origin.x;
            float y = (radius * Mathf.Sin(angle * Mathf.PI / 180f)) + origin.y;

            return new Vector2(x, y);
        }
        public static Mesh GenerateFaceMesh(int sides)
        {
            // no silly gooses here
            sides = Math.Max(4, sides);
            
            Plugin.Log.Info($"wants face mesh with {sides} sides");
            
            List<Vector3> vertices = new List<Vector3> { Vector3.zero };
            for (float i = 0; i < 360f; i += 360f / sides)
            {
                vertices.Add(PointOnCircle(0.5f, i, Vector2.zero));
            }
            
            List<int> triangles = new List<int>();
            for (int i = 1; i <= sides; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1 >= sides+1 ? 1 : i + 1);
                triangles.Add(i);
            }
            
            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.Optimize();
            
            Plugin.Log.Info($"generated face mesh with {sides} sides");
            
            return mesh;
        }
    }
}