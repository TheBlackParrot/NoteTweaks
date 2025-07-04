using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

// https://web.archive.org/web/20210222124128/http://wiki.unity3d.com/index.php/FastObjImporter

/* FastObjImporter.cs
 * by Marc Kusters (Nighteyes)
 *
 * Used for loading .obj files exported by Blender
 * Example usage: Mesh myMesh = FastObjImporter.Instance.ImportFile("path_to_obj_file.obj");
 */

namespace NoteTweaks.Utils
{
    public sealed class FastObjImporter
    {

        #region singleton

        // Singleton code
        // Static can be called from anywhere without having to make an instance
        private static FastObjImporter _instance;

        // If called check if there is an instance, otherwise create it
        public static FastObjImporter Instance => _instance ?? (_instance = new FastObjImporter());

        #endregion

        private List<int> _triangles;
        private List<Vector3> _vertices;
        private List<Vector2> _uv;
        private List<Vector3> _normals;
        private List<Vector3Int> _faceData;
        private List<int> _intArray;

        private const int MIN_POW_10 = -16;
        private const int MAX_POW_10 = 16;
        private const int NUM_POWS_10 = MAX_POW_10 - MIN_POW_10 + 1;
        private static readonly float[] Pow10 = GenerateLookupTable();

        // Use this for initialization
        public Mesh ImportFile(string filePath)
        {
            _triangles = new List<int>();
            _vertices = new List<Vector3>();
            _uv = new List<Vector2>();
            _normals = new List<Vector3>();
            _faceData = new List<Vector3Int>();
            _intArray = new List<int>();

            LoadMeshData(filePath);

            Vector3[] newVerts = new Vector3[_faceData.Count];
            Vector2[] newUVs = new Vector2[_faceData.Count];
            Vector3[] newNormals = new Vector3[_faceData.Count];

            /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
             * for the appropriate Unity mesh array.
             */
            for (int i = 0; i < _faceData.Count; i++)
            {
                newVerts[i] = _vertices[_faceData[i].x - 1];
                if (_faceData[i].y >= 1)
                    newUVs[i] = _uv[_faceData[i].y - 1];

                if (_faceData[i].z >= 1)
                    newNormals[i] = _normals[_faceData[i].z - 1];
            }

            Mesh mesh = new Mesh
            {
                vertices = newVerts,
                uv = newUVs,
                normals = newNormals,
                triangles = _triangles.ToArray()
            };

            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }

        private void LoadMeshData(string fileName)
        {

            StringBuilder sb = new StringBuilder();
            string text = File.ReadAllText(fileName);
            int start = 0;
            int faceDataCount = 0;

            StringBuilder sbFloat = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    sb.Remove(0, sb.Length);

                    // Start +1 for whitespace '\n'
                    sb.Append(text, start + 1, i - start);
                    start = i;

                    if (sb[0] == 'o' && sb[1] == ' ')
                    {
                        sbFloat.Remove(0, sbFloat.Length);
                        int j = 2;
                        while (j < sb.Length)
                        {
                            j++;
                        }
                    }
                    else if (sb[0] == 'v' && sb[1] == ' ') // Vertices
                    {
                        int splitStart = 2;

                        _vertices.Add(new Vector3(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'v' && sb[1] == 't' && sb[2] == ' ') // UV
                    {
                        int splitStart = 3;

                        _uv.Add(new Vector2(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'v' && sb[1] == 'n' && sb[2] == ' ') // Normals
                    {
                        int splitStart = 3;

                        _normals.Add(new Vector3(GetFloat(sb, ref splitStart, ref sbFloat),
                            GetFloat(sb, ref splitStart, ref sbFloat), GetFloat(sb, ref splitStart, ref sbFloat)));
                    }
                    else if (sb[0] == 'f' && sb[1] == ' ')
                    {
                        int splitStart = 2;

                        int j = 1;
                        _intArray.Clear();
                        int info = 0;
                        // Add faceData, a face can contain multiple triangles, facedata is stored in following order vert, uv, normal. If uv or normal are / set it to a 0
                        while (splitStart < sb.Length && char.IsDigit(sb[splitStart]))
                        {
                            _faceData.Add(new Vector3Int(GetInt(sb, ref splitStart, ref sbFloat),
                                GetInt(sb, ref splitStart, ref sbFloat), GetInt(sb, ref splitStart, ref sbFloat)));
                            j++;

                            _intArray.Add(faceDataCount);
                            faceDataCount++;
                        }

                        info += j;
                        j = 1;
                        while
                            (j + 2 <
                             info) //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                        {
                            _triangles.Add(_intArray[0]);
                            _triangles.Add(_intArray[j]);
                            _triangles.Add(_intArray[j + 1]);

                            j++;
                        }
                    }
                }
            }
        }

        private float GetFloat(StringBuilder sb, ref int start, ref StringBuilder sbFloat)
        {
            sbFloat.Remove(0, sbFloat.Length);
            while (start < sb.Length &&
                   (char.IsDigit(sb[start]) || sb[start] == '-' || sb[start] == '.'))
            {
                sbFloat.Append(sb[start]);
                start++;
            }

            start++;

            return ParseFloat(sbFloat);
        }

        private int GetInt(StringBuilder sb, ref int start, ref StringBuilder sbInt)
        {
            sbInt.Remove(0, sbInt.Length);
            while (start < sb.Length &&
                   (char.IsDigit(sb[start])))
            {
                sbInt.Append(sb[start]);
                start++;
            }

            start++;

            return IntParseFast(sbInt);
        }


        private static float[] GenerateLookupTable()
        {
            var result = new float[(-MIN_POW_10 + MAX_POW_10) * 10];
            for (int i = 0; i < result.Length; i++)
                // ReSharper disable once PossibleLossOfFraction
                result[i] = ((i / NUM_POWS_10) * Mathf.Pow(10, i % NUM_POWS_10 + MIN_POW_10));
            return result;
        }

        private float ParseFloat(StringBuilder value)
        {
            float result = 0;
            bool negate = false;
            int len = value.Length;
            int decimalIndex = value.Length;
            for (int i = len - 1; i >= 0; i--)
                if (value[i] == '.')
                {
                    decimalIndex = i;
                    break;
                }

            int offset = -MIN_POW_10 + decimalIndex;
            for (int i = 0; i < decimalIndex; i++)
                if (i != decimalIndex && value[i] != '-')
                    result += Pow10[(value[i] - '0') * NUM_POWS_10 + offset - i - 1];
                else if (value[i] == '-')
                    negate = true;
            for (int i = decimalIndex + 1; i < len; i++)
                if (i != decimalIndex)
                    result += Pow10[(value[i] - '0') * NUM_POWS_10 + offset - i];
            if (negate)
                result = -result;
            return result;
        }

        private int IntParseFast(StringBuilder value)
        {
            // An optimized int parse method.
            int result = 0;
            for (int i = 0; i < value.Length; i++)
            {
                result = 10 * result + (value[i] - 48);
            }

            return result;
        }
    }

    public sealed class Vector3Int
    {
        // ReSharper disable InconsistentNaming
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        // ReSharper restore InconsistentNaming

        public Vector3Int()
        {
        }

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}