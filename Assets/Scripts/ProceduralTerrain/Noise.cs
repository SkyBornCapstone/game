using UnityEngine;

public class Noise
{
    public static float[,] GenerateNoise(int mapWidth, int mapLength, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapLength];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        
        if (scale == 0)
        {
            scale = .0001f;
        }

        float halfWidth = mapWidth / 2f;
        float halfLength = mapLength / 2f;

        float max = float.MinValue;
        float min = float.MaxValue;
        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x- halfWidth) / scale * frequency  + octaveOffsets[i].x;
                    float sampleY = (y- halfLength) / scale * frequency +  octaveOffsets[i].y;
                
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency += lacunarity;
                }

                if (noiseHeight > max)
                {
                    max = noiseHeight;
                }else if (noiseHeight < min)
                {
                    min = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapLength; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(min, max, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
