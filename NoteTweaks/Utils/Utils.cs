using System;
using System.Collections.Generic;
//using AssetBundleLoadingTools.Models.Shaders;
//using AssetBundleLoadingTools.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

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

    internal static class Meshes
    {
        private static Vector2 PointOnCircle(float radius, float angle, Vector2 origin)
        {
            float x = (radius * Mathf.Cos(angle * Mathf.PI / 180f)) + origin.x;
            float y = (radius * Mathf.Sin(angle * Mathf.PI / 180f)) + origin.y;

            return new Vector2(x, y);
        }
        public static Mesh GenerateFaceMesh(int sides, float size = 1f)
        {
            // no silly gooses here
            sides = Math.Max(4, sides);
            
            Plugin.Log.Info($"wants face mesh with {sides} sides");
            
            List<Vector3> vertices = new List<Vector3> { Vector3.zero };
            for (float i = 0; i < 360f; i += 360f / sides)
            {
                vertices.Add(PointOnCircle(0.5f * size, i, Vector2.zero));
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

        public static Mesh GenerateBasicTriangleMesh()
        {
            return GenerateTriangleMesh(new Vector2(0.3f, 0.0933f), new Vector2(0f, 0.0033f), new Vector3(0f, 0f, 180f));
        }

        private static Mesh GenerateTriangleMesh(Vector2 size, Vector2 offset = default, Vector3 rotation = default)
        {
            Quaternion newRotation = new Quaternion
            {
                eulerAngles = rotation
            };
            
            float negativeX = (-1f * (size.x / 2f)) + offset.x;
            float positiveX = (size.x / 2f) + offset.x;
            float negativeY = (-1f * (size.y / 2f)) + offset.y;
            float positiveY = (size.y / 2f) + offset.y;
            
            Vector3[] vertices =
            {
                newRotation * new Vector3(positiveX, negativeY, 0f),
                newRotation * new Vector3(0f, positiveY, 0f),
                newRotation * new Vector3(negativeX, negativeY, 0f)
            };
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = new [] { 2, 1, 0 }
            };
            mesh.Optimize();
            
            return mesh;
        }
        
        // ...fuck
        /*public static Mesh GenerateStarMesh(int sides)
        {
            Mesh centerMesh = GenerateFaceMesh(sides, 0.5f);
            
            Mesh[] triangleMeshes = new Mesh[sides];
            for (int i = 0; i < sides; i++)
            {
                triangleMeshes[i] = GenerateTriangleMesh(new Vector2(1f / sides, 0.25f), new Vector2(0f, 0.5f), new Vector3(0f, 180f, i * (sides / 360f)));
            }

            int verticesCount = (sides * triangleMeshes[0].vertices.Length) + centerMesh.vertices.Length;
            int trianglesCount = (sides * triangleMeshes[0].triangles.Length) + centerMesh.triangles.Length;
            
            Vector3[] vertices = new Vector3[verticesCount];
            int[] triangles = new int[trianglesCount];
            for (int i = 0; i < centerMesh.vertices.Length; i++)
            {
                vertices[i] = centerMesh.vertices[i];
            }
            for (int i = 0; i < centerMesh.triangles.Length; i++)
            {
                triangles[i] = centerMesh.triangles[i];
            }
            
            for(int i = 0; i < triangleMeshes.Length; i++)
            {
                int vertLength = triangleMeshes[i].vertices.Length - 1;
                int triLength = triangleMeshes[i].triangles.Length - 1;
                
                for (int j = 0; j < vertLength; j++)
                {
                    vertices[(i * vertLength) + j + centerMesh.vertices.Length] = triangleMeshes[i].vertices[j];
                }
                
                for (int j = 0; j < triLength; j++)
                {
                    triangles[(i * triLength) + j + centerMesh.triangles.Length] = triangleMeshes[i].triangles[j] + (i * triLength);   
                }
            }
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };

            return mesh;
        }*/

        private static Mesh GenerateRectangle(Vector2 size, Vector2 offset = default, Vector3 rotation = default)
        {
            Quaternion newRotation = new Quaternion
            {
                eulerAngles = rotation
            };
            
            float negativeX = (-1f * (size.x / 2f)) + offset.x;
            float positiveX = (size.x / 2f) + offset.x;
            float negativeY = (-1f * (size.y / 2f)) + offset.y;
            float positiveY = (size.y / 2f) + offset.y;
            
            Vector3[] vertices =
            {
                newRotation * new Vector3(negativeX, positiveY, 0f),
                newRotation * new Vector3(positiveX, positiveY, 0f),
                newRotation * new Vector3(negativeX, negativeY, 0f),
                newRotation * new Vector3(positiveX, negativeY, 0f)
            };
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = new[] { 1, 3, 2, 0, 1, 2 }
            };
            mesh.Optimize();

            return mesh;
        }

        public static Mesh GenerateBasicLineMesh()
        {
            return GenerateRectangle(new Vector2(0.3f, 0.0933f), new Vector2(0f, -0.00335f));
        }
        
        public static Mesh GenerateChevronMesh()
        { 
            Mesh[] meshes =
            {
                GenerateRectangle(new Vector2(0.139f, 0.042f), new Vector2(0.033f, -0.067f), new Vector3(0f, 0f, 22.5f)),
                GenerateRectangle(new Vector2(0.139f, 0.042f), new Vector2(-0.033f, -0.067f), new Vector3(0f, 0f, -22.5f))
            };

            Vector3[] vertices = new Vector3[4 * meshes.Length];
            int[] triangles = new int[6 * meshes.Length];
            for(int i = 0; i < meshes.Length; i++)
            {
                for (int j = 0; j < meshes[i].vertices.Length; j++)
                {
                    vertices[i * meshes[i].vertices.Length + j] = meshes[i].vertices[j] + (Vector3.up * 0.045f);
                }

                for (int j = 0; j < meshes[i].triangles.Length; j++)
                {
                    triangles[i * meshes[i].triangles.Length + j] = meshes[i].triangles[j] + (i * 4);
                }
            }

            // getting sides and height equal. the quick and dirty way.
            // which is fine, this should only ever run once lol
            float diff = ((vertices[2].y * -1f) - vertices[0].y) / 2f;
            vertices[0].x = vertices[1].x;
            vertices[7].x = vertices[6].x;
            vertices[2].y = (vertices[0].y * -1f) - diff;
            vertices[5].y = (vertices[0].y * -1f) - diff;
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.Optimize();

            return mesh;
        }

        public static Mesh GeneratePointyMesh(Vector2 offset = default)
        {
            Vector3[] vertices =
            {
                new Vector3(0f + offset.x, -0.0333f + offset.y, 0f),
                new Vector3(-0.15f + offset.x, 0.06f + offset.y, 0f),
                new Vector3(0f + offset.x, 0.0333f + offset.y, 0f),
                new Vector3(0.15f + offset.x, 0.06f + offset.y, 0f)
            };
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Optimize();

            return mesh;
        }

        public static Mesh GeneratePentagonMesh()
        {
            Mesh[] meshes =
            {
                GenerateRectangle(new Vector2(0.3f, 0.03f), new Vector2(0f, 0.025f)),
                GenerateTriangleMesh(new Vector2(0.3f, 0.06f), new Vector2(0f, 0.02f), new Vector3(0f, 0f, 180f))
            };
            
            Vector3[] vertices = new Vector3[7];
            int[] triangles = new int[9];
            for (int i = 0; i < meshes[0].vertices.Length; i++)
            {
                vertices[i] = meshes[0].vertices[i];
            }
            for (int i = 0; i < meshes[0].triangles.Length; i++)
            {
                triangles[i] = meshes[0].triangles[i];
            }
            
            for (int i = 0; i < meshes[1].vertices.Length; i++)
            {
                vertices[i + meshes[0].vertices.Length] = meshes[1].vertices[i];
            }
            for (int i = 0; i < meshes[1].triangles.Length; i++)
            {
                triangles[i + meshes[0].triangles.Length] = meshes[1].triangles[i] + meshes[0].vertices.Length;
            }
            
            vertices[0].x /= 1.67f;
            vertices[3].x /= 1.67f;
            
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };
            mesh.Optimize();

            return mesh;
        }
        
