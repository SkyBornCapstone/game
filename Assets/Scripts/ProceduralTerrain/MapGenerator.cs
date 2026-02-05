using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap
    };

    public DrawMode drawMode;
    public MeshFilter meshFilter;
    public int mapWidth;
    public int mapLength;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public bool autoUpdate;
    public int seed;
    public Vector2 offset;
    
    public TerrainType[] regions;
    public void GenerateMap()
    {
        Color[] colorMap = new Color[mapWidth * mapLength];
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds bounds = meshFilter.sharedMesh.bounds;
            Vector3 scale = meshFilter.transform.localScale;
        
            // This will give you the actual world-space size
            float worldWidth = bounds.size.x * scale.x;
            float worldLength = bounds.size.y * scale.y;
        
            // Use these directly (will be large numbers like 200, 200)
            mapWidth = Mathf.RoundToInt(worldWidth);
            mapLength = Mathf.RoundToInt(worldLength);
        
            Debug.Log($"Plane dimensions - Width: {mapWidth}, Length: {mapLength}");
        }
        float[,] noiseMap = Noise.GenerateNoise(mapWidth, mapLength, seed, noiseScale,  octaves, persistance, lacunarity, offset);
        
        
        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Debug.Log($"Position: ({x},{y})");
                Debug.Log($"NoiseMap dimensions: {noiseMap.GetLength(0)} x {noiseMap.GetLength(1)}");
                Debug.Log($"Value at ({x},{y}): {noiseMap[x, y]}");
                float currentHeight =  noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight < regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColourMap)
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colorMap,  mapWidth, mapLength));
            
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
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    
    public Color colour;
}
