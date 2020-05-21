using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
    public Terrain _terrain; // terrain to modify
    public NoiseValues _noiseValues;

    public enum NormaliseMode { Local, Global };
    public float getHeight = 0;
    public float amplitude = 1;
    public float frequency = 1;

    void Start()
    {
        // Set heights for initial terrain
        SetMap(_terrain, _noiseValues);

    }

    private void Update()
    {
        //Check it new terrains are needed
        //See what is visible
        //If need new one then add terrain and call setmap to create heights
    }


    public void SetMap(Terrain _terrain, NoiseValues _noiseValues)
    {
        int mapWidth = _terrain.terrainData.heightmapWidth;
        int mapHeigth = _terrain.terrainData.heightmapHeight;
        Vector2 terrainCentre = new Vector2(_terrain.terrainData.size.x / 2, _terrain.terrainData.size.z / 2);

        //Update height array using Perlin Noise
        float[,] heights = SetPerlinNoise(mapWidth, mapHeigth, _noiseValues, terrainCentre);

        // set the new height
        _terrain.terrainData.SetHeights(0, 0, heights);

    }

    public float[,] SetPerlinNoise(int width, int height, NoiseValues _noiseValues, Vector2 sampleCentre)
    {
        float[,] map = new float[width, height];

        // Randomise the seed
        System.Random randomSeed = new System.Random(_noiseValues.seed);

        // Set octaves
        Vector2[] setOctaves = new Vector2[_noiseValues.octaves];

        // Loop through octaves
        for (int i = 0; i < _noiseValues.octaves; i++)
        {
            // Randomise seed across the x and y axis between the values -100000, 100000
            float sampleX = randomSeed.Next(-100000, 100000) + _noiseValues.offset.x + sampleCentre.x;
            float sampleY = randomSeed.Next(-100000, 100000) + _noiseValues.offset.y + sampleCentre.y;
            setOctaves[i] = new Vector2(sampleX, sampleY);

            getHeight += amplitude;
            amplitude *= _noiseValues.persistance;
        }

        // Set Min and Max heights for perlin noise
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        // Calculate centre point map
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // Loop throught map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                amplitude = 1;
                frequency = 1;
                float perlinNoiseHeight = 0;

                for (int i = 0; i < _noiseValues.octaves; i++)
                {
                    // Sample height values
                    float sampleX = (x - halfWidth + setOctaves[i].x) / _noiseValues.scale * frequency;
                    float sampleY = (y - halfHeight + setOctaves[i].y) / _noiseValues.scale * frequency;

                    // Set perlinValue
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Apply perlinValue to perlinNoiseHeight multiplied by the applied amplitude
                    perlinNoiseHeight += perlinValue * amplitude;

                    // Set amplitude to equal persitance where persitance puts the value in the range 0 and 1, decreasing each octave
                    amplitude *= _noiseValues.persistance;

                    // Frequence is used to increase each octave, since lacunarity should be more than 1
                    frequency *= _noiseValues.lacunarity;

                    // If perlinNoiseHeight is greater than the maxNoiseHeight set the maxNoiseHeight to equal noiseHeight
                    if (perlinNoiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = perlinNoiseHeight;
                    }
                    // If noiseHeight is less than the minNoiseHeight set the minNoiseHeight to equal noiseHeight
                    if (perlinNoiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = perlinNoiseHeight;
                    }
                    // Set array map[x, y] equal to perlinNoiseHeight
                    map[x, y] = perlinNoiseHeight;

                    if (_noiseValues._normaliseMode == NormaliseMode.Global)
                    {
                        // Set Global Normalisation
                        float normalisedHeight = (map[x, y] + 1) / (perlinNoiseHeight / 0.9f);
                        // Clamp map to normlisedHeight on x axis, y to 0 and z axis to the max value
                        map[x, y] = Mathf.Clamp(normalisedHeight, 0, int.MaxValue);
                    }
                }
            }
        }

        // Set Local normalise mode
        if (_noiseValues._normaliseMode == NormaliseMode.Local)
        {
            // For loop to normalise map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // If normaliseMode equals normal then the entire map can be generated at one knowing the min and max noiseheight values

                    map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]) / 30;
                }
            }
        }
        return map;
    }
}



[System.Serializable]
public class NoiseValues
{
    public MapGenerator.NormaliseMode _normaliseMode;

    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2;
    public int seed;
    public Vector2 offset;

    // Function to declare validated values 
    public void ValidateValues()
    {
        // Max function used to choose whichever is greater, so if the scale is less than 0.01 then the value will be set to 0.01
        // The max function will be apply the same functions to octaves and lacunarity
        scale = Mathf.Max(scale, 0.01f);
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        // Clamp persistance between 0 and 1
        persistance = Mathf.Clamp01(persistance);
    }
}