#if !PRE_V1_39_1
        internal static Mesh Invert(this Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
            for(int i = 0; i < normals.Length; i++)
            {
                normals[i] = -1 * normals[i];
            }
            mesh.normals = normals;

            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i+=3)
            {
                (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
            }

            mesh.triangles = triangles;
            return mesh;
        }
        
        // https://discussions.unity.com/t/reading-meshes-at-runtime-that-are-not-enabled-for-read-write/804189/8
        public static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
        {
            Mesh meshCopy = new Mesh
            {
                indexFormat = nonReadableMesh.indexFormat
            };

            // Handle vertices
            GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
            int totalSize = verticesBuffer.stride * verticesBuffer.count;
            byte[] data = new byte[totalSize];
            verticesBuffer.GetData(data);
            meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
            meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
            verticesBuffer.Release();

            // Handle triangles
            meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
            GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
            int tot = indexesBuffer.stride * indexesBuffer.count;
            byte[] indexesData = new byte[tot];
            indexesBuffer.GetData(indexesData);
            meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
            meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
            indexesBuffer.Release();

            // Restore submesh structure
            uint currentIndexOffset = 0;
            for (int i = 0; i < meshCopy.subMeshCount; i++)
            {
                uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
                meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
                currentIndexOffset += subMeshIndexCount;
            }

            // Recalculate normals and bounds
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();

            return meshCopy;
        }
#endif
    }

    internal static class Vectors
    {
        public static Vector2 Min(Vector2 vector, float min)
        {
            vector.x = Mathf.Min(vector.x, min);
            vector.y = Mathf.Min(vector.y, min);
            
            return vector;
        }
        
        public static Vector3 Min(Vector3 vector, float min)
        {
            vector.x = Mathf.Min(vector.x, min);
            vector.y = Mathf.Min(vector.y, min);
            vector.z = Mathf.Min(vector.z, min);
            
            return vector;
        }

        public static Vector2 Max(Vector2 vector, float max)
        {
            vector.x = Mathf.Max(vector.x, max);
            vector.y = Mathf.Max(vector.y, max);
            
            return vector;
        }
        
        public static Vector3 Max(Vector3 vector, float max)
        {
            vector.x = Mathf.Max(vector.x, max);
            vector.y = Mathf.Max(vector.y, max);
            vector.z = Mathf.Max(vector.z, max);
            
            return vector;
        }
    }
    
    internal static class ColorBrightnessHelper
    {
        public static float Brightness(this Color source) => (source.r * 0.299f) + (source.g * 0.587f) + (source.b * 0.114f);
    }
}