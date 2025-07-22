using HarmonyLib;
using IPA.Utilities;
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;
#pragma warning disable CS0612

namespace NoteTweaks.Managers
{
    internal abstract class Outlines
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        private static Mesh _defaultNoteMesh;
        public static Mesh InvertedNoteMesh;
        private static Mesh _defaultDotNoteMesh;
        public static Mesh InvertedDotNoteMesh;
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
                _defaultNoteMesh = Utils.Meshes.MakeReadableMeshCopy(mesh);
                InvertedNoteMesh = _defaultNoteMesh.Invert();
            }
        }
        public static void UpdateDefaultDotNoteMesh(Mesh mesh)
        {
            if (_defaultDotNoteMesh == null)
            {
                _defaultDotNoteMesh = Utils.Meshes.MakeReadableMeshCopy(mesh);
                InvertedDotNoteMesh = _defaultDotNoteMesh.Invert();
            }
        }
        public static void UpdateDefaultChainMesh(Mesh mesh)
        {
            if (_defaultChainMesh == null)
            {
                _defaultChainMesh = Utils.Meshes.MakeReadableMeshCopy(mesh);
                InvertedChainMesh = _defaultChainMesh.Invert();
            }
        }
        public static void UpdateDefaultChainHeadMesh(Mesh mesh)
        {
            if (_defaultChainHeadMesh == null)
            {
                _defaultChainHeadMesh = Utils.Meshes.MakeReadableMeshCopy(mesh);
                InvertedChainHeadMesh = _defaultChainHeadMesh.Invert();
            }
        }
        public static void UpdateDefaultBombMesh(Mesh mesh, bool force = false)
        {
            if (_defaultBombMesh == null || force)
            {
                _defaultBombMesh = Utils.Meshes.MakeReadableMeshCopy(mesh);
                InvertedBombMesh = _defaultBombMesh.Invert();
            }
        }

        public static void AddOutlineObject(Transform rootTransform, Mesh wantedMesh, bool isFullNote = true)
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
                Transform noteOutlineTransform = rootTransform.Find("NoteOutline");
                if (noteOutlineTransform != null)
                {
                    if (isFullNote)
                    {
                        if (noteOutlineTransform.TryGetComponent(out MeshFilter existingOutlineMeshFilter))
                        {
                            existingOutlineMeshFilter.sharedMesh = wantedMesh;
                        }
                    }
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