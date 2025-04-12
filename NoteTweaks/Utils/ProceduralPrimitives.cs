using System;
using UnityEngine;
using JetBrains.Annotations;
using NoteTweaks.Configuration;

// https://github.com/GlitchEnzo/UnityProceduralPrimitives/blob/f78ae73f4da9ad92ebcbf47991d62f877c0b3e8c/Assets/Procedural%20Primitives/Scripts/Primitive.cs
// (c) 2015 Patrick McCarthy
// MIT License (https://github.com/GlitchEnzo/UnityProceduralPrimitives/blob/f78ae73f4da9ad92ebcbf47991d62f877c0b3e8c/LICENSE.md)

// (some changes to get rider to shut up)

namespace NoteTweaks.Utils
{
    /// <summary>
    /// Holds extension methods for a Unity <see cref="Mesh"/>.
    /// </summary>
    public static class MeshExtensions
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        private static int[] CreateIndexBuffer(int vertexCount, int indexCount, int slices)
        {
            int[] indices = new int[indexCount];
            int currentIndex = 0;

            // Bottom circle/cone of shape
            for (int i = 1; i <= slices; i++)
            {
                indices[currentIndex++] = i;
                indices[currentIndex++] = 0;
                if (i - 1 == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
            }

            // Middle sides of shape
            for (int i = 1; i < vertexCount - slices - 1; i++)
            {
                indices[currentIndex++] = i + slices;
                indices[currentIndex++] = i;
                if ((i - 1)%slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;

                indices[currentIndex++] = i;
                if ((i - 1)%slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                if ((i - 1)%slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;
            }

            // Top circle/cone of shape
            for (int i = vertexCount - slices - 1; i < vertexCount - 1; i++)
            {
                indices[currentIndex++] = i;
                if ((i - 1)%slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                indices[currentIndex++] = vertexCount - 1;
            }

            return indices;
        }
        
        /// <summary>
        /// Fills this <see cref="Mesh"/> with vertices forming a sphere.
        /// </summary>
        [UsedImplicitly]
        public static Mesh CreateSphere(float radius, int slices, int stacks, bool worldNormals = true)
        {
            slices = Math.Max(Math.Min(slices, 48), 2);
            stacks = Math.Max(Math.Min(stacks, 48), 2);
            
            Plugin.Log.Info($"Generating a sphere mesh: {slices} slices, {stacks} stacks, smooth: {Config.BombMeshSmoothNormals}, world: {worldNormals}");
            
            Mesh mesh = new Mesh
            {
                name = "SphereMesh"
            };

            float sliceStep = (float) Math.PI*2.0f/slices;
            float stackStep = (float) Math.PI/stacks;
            int vertexCount = slices*(stacks - 1) + 2;
            int triangleCount = slices*(stacks - 1)*2;
            int indexCount = triangleCount*3;

            Vector3[] sphereVertices = new Vector3[vertexCount];
            Vector3[] sphereNormals = new Vector3[vertexCount];
            //Vector2[] sphereUVs = new Vector2[vertexCount];

            int currentVertex = 0;
            sphereVertices[currentVertex] = new Vector3(0, -radius, 0);
            sphereNormals[currentVertex] = worldNormals ? Vector3.down : Vector3.one;
            currentVertex++;
            float stackAngle = (float) Math.PI - stackStep;
            for (int i = 0; i < stacks - 1; i++)
            {
                float sliceAngle = 0;
                
                for (int j = 0; j < slices; j++)
                {
                    //NOTE: y and z were switched from normal spherical coordinates because the sphere is "oriented" along the Y axis as opposed to the Z axis
                    float x = (float) (radius*Math.Sin(stackAngle)*Math.Cos(sliceAngle));
                    float y = (float) (radius*Math.Cos(stackAngle));
                    float z = (float) (radius*Math.Sin(stackAngle)*Math.Sin(sliceAngle));

                    Vector3 position = new Vector3(x, y, z);
                    
                    sphereVertices[currentVertex] = position;
                    sphereNormals[currentVertex] = worldNormals ? Vector3.Normalize(position) : Vector3.one;
                    /*sphereUVs[currentVertex] =
                        new Vector2((float) (Math.Sin(sphereNormals[currentVertex].x)/Math.PI + 0.5f),
                            (float) (Math.Sin(sphereNormals[currentVertex].y)/Math.PI + 0.5f));*/

                    currentVertex++;

                    sliceAngle += sliceStep;
                }
                
                stackAngle -= stackStep;
            }
            sphereVertices[currentVertex] = new Vector3(0, radius, 0);
            sphereNormals[currentVertex] = worldNormals ? Vector3.up : Vector3.one;

            /*mesh.vertices = sphereVertices;
            mesh.normals = sphereNormals;
            mesh.uv = sphereUVs;
            mesh.triangles = CreateIndexBuffer(vertexCount, indexCount, slices);*/
            
            int[] triangles = CreateIndexBuffer(vertexCount, indexCount, slices);
            
            Vector3[] vertices = new Vector3[triangles.Length];
            Vector3[] normals = new Vector3[triangles.Length];
            int[] fullTriangles = new int[triangles.Length];
            
            for (int i = 0; i < triangles.Length; i++)
            {
                vertices[i] = sphereVertices[triangles[i]];
                normals[i] = sphereNormals[triangles[i]];
                fullTriangles[i] = i;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = fullTriangles;
            if (Config.BombMeshSmoothNormals)
            {
                mesh.normals = normals;
            }
            else
            {
                mesh.RecalculateNormals();
            }
            
            Plugin.Log.Info($"Generated sphere mesh");

            return mesh;
        }
    }
}