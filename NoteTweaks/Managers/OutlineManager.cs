using UnityEngine;
using UnityEngine.Rendering;

namespace NoteTweaks.Managers
{
    internal abstract class Outlines
    {
        // https://discussions.unity.com/t/reading-meshes-at-runtime-that-are-not-enabled-for-read-write/804189/8
        private static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
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
        
        private static Mesh _defaultNoteMesh;
        public static Mesh InvertedNoteMesh;
        public static void UpdateDefaultMesh(Mesh mesh)
        {
            if (_defaultNoteMesh == null)
            {
                _defaultNoteMesh = MakeReadableMeshCopy(mesh);
                InvertedNoteMesh = _defaultNoteMesh;
                
                Vector3[] normals = _defaultNoteMesh.normals;
                for(int i = 0; i < normals.Length; i++)
                {
                    normals[i] = -1 * normals[i];
                }
                InvertedNoteMesh.normals = normals;

                int[] triangles = _defaultNoteMesh.triangles;
                for (int i = 0; i < triangles.Length; i+=3)
                {
                    (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
                }           

                InvertedNoteMesh.triangles = triangles;
            }
        }

        public static void AddOutlineObject(Transform rootTransform)
        {
            if (rootTransform.Find("NoteOutline") != null)
            {
                return;
            }
            
            GameObject outlineObject = new GameObject();
            MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
            MeshRenderer outlineMeshRenderer = outlineObject.AddComponent<MeshRenderer>();
            
            outlineMeshFilter.sharedMesh = InvertedNoteMesh;
            outlineMeshRenderer.sharedMaterial = Materials.OutlineMaterial;
            
            GameObject clonedOutlineObject = Object.Instantiate(outlineObject, rootTransform);
            clonedOutlineObject.name = "NoteOutline";
            clonedOutlineObject.SetActive(Plugin.Config.EnableNoteOutlines);
        }
    }
}