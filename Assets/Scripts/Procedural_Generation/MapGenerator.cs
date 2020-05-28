using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
    
    public Terrain _terrain; // terrain to modify
    public NoiseValues _noiseValues;
    public TexturingTerrain _texturingTerrain;

    public enum NormaliseMode { Local, Global };

    private float amplitude = 1;
    private float frequency = 1;
    public float GetHeight { get; set; } = 0;
    private TerrainData defaultTD;
    private GameObject secondTerrain;
    private Terrain _terrain2;
    private Vector3 chkTerrainPos;
    void Start()
    {
        // Set heights for initial terrain
        SetMap(_terrain, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain);

        // Creates default terrain data to equal current terrain data
        defaultTD = _terrain.terrainData;

        // At start of the game run, to create endless terrain
        CreateEndlessTerrain(_terrain);
       
    }

    private void Update()
    {
        //Check if new terrains are needed
        //See what is visible
        //If need new one then add terrain and call setmap to create heights

    }

    public void CreateEndlessTerrain(Terrain _currTerrain)
    {

        Vector3 curTerrainPos = _currTerrain.GetPosition();
        Vector3[] surroundingTerrain = new Vector3[8];
        surroundingTerrain[0] = new Vector3(-defaultTD.size.x, 0, defaultTD.size.z);
        surroundingTerrain[1] = new Vector3(0, 0, defaultTD.size.z);
        surroundingTerrain[2] = new Vector3(defaultTD.size.x, 0, defaultTD.size.z);
        surroundingTerrain[3] = new Vector3(defaultTD.size.x, 0, 0);
        surroundingTerrain[4] = new Vector3(defaultTD.size.x, 0, -defaultTD.size.z);
        surroundingTerrain[5] = new Vector3(0, 0, -defaultTD.size.z);
        surroundingTerrain[6] = new Vector3(-defaultTD.size.x, 0, -defaultTD.size.z);
        surroundingTerrain[7] = new Vector3(-defaultTD.size.x, 0, 0);

        //Get all terrains into an array to check
        Terrain[] _terrains = Terrain.activeTerrains;

        
        int found = 0;


        for (int x = 0; x < 8; x++)
        {

            found = 0;
            for (int i = 0; i < _terrains.Length; i++)
            {
                chkTerrainPos = _terrains[i].GetPosition();
                if (chkTerrainPos == surroundingTerrain[x] + curTerrainPos)
                {
                    found = 1;
                }
            }

            if (found == 0)
            {
                // Create news terrain object
                secondTerrain = Terrain.CreateTerrainGameObject(new TerrainData());
                // Set position for new terrain
                secondTerrain.transform.position = surroundingTerrain[x] + curTerrainPos;


                // Terrain2 equals new terrain
                _terrain2 = secondTerrain.GetComponent<Terrain>();
                

                // Set terrain 2 values equal to default terrain data
                _terrain2.terrainData.terrainLayers = defaultTD.terrainLayers;
                _terrain2.terrainData.heightmapResolution = defaultTD.heightmapResolution;
                _terrain2.terrainData.baseMapResolution = defaultTD.baseMapResolution;
                _terrain2.terrainData.size = defaultTD.size;

                // Set heights for terrain
                SetMap(_terrain2, _noiseValues, (int)(Random.value * 100));
                // Apply SplatMap to terrain (textures)
                _texturingTerrain.SplatMap(_terrain2);
            }

        }



            

        /*
        // Create news terrain object
        GameObject thirdTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        thirdTerrain.transform.position = new Vector3(0, 0, 250);

        // Terrain2 equals new terrain
        Terrain _terrain3 = thirdTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain3.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain3.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain3.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain3.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain3, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain3);

        // Create news terrain object
        GameObject forthTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        forthTerrain.transform.position = new Vector3(250, 0, 0);

        // Terrain2 equals new terrain
        Terrain _terrain4 = forthTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain4.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain4.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain4.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain4.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain4, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain4);

        // Create news terrain object
        GameObject fithTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        fithTerrain.transform.position = new Vector3(-250, 0, 0);

        // Terrain2 equals new terrain
        Terrain _terrain5 = fithTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain5.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain5.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain5.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain5.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain5, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain5);

        // Create news terrain object
        GameObject sixthTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        sixthTerrain.transform.position = new Vector3(-250, 0, 250);

        // Terrain2 equals new terrain
        Terrain _terrain6 = sixthTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain6.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain6.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain6.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain6.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain6, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain6);

        // Create news terrain object
        GameObject seventhTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        seventhTerrain.transform.position = new Vector3(-250, 0, -250);

        // Terrain2 equals new terrain
        Terrain _terrain7 = seventhTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain7.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain7.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain7.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain7.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain7, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain7);

        // Create news terrain object
        GameObject eighthTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        eighthTerrain.transform.position = new Vector3(250, 0, -250);

        // Terrain2 equals new terrain
        Terrain _terrain8 = eighthTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain8.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain8.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain8.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain8.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain8, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain8);

        // Create news terrain object
        GameObject ninthTerrain = Terrain.CreateTerrainGameObject(new TerrainData());

        // Set position for new terrain
        ninthTerrain.transform.position = new Vector3(250, 0, 250);

        // Terrain2 equals new terrain
        Terrain _terrain9 = ninthTerrain.GetComponent<Terrain>();

        // Set terrain 2 values equal to default terrain data
        _terrain9.terrainData.terrainLayers = defaultTD.terrainLayers;
        _terrain9.terrainData.heightmapResolution = defaultTD.heightmapResolution;
        _terrain9.terrainData.baseMapResolution = defaultTD.baseMapResolution;
        _terrain9.terrainData.size = defaultTD.size;

        // Set heights for terrain
        SetMap(_terrain9, _noiseValues, (int)(Random.value * 100));
        // Apply SplatMap to terrain (textures)
        _texturingTerrain.SplatMap(_terrain9);

        //SetNeighbors(_terrain, _terrain, _terrain, _terrain);

        //StitchToBottom(_terrain, _terrain2);
        //StitchToLeft(_terrain, _terrain2);
        */
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

    public void SetMap(Terrain _terrain, NoiseValues _noiseValues, int seed)
    {
        int mapWidth = _terrain.terrainData.heightmapWidth;
        int mapHeigth = _terrain.terrainData.heightmapHeight;
        Vector2 terrainCentre = new Vector2(_terrain.terrainData.size.x / 2, _terrain.terrainData.size.z / 2);

        //Update height array using Perlin Noise
        float[,] heights = SetPerlinNoise(mapWidth, mapHeigth, _noiseValues, terrainCentre, seed);

        // set the new height
        _terrain.terrainData.SetHeights(0, 0, heights);
    }

    public float[,] SetPerlinNoise(int width, int height, NoiseValues _noiseValues, Vector2 sampleCentre, int seed)
    {
        float[,] map = new float[width, height];

        // Randomise the seed
        System.Random randomSeed = new System.Random(seed);

        // Set octaves
        Vector2[] setOctaves = new Vector2[_noiseValues.octaves];

        // Loop through octaves
        for (int i = 0; i < _noiseValues.octaves; i++)
        {
            // Randomise seed across the x and y axis between the values -100000, 100000
            float sampleX = randomSeed.Next(-100000, 100000) + _noiseValues.offset.x + sampleCentre.x;
            float sampleY = randomSeed.Next(-100000, 100000) + _noiseValues.offset.y + sampleCentre.y;
            setOctaves[i] = new Vector2(sampleX, sampleY);

            GetHeight += amplitude;
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
                        map[x, y] = Mathf.Clamp(normalisedHeight, 0, maxNoiseHeight) / 25;
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

                    map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]) / 20;
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