using System.IO;
using IPA.Utilities;
using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Meshes
    {
        private static PluginConfig Config => PluginConfig.Instance;
        internal static readonly string MeshFolder = Path.Combine(UnityGame.UserDataPath, "NoteTweaks", "Meshes", "Notes");
        
        private static readonly Mesh TriangleArrowMesh = Utils.Meshes.GenerateBasicTriangleMesh();
        private static readonly Mesh LineArrowMesh = Utils.Meshes.GenerateBasicLineMesh();
        private static readonly Mesh ChevronArrowMesh = Utils.Meshes.GenerateChevronMesh();
        private static readonly Mesh PointyMesh = Utils.Meshes.GeneratePointyMesh(new Vector2(0f, -0.0165f));
        private static readonly Mesh PentagonArrowMesh = Utils.Meshes.GeneratePentagonMesh();
        private static readonly Mesh OvalMesh = Utils.Meshes.GenerateFaceMesh(32, new Vector2(1f, 0.3f), 0.31f);
        private static Mesh _defaultArrowMesh;

        private static int _sphereSlices = Config.BombMeshSlices;
        private static int _sphereStacks = Config.BombMeshStacks;
        private static bool _sphereNormalsSmooth = Config.BombMeshSmoothNormals;
        private static bool _sphereNormalsWorld = Config.BombMeshWorldNormals;
        private const float SPHERE_RADIUS = 0.225f;
        private static Mesh _sphereMesh = MeshExtensions.CreateSphere(SPHERE_RADIUS, _sphereSlices, _sphereStacks, _sphereNormalsWorld);
        private static Mesh _defaultBombMesh;

        private static Mesh _defaultNoteMesh;
        private static Mesh _defaultChainHeadMesh;
        private static Mesh _defaultChainLinkMesh;
        private static Mesh _customNoteMesh;
        private static Mesh _defaultNoteMeshScaled;
        private static Mesh _defaultChainHeadMeshScaled;
        private static Mesh _defaultChainLinkMeshScaled;
        
        // (for now)
        public static Mesh CurrentChainHeadMesh => _defaultChainHeadMeshScaled;
        public static Mesh CurrentChainLinkMesh => _defaultChainLinkMeshScaled;

        public static Mesh CurrentBombMesh
        {
            get
            {
                switch (Config.BombMesh)
                {
                    case "Sphere":
                        return _sphereMesh;
                    default:
                        return _defaultBombMesh;
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

        public static void UpdateCustomNoteMesh()
        {
            if (!Directory.Exists(MeshFolder))
            {
                Directory.CreateDirectory(MeshFolder);
            }

            if (Config.NoteMesh == "Default")
            {
                _defaultNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(_defaultNoteMesh), Config.NoteScale);
                Outlines.InvertedNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultNoteMeshScaled).Invert();
                return;
            }
            
            _customNoteMesh =
                Utils.Meshes.Scale(
                    FastObjImporter.Instance.ImportFile(Path.Combine(MeshFolder, $"{Config.NoteMesh}.obj")),
                    Config.NoteScale);
            _customNoteMesh.Optimize();

            Outlines.InvertedNoteMesh = Utils.Meshes.MakeReadableMeshCopy(_customNoteMesh).Invert();
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

        internal static Mesh DotMesh = Utils.Meshes.GenerateFaceMesh(Config.DotMeshSides, Vector2.one);

        public static void UpdateDefaultArrowMesh(Mesh mesh)
        {
            if (_defaultArrowMesh == null)
            {
                _defaultArrowMesh = mesh;
            }
        }
        
        public static void UpdateDefaultBombMesh(Mesh mesh)
        {
            if (_defaultBombMesh == null)
            {
                _defaultBombMesh = mesh;
            }
        }

        public static void UpdateSphereMesh(int slices, int stacks, bool smoothNormals = false, bool worldNormals = false)
        {
            if (slices == _sphereSlices && stacks == _sphereStacks && smoothNormals == _sphereNormalsSmooth && worldNormals == _sphereNormalsWorld)
            {
                return;
            }
            
            Plugin.Log.Info($"Wants a sphere mesh: {slices} slices, {stacks} stacks, smooth: {smoothNormals}, world: {worldNormals}");
            
            _sphereSlices = slices;
            _sphereStacks = stacks;
            _sphereNormalsSmooth = smoothNormals;
            _sphereNormalsWorld = worldNormals;
            _sphereMesh = MeshExtensions.CreateSphere(SPHERE_RADIUS, _sphereSlices, _sphereStacks, _sphereNormalsWorld);
        }

        public static void UpdateDefaultNoteMesh(Mesh mesh)
        {
            if (_defaultNoteMesh != null)
            {
                return;
            }
            
            _defaultNoteMesh = mesh;
            _defaultNoteMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale);
        }

        public static void UpdateDefaultChainHeadMesh(Mesh mesh)
        {
            if (_defaultChainHeadMesh != null)
            {
                return;
            }
            
            _defaultChainHeadMesh = mesh;
            _defaultChainHeadMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale);
            Outlines.InvertedChainHeadMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultChainHeadMeshScaled).Invert();
        }

        public static void UpdateDefaultChainLinkMesh(Mesh mesh)
        {
            if (_defaultChainLinkMesh != null)
            {
                return;
            }
            
            _defaultChainLinkMesh = mesh;
            _defaultChainLinkMeshScaled = Utils.Meshes.Scale(Utils.Meshes.MakeReadableMeshCopy(mesh), Config.NoteScale * Config.LinkScale);
            Outlines.InvertedChainMesh = Utils.Meshes.MakeReadableMeshCopy(_defaultChainLinkMeshScaled).Invert();
        }
    }
}