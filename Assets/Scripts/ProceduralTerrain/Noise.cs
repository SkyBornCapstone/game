using UnityEngine;
using UnityEngine.UIElements;

public class Noise
{

    
    public static float GenerateHeight(float x, float y, float persistance, float lacunarity, float heightScale, int octaves, Vector2[] octaveOffsets)
    {
        float height = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x * frequency + octaveOffsets[i].x;
            float sampleY = y * frequency + octaveOffsets[i].y;
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            height += perlinValue * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity; 
        }
        
        return height * heightScale;
    }
    
   
}
