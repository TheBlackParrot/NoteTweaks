using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class Meshes
    {
        private static readonly Mesh TriangleArrowMesh = Utils.Meshes.GenerateBasicTriangleMesh();
        private static readonly Mesh LineArrowMesh = Utils.Meshes.GenerateBasicLineMesh();
        private static Mesh _defaultArrowMesh;

        public static Mesh CurrentArrowMesh
        {
            get
            {
                switch (Plugin.Config.ArrowMesh)
                {
                    case "Triangle":
                        return TriangleArrowMesh;
                    case "Line":
                        return LineArrowMesh;
                    default:
                        return _defaultArrowMesh;
                }
            }
        }

        internal static Mesh DotMesh = Utils.Meshes.GenerateFaceMesh(Plugin.Config.DotMeshSides);

        public static void UpdateDefaultArrowMesh(Mesh mesh)
        {
            if (_defaultArrowMesh == null)
            {
                _defaultArrowMesh = mesh;
            }
        }
    }
}