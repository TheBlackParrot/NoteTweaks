using NoteTweaks.Configuration;
using NoteTweaks.Utils;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Meshes
    {
        private static PluginConfig Config => PluginConfig.Instance;
        
        private static readonly Mesh TriangleArrowMesh = Utils.Meshes.GenerateBasicTriangleMesh();
        private static readonly Mesh LineArrowMesh = Utils.Meshes.GenerateBasicLineMesh();
        private static readonly Mesh ChevronArrowMesh = Utils.Meshes.GenerateChevronMesh();
        private static readonly Mesh PointyMesh = Utils.Meshes.GeneratePointyMesh(new Vector2(0f, -0.0165f));
        private static readonly Mesh PentagonArrowMesh = Utils.Meshes.GeneratePentagonMesh();
        private static Mesh _defaultArrowMesh;

        private static int _sphereSlices = Config.BombMeshSlices;
        private static int _sphereStacks = Config.BombMeshStacks;
        private static bool _sphereNormalsSmooth = Config.BombMeshSmoothNormals;
        private static bool _sphereNormalsWorld = Config.BombMeshWorldNormals;
        private const float SPHERE_RADIUS = 0.225f;
        private static Mesh _sphereMesh = MeshExtensions.CreateSphere(SPHERE_RADIUS, _sphereSlices, _sphereStacks, _sphereNormalsWorld);
        private static Mesh _defaultBombMesh;

        public static Mesh DefaultCubeMesh;

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
                    default:
                        return _defaultArrowMesh;
                }
            }
        }

        internal static Mesh DotMesh = Utils.Meshes.GenerateFaceMesh(Config.DotMeshSides);

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
    }
}