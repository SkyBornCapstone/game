using UnityEngine;
using System.Collections.Generic;

namespace core
{
    public static class MeshBuilder
    {
        public static Mesh Build(float[,,] densityField, float isoLevel, float cellSize = 1f,
            Gradient gradient = null)
        {
            int resX = densityField.GetLength(0);
            int resY = densityField.GetLength(1);
            int resZ = densityField.GetLength(2);

            Vector3 centerOffset = new Vector3(resX - 1, resY - 1, resZ - 1) * cellSize * 0.5f;

            List<Vector3> vertices  = new List<Vector3>();
            List<int>     triangles = new List<int>();
            List<Color>   colors    = new List<Color>();

            float minY = -centerOffset.y;
            float maxY =  centerOffset.y;

            for (int x = 0; x < resX - 1; x++)
            for (int y = 0; y < resY - 1; y++)
            for (int z = 0; z < resZ - 1; z++)
            {
                float[] corners = new float[8];
                for (int i = 0; i < 8; i++)
                {
                    Vector3 offset = MarchingCubesTables.CornerOffsets[i];
                    corners[i] = densityField[
                        x + (int)offset.x,
                        y + (int)offset.y,
                        z + (int)offset.z
                    ];
                }

                int cubeIndex = 0;
                for (int i = 0; i < 8; i++)
                    if (corners[i] < isoLevel)
                        cubeIndex |= (1 << i);

                if (MarchingCubesTables.EdgeTable[cubeIndex] == 0) continue;

                Vector3[] edgeVertices = new Vector3[12];
                for (int i = 0; i < 12; i++)
                {
                    if ((MarchingCubesTables.EdgeTable[cubeIndex] & (1 << i)) != 0)
                    {
                        int a = MarchingCubesTables.EdgeVertices[i, 0];
                        int b = MarchingCubesTables.EdgeVertices[i, 1];

                        Vector3 posA = (new Vector3(x, y, z) + MarchingCubesTables.CornerOffsets[a]) * cellSize - centerOffset;
                        Vector3 posB = (new Vector3(x, y, z) + MarchingCubesTables.CornerOffsets[b]) * cellSize - centerOffset;

                        edgeVertices[i] = Interpolate(posA, posB, corners[a], corners[b], isoLevel);
                    }
                }

                for (int i = 0; MarchingCubesTables.TriTable[cubeIndex, i] != -1; i += 3)
                {
                    Vector3 v0 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i    ]];
                    Vector3 v1 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 1]];
                    Vector3 v2 = edgeVertices[MarchingCubesTables.TriTable[cubeIndex, i + 2]];

                    float centerY = (v0.y + v1.y + v2.y) / 3f;
                    float t = Mathf.InverseLerp(minY, maxY, centerY);
                    Color triColor = gradient != null ? gradient.Evaluate(t) : Color.white;

                    int startIndex = vertices.Count;
                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);

                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);

                    colors.Add(triColor);
                    colors.Add(triColor);
                    colors.Add(triColor);
                }
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices  = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors    = colors.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        static Vector3 Interpolate(Vector3 a, Vector3 b, float valA, float valB, float isoLevel)
        {
            if (Mathf.Abs(valA - valB) < 0.00001f) return a;
            float t = (isoLevel - valA) / (valB - valA);
            return a + t * (b - a);
        }
    }
}