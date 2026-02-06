using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };

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

            Bounds bounds = mesh.bounds;
            float margin = .01f;
            
            
            for (int i = 0; i < originalvertices.Length; i++)
            {
                Vector3 v = originalvertices[i];
                        
                bool isEdge = (v.x <= bounds.min.x + margin || v.x >= bounds.max.x - margin || 
                               v.y <= bounds.min.y + margin || v.y >= bounds.max.y - margin);
                float height = Noise.GenerateHeight(v.x * noiseScale, v.y * noiseScale, persistance, lacunarity, heightMeshMultiplier, octaves, octaveOffsets);
             
                float finalHeight = heightCurve.Evaluate(height) * heightMeshMultiplier;
                if (isEdge)
                    finalHeight = 0f;
                modifiedvertices[i] = new Vector3(v.x, v.y, finalHeight);
            }

            mesh.vertices = modifiedvertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
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
