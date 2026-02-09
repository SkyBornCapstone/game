using UnityEngine;
using System.Collections.Generic;
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };
    [Header("NoiseMap")]
    public DrawMode drawMode;
    public MeshFilter meshFilter;
    public int mapWidth;
    public int mapLength;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;
    public bool autoUpdate;
    public int seed;
    public Vector2 offset;
    [Range(0, 1)] public float heightMeshMultiplier = 0f;
    public AnimationCurve heightCurve;
    public TerrainType[] regions;
    [Header("Colors")] public Gradient colorGradient;
    
    private HashSet<int> edgeVertices = new HashSet<int>();
    private bool edgeVerticesInitialized = false;
    public void GenerateMap()
    {
        if (drawMode == DrawMode.Mesh)
        {
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++) {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] originalvertices = mesh.vertices;
            Vector3[] modifiedvertices = new Vector3[originalvertices.Length];
            
            Color[] colors = mesh.colors;
           
            Bounds bounds = mesh.bounds;

            if (!edgeVerticesInitialized)
            {
                IntializeEdgeVertices(mesh);
            }
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int i = 0; i < originalvertices.Length; i++)
            {
                Vector3 v = originalvertices[i];
                float height = Noise.GenerateHeight(v.x * noiseScale, v.y * noiseScale, persistance, lacunarity, heightMeshMultiplier, octaves, octaveOffsets);
             
                float finalHeight = heightCurve.Evaluate(height) * heightMeshMultiplier;
                if (colors[i].r == 0.0f && colors[i].g == 0.0f && colors[i].b == 0.0f)
                {
                    finalHeight = 0f;
                }
                modifiedvertices[i] = new Vector3(v.x, v.y, finalHeight);
                if (finalHeight < minHeight)
                {
                    minHeight = finalHeight;
                }else if (finalHeight > maxHeight)
                {
                    maxHeight = finalHeight;
                }
            }

            mesh.vertices = modifiedvertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            TextureGenerator.GenerateColors(mesh, minHeight, maxHeight, colorGradient, edgeVertices);
            
        }
    }

    private void IntializeEdgeVertices(Mesh mesh)
    {
        edgeVertices.Clear();
        edgeVerticesInitialized = true;

        Color[] colors = mesh.colors;
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].r == 0.0f && colors[i].g == 0.0f && colors[i].b == 0.0f)
            {
                edgeVertices.Add(i);
            }
        }
        edgeVerticesInitialized = true;
    }
    
    void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }

        if (mapLength < 1)
        {
            mapLength = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }

    }


    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;

        public Color colour;
    }
}
