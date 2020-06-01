﻿using System.Collections.Generic;
using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
    // Terrain to modify
    public Terrain _terrain;

    // Class referemces
    public NoiseValues _noiseValues;
    public TexturingTerrain _texturingTerrain;
    public MainController _mainController;

    // Float arrays for heightmap and splat data
    private float[,] heightmapData;
    private float[,,] splatData;

    // Default terrain data, used to store the values for the initial terrain
    private TerrainData defaultTD;
    // Create next terrain chunk
    private GameObject nextTerrainChunk;
    // Create next terrain
    private Terrain _nextTerrain;

    // Terrains array
    private Terrain[] _terrains;

    // Check all positions of the terrains in the terrains array
    private Vector3 checkTerrainPosition;
    // Get current terrain position
    private Vector3 currentTerrainPosition;
    // Used to set the surronding terrain position in endless terrain
    private Vector3[] surroundingTerrain;
    private Vector3[] neighbouringTerrain;
    // Calculate center of terrain
    private Vector2 terrainCentre;

    // Set number of terrains, used to change how many possible heightmap and splatmaps are used
    public int numOfTerrains;
    // Terrain number that is created
    private int terrainNumber = 0;
    // Create float 2D array list of terrain heights
    private List<float[,]> terrainHeights = new List<float[,]>();
    // Create 3D float array for terrainS splatmap
    private List<float[,,]> terrainSplatmap = new List<float[,,]>();

    // Check if terrain is found when checking around the current terrain the player is on
    private bool terrainFound;
    private int foundTerrainID;

    void Start()
    {
        // Calculate center of terrain
        terrainCentre = new Vector2(_terrain.terrainData.size.x / 2, _terrain.terrainData.size.z / 2);

        for (int i = 0; i < numOfTerrains; i++)
        {
            // Set heightmapData to eqaul the perlin noise function, passing in the terrain width, height, noise values applied to map, and random value times 100 to create random terrain seeds
            heightmapData = SetPerlinNoise(_terrain.terrainData.heightmapWidth, _terrain.terrainData.heightmapHeight, _noiseValues, terrainCentre, (int)(Random.value * 100));
            // Set heights for initial terrain
            _terrain.terrainData.SetHeights(0, 0, heightmapData);
            // Apply SplatMap to terrain (textures)
            _texturingTerrain.SplatMap(_terrain);

            terrainHeights.Add(heightmapData);
            terrainSplatmap.Add(_terrain.terrainData.GetAlphamaps(0, 0, _terrain.terrainData.alphamapWidth, _terrain.terrainData.alphamapHeight));
        }

        // Creates default terrain data to equal current terrain data
        defaultTD = _terrain.terrainData;

        // Loop through CreateEndlessTerrain 8 times to create initial 3x3 terrain
        // CreateEndlessTerrain is called in MainController to create new terrain, this will however, loop through ensuring the player
        // is always in a 3x3 terrain
        for (int i = 0; i < 8; i++)
        {
            CreateEndlessTerrain(_terrain);
        }
    }

    private void Update()
    {
        //Check if new terrains are needed
        //See what is visible
        //If need new one then add terrain and call setmap to create heights
    }



    public void CreateEndlessTerrain(Terrain _currentTerrain)
    {
        currentTerrainPosition = _currentTerrain.GetPosition();
        // Create 8 surrounding terrains in a vector3 terrain
        surroundingTerrain = new Vector3[8];
        // Set 8 default positions around original terrain located at 0, 0, 0
        surroundingTerrain[0] = new Vector3(0, 0, defaultTD.size.z);
        surroundingTerrain[1] = new Vector3(defaultTD.size.x, 0, defaultTD.size.z);
        surroundingTerrain[2] = new Vector3(defaultTD.size.x, 0, 0);
        surroundingTerrain[3] = new Vector3(defaultTD.size.x, 0, -defaultTD.size.z);
        surroundingTerrain[4] = new Vector3(0, 0, -defaultTD.size.z);
        surroundingTerrain[5] = new Vector3(-defaultTD.size.x, 0, -defaultTD.size.z);
        surroundingTerrain[6] = new Vector3(-defaultTD.size.x, 0, 0);
        surroundingTerrain[7] = new Vector3(-defaultTD.size.x, 0, defaultTD.size.z);

        // Create 4 neighbouring terrains in a vector3 terrain (top, bottom, left, right)
        neighbouringTerrain = new Vector3[4];
        // Set 4 default positions around original terrain located at 0, 0, 0
        neighbouringTerrain[0] = new Vector3(0, 0, defaultTD.size.z);
        neighbouringTerrain[1] = new Vector3(defaultTD.size.x, 0, 0);
        neighbouringTerrain[2] = new Vector3(0, 0, -defaultTD.size.z);
        neighbouringTerrain[3] = new Vector3(-defaultTD.size.x, 0, 0);

        heightmapData = terrainHeights[terrainNumber];
        
        splatData = terrainSplatmap[terrainNumber];

        // Number of terrains is the total number of terrains
        // Terrain number is the number of the current terrain being assigned to the game
        if (terrainNumber == numOfTerrains - 1)
        {
            terrainNumber = 0;
        }
        else
        {
            terrainNumber++;
        }

        //Get all terrains into an array to check
        _terrains = Terrain.activeTerrains;

        // Loops through each of the surrounding terrain positions, checks if it exists and if not build one and only one
        bool terrainBuilt = false;
        for (int x = 0; x < 8 && terrainBuilt == false; x++)
        {
            // This checks all existing terrains to see if the position matches the surrounding position being checked
            terrainFound = false;
            for (int i = 0; i < _terrains.Length; i++)
            {
                checkTerrainPosition = _terrains[i].GetPosition();
                if (checkTerrainPosition == surroundingTerrain[x] + currentTerrainPosition)
                {
                    terrainFound = true;
                }
            }

            // This is where the new terrain is created
            if (terrainFound == false)
            {
                // Create new terrain object
                nextTerrainChunk = Terrain.CreateTerrainGameObject(new TerrainData());
                // Set position for new terrain
                nextTerrainChunk.transform.position = surroundingTerrain[x] + currentTerrainPosition;

                // Next terrain equals new terrain
                _nextTerrain = nextTerrainChunk.GetComponent<Terrain>();

                // Set next terrain values equal to default terrain data
                _nextTerrain.terrainData.terrainLayers = defaultTD.terrainLayers;
                _nextTerrain.terrainData.heightmapResolution = defaultTD.heightmapResolution;
                _nextTerrain.terrainData.baseMapResolution = defaultTD.baseMapResolution;
                _nextTerrain.terrainData.size = defaultTD.size;

                // Set heights for terrain
                _nextTerrain.terrainData.SetHeights(0, 0, heightmapData);
                // Apply SplatMap to terrain (textures)
                _nextTerrain.terrainData.SetAlphamaps(0, 0, splatData);

                // Check for neighbours
                terrainFound = false;
                for (int i = 0; i < _terrains.Length; i++)
                {
                    checkTerrainPosition = _terrains[i].GetPosition();
                    if (checkTerrainPosition == neighbouringTerrain[0] + nextTerrainChunk.transform.position)
                    {
                        terrainFound = true;
                        foundTerrainID = i;
                    }
                }
                if (terrainFound == true)
                {
                    // Smooth new terrain to neighbour
                    //_terrains[foundTerrainID]; is the terrain next to the one we just created
                    //_nextTerrain is the terrain just created
                    // Add neighbour to set neighbours
                    // Add the new terrain to the existing terrain neighbours
                }
                // Repeat above for neighbouring terrains one to three

                terrainBuilt = true;
            }
        }
        // No terrains built in this loop, turn off request new needed terrain
        if (terrainBuilt == false)
        {
            _mainController.terrainsNeeded = false;
        }
    }

    public void SetNeighbors(Terrain left, Terrain top, Terrain right, Terrain bottom)
    {
        TerrainData terrainData = _terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        float[,] leftEdgeValues = left.terrainData.GetHeights(resolution - 1, 0, 1, resolution);
        float[,] righttEdgeValues = right.terrainData.GetHeights(1, resolution, resolution - 1, 0);
        float[,] topEdgeValues = top.terrainData.GetHeights(resolution, 1, 0, resolution - 1);
        float[,] bottomEdgeValues = bottom.terrainData.GetHeights(0, resolution - 1, resolution, 1);

        terrainData.SetHeights(0, 0, leftEdgeValues);
        terrainData.SetHeights(0, 0, topEdgeValues);
        terrainData.SetHeights(0, 0, righttEdgeValues);
        terrainData.SetHeights(0, 0, bottomEdgeValues);
    }

    public void StitchToLeft(Terrain terrain, Terrain leftNeighbor)
    {
        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;

        // Take the last x-column of neighbors heightmap array
        // 1 pixel wide (single x value), resolution pixels tall (all y values)
        float[,] edgeValues = leftNeighbor.terrainData.GetHeights(resolution - 1, 0, 1, resolution);

        // Stitch with other terrain by setting same heightmap values on the edge
        data.SetHeights(0, 0, edgeValues);
    }

    public void StitchToBottom(Terrain terrain, Terrain bottomNeighbor)
    {
        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;

        // Take the top y-column of neighbors heightmap array
        // resolution pixels wide (all x values), 1 pixel tall (single y value)
        float[,] edgeValues = bottomNeighbor.terrainData.GetHeights(0, resolution - 1, resolution, 1);

        // Stitch with other terrain by setting same heightmap values on the edge
        data.SetHeights(0, 0, edgeValues);
    }

    public float[,] SetPerlinNoise(int width, int height, NoiseValues _noiseValues, Vector2 sampleCentre, int seed)
    {
        float[,] map = new float[width, height];
        
        // Randomise the seed
        System.Random randomSeed = new System.Random(seed);

        // Set octaves
        Vector2[] setOctaves = new Vector2[_noiseValues.octaves];

        // Set values for max possible height, amplitude and frequency
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Loop through octaves
        for (int i = 0; i < _noiseValues.octaves; i++)
        {
            // Randomise seed across the x and y axis between the values -100000, 100000
            float sampleX = randomSeed.Next(-100000, 100000) + _noiseValues.offset.x + sampleCentre.x;
            float sampleY = randomSeed.Next(-100000, 100000) + _noiseValues.offset.y + sampleCentre.y;
            setOctaves[i] = new Vector2(sampleX, sampleY);

            maxPossibleHeight += amplitude;
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
                // Reset amplitude and frequency to equal 1
                amplitude = 1f;
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

                    // Doing this creates a flat border around whole terrain 
                    //if (x < 0.1 * width || x > 0.9 * width || y < 0.1 * height || y > 0.9 * height)
                    //{
                    //    perlinNoiseHeight *= 0.5f;
                    //    perlinNoiseHeight = perlinNoiseHeight * 0.5f;
                    //    +(maxNoiseHeight - minNoiseHeight) / 2;

                    //}

                    // Set array map[x, y] equal to perlinNoiseHeight
                    
                }
                map[x, y] = perlinNoiseHeight;
            }
        }


        float pointheight;
        float pointheightmultiplier;
        float closenessX;  //How close to edge X as a number between 0 and 1 
        float closenessY;
        float closeness;

        // For loop to normalise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pointheight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]);

                if ((float)x / width < 0.5f)
                {
                    closenessX = (float)x / width;
                }
                else
                {
                    closenessX = 1 - (float)x / width;
                }

                if ((float)y / height < 0.5f)
                {
                    closenessY = (float)y / height;
                }
                else
                {
                    closenessY = 1 - (float)y / height;
                }

                if (closenessX < closenessY)
                {
                    closeness = closenessX;
                }
                else 
                { 
                    closeness = closenessY; 
                }

                if (closeness < 0.1)
                {
                    pointheightmultiplier = 0.5f + (closeness * 5f);
                }
                else
                {
                    pointheightmultiplier = 1;
                }

                //map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]) ;
                    
                map[x, y] = (pointheight * pointheightmultiplier) + (1 - pointheightmultiplier)/2;
            }
        }

        return map;
    }
}

[System.Serializable]
public class NoiseValues
{
    //public MapGenerator.NormaliseMode _normaliseMode;

    public float scale = 250;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
    //public int seed;
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