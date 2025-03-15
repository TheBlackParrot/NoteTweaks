using HarmonyLib;
using IPA.Utilities;
using NoteTweaks.Configuration;
using UnityEngine;
#if !PRE_V1_39_1
using UnityEngine.Rendering;
#endif
#pragma warning disable CS0612

namespace NoteTweaks.Managers
{
#if !PRE_V1_39_1
    internal static class MeshExtensions
    {
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
    }
#endif
    
    internal abstract class Outlines
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
#if !PRE_V1_39_1
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
#endif
        
        private static Mesh _defaultNoteMesh;
        public static Mesh InvertedNoteMesh;
        private static Mesh _defaultChainMesh;
        public static Mesh InvertedChainMesh;
        private static Mesh _defaultChainHeadMesh;
        public static Mesh InvertedChainHeadMesh;
        private static Mesh _defaultBombMesh;
        public static Mesh InvertedBombMesh;
        
        public static void UpdateDefaultNoteMesh(Mesh mesh)
        {
            if (_defaultNoteMesh == null)
            {
#if PRE_V1_39_1
                _defaultNoteMesh = mesh;
                InvertedNoteMesh = _defaultNoteMesh;
#else
                _defaultNoteMesh = MakeReadableMeshCopy(mesh);
                InvertedNoteMesh = _defaultNoteMesh.Invert();
#endif
            }
        }
        public static void UpdateDefaultChainMesh(Mesh mesh)
        {
            if (_defaultChainMesh == null)
            {
#if PRE_V1_39_1
                _defaultChainMesh = mesh;
                InvertedChainMesh = _defaultChainMesh;
#else
                _defaultChainMesh = MakeReadableMeshCopy(mesh);
                InvertedChainMesh = _defaultChainMesh.Invert();
#endif
            }
        }
        public static void UpdateDefaultChainHeadMesh(Mesh mesh)
        {
            if (_defaultChainHeadMesh == null)
            {
#if PRE_V1_39_1
                _defaultChainHeadMesh = mesh;
                InvertedChainHeadMesh = _defaultChainHeadMesh;
#else
                _defaultChainHeadMesh = MakeReadableMeshCopy(mesh);
                InvertedChainHeadMesh = _defaultChainHeadMesh.Invert();
#endif
            }
        }
        public static void UpdateDefaultBombMesh(Mesh mesh, bool force = false)
        {
            if (_defaultBombMesh == null || force)
            {
#if PRE_V1_39_1
                _defaultBombMesh = mesh;
                InvertedBombMesh = _defaultBombMesh;
#else
                _defaultBombMesh = MakeReadableMeshCopy(mesh);
                InvertedBombMesh = _defaultBombMesh.Invert();
#endif
            }
        }

        public static void AddOutlineObject(Transform rootTransform, Mesh wantedMesh)
        {
            if (wantedMesh == InvertedBombMesh)
            {
                if (rootTransform.Find("Mesh").Find("NoteOutline") != null)
                {
                    return;
                }
            }
            else
            {
                if (rootTransform.Find("NoteOutline") != null)
                {
                    return;
                }
            }

            GameObject outlineObject = new GameObject();
            outlineObject.SetActive(false);
            outlineObject.AddComponent<MeshFilter>();
            outlineObject.AddComponent<MeshRenderer>();
            outlineObject.AddComponent<MaterialPropertyBlockController>();
            outlineObject.AddComponent<ConditionalMaterialSwitcher>();
            
            // sigh
            GameObject clonedOutlineObject = wantedMesh == InvertedBombMesh ? Object.Instantiate(outlineObject, rootTransform.Find("Mesh")) : Object.Instantiate(outlineObject, rootTransform);

            MeshFilter outlineMeshFilter = clonedOutlineObject.GetComponent<MeshFilter>();
            Renderer outlineRenderer = clonedOutlineObject.GetComponent<Renderer>();
            MaterialPropertyBlockController outlineMaterialPropertyBlockController = clonedOutlineObject.GetComponent<MaterialPropertyBlockController>();
            
            rootTransform.GetComponent<CutoutEffect>().CopyComponent(typeof(CutoutEffect), clonedOutlineObject);
            CutoutEffect outlineCutoutEffect = clonedOutlineObject.GetComponent<CutoutEffect>();
            
            ConditionalMaterialSwitcher outlineMaterialSwitcher = clonedOutlineObject.GetComponent<ConditionalMaterialSwitcher>();
            outlineMaterialSwitcher._renderer = outlineRenderer;
            
            outlineMaterialPropertyBlockController.SetField("_renderers", new [] { outlineRenderer });
            
            outlineMeshFilter.sharedMesh = wantedMesh;
            outlineRenderer.sharedMaterial = Materials.OutlineMaterial;
            outlineRenderer.SetPropertyBlock(outlineMaterialPropertyBlockController.materialPropertyBlock);
            outlineCutoutEffect._materialPropertyBlockController = outlineMaterialPropertyBlockController;
            
            outlineMaterialSwitcher._material0 = Materials.OutlineMaterial;
            outlineMaterialSwitcher._material1 = Materials.OutlineMaterial;
            outlineMaterialSwitcher._renderer = outlineRenderer;
            if (rootTransform.TryGetComponent(out ConditionalMaterialSwitcher switcher))
            {
                outlineMaterialSwitcher._value = switcher._value;
            }
            else
            {
                if(rootTransform.Find("Mesh").TryGetComponent(out ConditionalMaterialSwitcher switcherInMesh))
                {
                    // bombs
                    outlineMaterialSwitcher._value = switcherInMesh._value;
                }
            }

            if (rootTransform.TryGetComponent(out CutoutAnimateEffect cutoutAnimateEffect))
            {
                cutoutAnimateEffect._cuttoutEffects.AddToArray(outlineCutoutEffect);
            }
            
            clonedOutlineObject.name = "NoteOutline";
            clonedOutlineObject.layer = LayerMask.NameToLayer("Note");
            clonedOutlineObject.SetActive(wantedMesh == InvertedBombMesh ? Config.EnableBombOutlines : Config.EnableNoteOutlines);
        }
    }
}