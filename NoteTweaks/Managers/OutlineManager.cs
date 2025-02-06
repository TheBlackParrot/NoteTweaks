using HarmonyLib;
using IPA.Utilities;
using NoteTweaks.Configuration;
using UnityEngine;
#pragma warning disable CS0612

namespace NoteTweaks.Managers
{
    internal abstract class Outlines
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
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
                _defaultNoteMesh = mesh;
                InvertedNoteMesh = _defaultNoteMesh;
            }
        }
        public static void UpdateDefaultChainMesh(Mesh mesh)
        {
            if (_defaultChainMesh == null)
            {
                _defaultChainMesh = mesh;
                InvertedChainMesh = _defaultChainMesh;
            }
        }
        public static void UpdateDefaultChainHeadMesh(Mesh mesh)
        {
            if (_defaultChainHeadMesh == null)
            {
                _defaultChainHeadMesh = mesh;
                InvertedChainHeadMesh = _defaultChainHeadMesh;
            }
        }
        public static void UpdateDefaultBombMesh(Mesh mesh)
        {
            if (_defaultBombMesh == null)
            {
                _defaultBombMesh = mesh;
                InvertedBombMesh = _defaultBombMesh;
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
            outlineRenderer.sharedMaterial = Materials.OutlineMaterial1;
            outlineRenderer.SetPropertyBlock(outlineMaterialPropertyBlockController.materialPropertyBlock);
            outlineCutoutEffect._materialPropertyBlockController = outlineMaterialPropertyBlockController;
            
            outlineMaterialSwitcher._material0 = Materials.OutlineMaterial0;
            outlineMaterialSwitcher._material1 = Materials.OutlineMaterial1;
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
            clonedOutlineObject.SetActive(wantedMesh == InvertedBombMesh ? Config.EnableBombOutlines : Config.EnableNoteOutlines);
        }
    }
}