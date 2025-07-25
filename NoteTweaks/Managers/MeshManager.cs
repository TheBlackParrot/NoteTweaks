﻿using System.IO;
using IPA.Utilities;
using JetBrains.Annotations;
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Meshes
    {
        private static PluginConfig Config => PluginConfig.Instance;
        internal static readonly string MeshFolder = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Meshes");
        
        private static readonly Mesh TriangleArrowMesh = Utils.Meshes.GenerateBasicTriangleMesh();
        private static readonly Mesh LineArrowMesh = Utils.Meshes.GenerateBasicLineMesh();
        private static readonly Mesh ChevronArrowMesh = Utils.Meshes.GenerateChevronMesh();
        private static readonly Mesh PointyMesh = Utils.Meshes.GeneratePointyMesh(new Vector2(0f, -0.0165f));
        private static readonly Mesh PentagonArrowMesh = Utils.Meshes.GeneratePentagonMesh();
        private static readonly Mesh OvalMesh = Utils.Meshes.GenerateFaceMesh(32, new Vector2(1f, 0.3f), 0.31f);
        private static Mesh _defaultArrowMesh;

        private static Vector3 BombScaleVector => new Vector3(Config.BombScale, Config.BombScale, Config.BombScale);
        private static int _sphereSlices = Config.BombMeshSlices;
        private static int _sphereStacks = Config.BombMeshStacks;
        private static bool _sphereNormalsSmooth = Config.BombMeshSmoothNormals;
        private static bool _sphereNormalsWorld = Config.BombMeshWorldNormals;
        private const float SPHERE_RADIUS = 0.225f;
        private static Mesh _sphereMesh =
            Utils.Meshes.Scale(
                MeshExtensions.CreateSphere(SPHERE_RADIUS, _sphereSlices, _sphereStacks, _sphereNormalsWorld),
                BombScaleVector);
        private static Mesh _defaultBombMesh;
        private static Mesh _defaultBombMeshScaled;

        private static Mesh _defaultNoteMesh;
        private static Mesh _defaultChainHeadMesh;
        private static Mesh _defaultChainLinkMesh;
        private static Mesh _defaultNoteMeshScaled;
        private static Mesh _defaultDotNoteMeshScaled;
        private static Mesh _defaultChainHeadMeshScaled;
        private static Mesh _defaultChainLinkMeshScaled;
        
        private static Mesh _customNoteMesh;
        private static Mesh _customDotNoteMesh;
        private static Mesh _customChainHeadMesh;
        private static Mesh _customChainLinkMesh;

        public static Mesh CurrentBombMesh
        {
            get
            {
                switch (Config.BombMesh)
                {
                    case "Sphere":
                        return _sphereMesh;
                    default:
                        return _defaultBombMeshScaled;
                }
            }
        }

        public static Mesh CurrentArrowMesh
        {
            get
            {
                switch (Config.ArrowMesh)
                {
                    case "Triangle":
                        return TriangleArrowMesh;
                    case "Line":
                        return LineArrowMesh;
                    case "Chevron":
                        return ChevronArrowMesh;
                    case "Pointy":
                        return PointyMesh;
                    case "Pentagon":
                        return PentagonArrowMesh;
                    case "Oval":
                        return OvalMesh;
                    default:
                        return _defaultArrowMesh;
                }
            }
        }

        public static void UpdateCustomNoteMesh(bool isDotNote = false)
        {
            string noteMeshFolder = Path.Combine(MeshFolder, "Notes");
            if (!Directory.Exists(noteMeshFolder))
            {
                Directory.CreateDirectory(noteMeshFolder);
            }

            switch (isDotNote)
            {
                case true when Config.DotNoteMesh == "Default":
                    _defaultDotNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultNoteMesh), Config.NoteScale);
                    Outlines.InvertedDotNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultDotNoteMeshScaled).Invert();
                    return;
                
                case false when Config.NoteMesh == "Default":
                    _defaultNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultNoteMesh), Config.NoteScale);
                    Outlines.InvertedNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultNoteMeshScaled).Invert();   
                    return;
                
                case true:
                    _customDotNoteMesh =
                        Utils.Meshes.Scale(
                            FastObjImporter.Instance.ImportFile(Path.Combine(noteMeshFolder, $"{Config.DotNoteMesh}.obj")),
                            Config.NoteScale);
                    _customDotNoteMesh.Optimize();

                    Outlines.InvertedDotNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_customDotNoteMesh).Invert();
                    break;
                
                default:
                    _customNoteMesh =
                        Utils.Meshes.Scale(
                            FastObjImporter.Instance.ImportFile(Path.Combine(noteMeshFolder, $"{Config.NoteMesh}.obj")),
                            Config.NoteScale);
                    _customNoteMesh.Optimize();

                    Outlines.InvertedNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_customNoteMesh).Invert();
                    break;
            }
        }
        
        public static void UpdateCustomChainHeadMesh()
        {
            string chainMeshFolder = Path.Combine(MeshFolder, "ChainHeads");
            if (!Directory.Exists(chainMeshFolder))
            {
                Directory.CreateDirectory(chainMeshFolder);
            }

            if (Config.ChainHeadMesh == "Default")
            {
                if (_defaultChainHeadMesh == null)
                {
                    return;
                }
                _defaultChainHeadMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultChainHeadMesh), Config.NoteScale);
                Outlines.InvertedChainHeadMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultChainHeadMeshScaled).Invert();
                return;
            }
            
            _customChainHeadMesh =
                Utils.Meshes.Scale(
                    FastObjImporter.Instance.ImportFile(Path.Combine(chainMeshFolder, $"{Config.ChainHeadMesh}.obj")),
                    Config.NoteScale);
            _customChainHeadMesh.Optimize();

            Outlines.InvertedChainHeadMesh = Utils.Meshes.MakeReadableMeshCopy(_customChainHeadMesh).Invert();
        }
        
        public static void UpdateCustomChainLinkMesh()
        {
            string chainMeshFolder = Path.Combine(MeshFolder, "ChainLinks");
            if (!Directory.Exists(chainMeshFolder))
            {
                Directory.CreateDirectory(chainMeshFolder);
            }

            if (Config.ChainLinkMesh == "Default")
            {
                _defaultChainLinkMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultChainLinkMesh), Config.NoteScale * Config.LinkScale);
                Outlines.InvertedChainMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultChainLinkMeshScaled).Invert();
                return;
            }
            
            _customChainLinkMesh =
                Utils.Meshes.Scale(
                    FastObjImporter.Instance.ImportFile(Path.Combine(chainMeshFolder, $"{Config.ChainLinkMesh}.obj")),
                    Config.NoteScale * Config.LinkScale);
            _customChainLinkMesh.Optimize();

            Outlines.InvertedChainMesh = Utils.Meshes.MakeReadableMeshCopy(_customChainLinkMesh).Invert();
        }

        public static Mesh CurrentNoteMesh
        {
            get
            {
                if (Config.NoteMesh == "Default")
                {
                    return _defaultNoteMeshScaled;
                }

                if (_customNoteMesh == null)
                {
                    UpdateCustomNoteMesh();
                }
                return _customNoteMesh;
            }
        }
        
        public static Mesh CurrentDotNoteMesh
        {
            get
            {
                if (Config.DotNoteMesh == "Default")
                {
                    return _defaultDotNoteMeshScaled;
                }

                if (_customDotNoteMesh == null)
                {
                    UpdateCustomNoteMesh(true);
                }
                return _customDotNoteMesh;
            }
        }
        
        public static Mesh CurrentChainHeadMesh
        {
            get
            {
                if (Config.ChainHeadMesh == "Default")
                {
                    return _defaultChainHeadMeshScaled;
                }

                if (_customChainHeadMesh == null)
                {
                    UpdateCustomChainHeadMesh();
                }
                return _customChainHeadMesh;
            }
        }
        
        public static Mesh CurrentChainLinkMesh
        {
            get
            {
                if (Config.ChainLinkMesh == "Default")
                {
                    return _defaultChainLinkMeshScaled;
                }

                if (_customChainLinkMesh == null)
                {
                    UpdateCustomChainLinkMesh();
                }
                return _customChainLinkMesh;
            }
        }

        internal static Mesh DotMesh = Utils.Meshes.GenerateFaceMesh(Config.DotMeshSides, Vector2.one);

        public static void UpdateDefaultArrowMesh(Mesh mesh)
        {
            if (_defaultArrowMesh == null)
            {
                _defaultArrowMesh = mesh;
            }
        }
        
        public static void UpdateDefaultBombMesh([CanBeNull] Mesh mesh, bool allowRescale = false)
        {
            if (_defaultBombMesh != null || mesh == null)
            {
                if (allowRescale)
                {
                    _defaultBombMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultBombMesh), BombScaleVector);
                }
                
                return;
            }
            
            _defaultBombMesh = mesh;
            _defaultBombMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), BombScaleVector);
        }

        public static void UpdateSphereMesh(int slices, int stacks, bool smoothNormals = false, bool worldNormals = false, bool force = false)
        {
            if (slices == _sphereSlices &&
                stacks == _sphereStacks &&
                smoothNormals == _sphereNormalsSmooth &&
                worldNormals == _sphereNormalsWorld &&
                !force)
            {
                return;
            }
            
            Plugin.Log.Info($"Wants a sphere mesh: {slices} slices, {stacks} stacks, smooth: {smoothNormals}, world: {worldNormals}");
            
            _sphereSlices = slices;
            _sphereStacks = stacks;
            _sphereNormalsSmooth = smoothNormals;
            _sphereNormalsWorld = worldNormals;
            _sphereMesh = Utils.Meshes.Scale(MeshExtensions.CreateSphere(SPHERE_RADIUS, _sphereSlices, _sphereStacks, _sphereNormalsWorld), BombScaleVector);
        }

        public static void UpdateDefaultNoteMesh(Mesh mesh)
        {
            if (_defaultNoteMesh != null)
            {
                return;
            }
            
            _defaultNoteMesh = mesh;
            _defaultNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale);
            _defaultDotNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale);
        }

        public static void UpdateDefaultChainHeadMesh(Mesh mesh)
        {
            if (_defaultChainHeadMesh != null)
            {
                return;
            }
            
            _defaultChainHeadMesh = mesh;
            _defaultChainHeadMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale);
        }

        public static void UpdateDefaultChainLinkMesh(Mesh mesh)
        {
            if (_defaultChainLinkMesh != null)
            {
                return;
            }
            
            _defaultChainLinkMesh = mesh;
            _defaultChainLinkMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale * Config.LinkScale);
        }
    }
}