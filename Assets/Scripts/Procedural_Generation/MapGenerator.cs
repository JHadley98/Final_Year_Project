using System.Collections.Generic;
using UnityEngine;

// Height map and noise class

public class MapGenerator : MonoBehaviour
{
    // Terrain to modify
    public Terrain _terrain;

    // Class references
    public NoiseValues _noiseValues;
    public TextureGenerator _textureGenerator;
    public MainController _mainController;
    public TerrainAssetGenerator _terrainAssetGenerator;

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
    private bool terrainOneFound;
    private bool terrainTwoFound;
    private float distToJoin;



    void Start()
    {
        // Calculate center of terrain
        terrainCentre = new Vector2(_terrain.terrainData.size.x / 2, _terrain.terrainData.size.z / 2);

        // Loop throug number of terrains applying the heightmap, splatmap and terrain heights
        for (int i = 0; i < numOfTerrains; i++)
        {
            // Set heightmapData to eqaul the perlin noise function, passing in the terrain width, height, noise values applied to map, and random value times 100 to create random terrain seeds
            heightmapData = SetPerlinNoise(_terrain.terrainData.heightmapWidth, _terrain.terrainData.heightmapHeight, _noiseValues, terrainCentre, (int)(Random.value * 100));
            // Set heights for initial terrain
            _terrain.terrainData.SetHeights(0, 0, heightmapData);
            // Apply SplatMap to terrain (textures)
            _textureGenerator.SplatMap(_terrain);

            terrainHeights.Add(heightmapData);
            terrainSplatmap.Add(_terrain.terrainData.GetAlphamaps(0, 0, _terrain.terrainData.alphamapWidth, _terrain.terrainData.alphamapHeight));
        }

        // Creates default terrain data to equal current terrain data
        defaultTD = _terrain.terrainData;

        // Call create trees function to generate trees across the 9 original generated terrains
        _terrainAssetGenerator.CreateTrees(_terrain);

        // Call create water function to generate water across the 9 original generated terrains
        _terrainAssetGenerator.CreateWater(_terrain);

        // Call create grass function to generate grass across the 9 original generated terrains
        _terrainAssetGenerator.CreateGrass(_terrain);

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
        surroundingTerrain[0] = new Vector3(0, 0, defaultTD.size.z);                    // Top terrain
        surroundingTerrain[1] = new Vector3(defaultTD.size.x, 0, defaultTD.size.z);     // Top right terrain
        surroundingTerrain[2] = new Vector3(defaultTD.size.x, 0, 0);                    // Right terrain
        surroundingTerrain[3] = new Vector3(defaultTD.size.x, 0, -defaultTD.size.z);    // Bottom right terrain
        surroundingTerrain[4] = new Vector3(0, 0, -defaultTD.size.z);                   // Bottom terrain
        surroundingTerrain[5] = new Vector3(-defaultTD.size.x, 0, -defaultTD.size.z);   // Bottom left terrain
        surroundingTerrain[6] = new Vector3(-defaultTD.size.x, 0, 0);                   // Left terrain
        surroundingTerrain[7] = new Vector3(-defaultTD.size.x, 0, defaultTD.size.z);    // Top left terrain

        // Create 4 neighbouring terrains in a vector3 terrain (top, bottom, left, right)
        neighbouringTerrain = new Vector3[4];
        // Set 4 default positions around original terrain located at 0, 0, 0
        neighbouringTerrain[0] = new Vector3(0, 0, -defaultTD.size.z); // Bottom terrain
        neighbouringTerrain[1] = new Vector3(-defaultTD.size.x, 0, 0); // Left terrain
        neighbouringTerrain[2] = new Vector3(0, 0, defaultTD.size.z);  // Top terrain
        neighbouringTerrain[3] = new Vector3(defaultTD.size.x, 0, 0); // Right terrain

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
                _nextTerrain.terrainData.treePrototypes = defaultTD.treePrototypes;

                // Set heights for terrain
                _nextTerrain.terrainData.SetHeights(0, 0, heightmapData);
                // Apply SplatMap to terrain (textures)
                _nextTerrain.terrainData.SetAlphamaps(0, 0, splatData);

                // Check for neighbours


                //Now check left join 
                terrainFound = false;
                for (int i = 0; i < _terrains.Length; i++)
                {
                    checkTerrainPosition = _terrains[i].GetPosition();
                    if (checkTerrainPosition == neighbouringTerrain[1] + nextTerrainChunk.transform.position)
                    {
                        terrainFound = true;
                        foundTerrainID = i;
                    }
                }
                if (terrainFound == true)
                {
                    // Smooth new terrain to neighbour
                    // Stitch edges of neighbouring terrain to smooth out the terrain
                    //_nextTerrain is the terrain just created
                    //_terrains[foundTerrainID]; is the terrain next to the one we just created
                    StitchToLeft(_nextTerrain, _terrains[foundTerrainID]);

                    // Add neighbour to set neighbours
                    _nextTerrain.SetNeighbors(_terrains[foundTerrainID], _nextTerrain.topNeighbor, _nextTerrain.rightNeighbor, _nextTerrain.bottomNeighbor);
                    // Add the new terrain to the existing terrain neighbours
                    _terrains[foundTerrainID].SetNeighbors(_terrains[foundTerrainID].leftNeighbor, _terrains[foundTerrainID].topNeighbor, _nextTerrain, _terrains[foundTerrainID].bottomNeighbor);
                }

                //Now check top join 
                terrainFound = false;
                for (int i = 0; i < _terrains.Length; i++)
                {
                    checkTerrainPosition = _terrains[i].GetPosition();
                    if (checkTerrainPosition == neighbouringTerrain[2] + nextTerrainChunk.transform.position)
                    {
                        terrainFound = true;
                        foundTerrainID = i;
                    }
                }
                if (terrainFound == true)
                {
                    // Smooth new terrain to neighbour
                    // Stitch edges of neighbouring terrain to smooth out the terrain
                    //need to be aware of other terrains at corners neighbouringTerrains[]


                    //_nextTerrain is the terrain just created
                    //_terrains[foundTerrainID]; is the terrain next to the one we just created
                    StitchToTop(_nextTerrain, _terrains[foundTerrainID]);

                    // Add neighbour to set neighbours
                    _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, _terrains[foundTerrainID], _nextTerrain.rightNeighbor, _nextTerrain.bottomNeighbor);
                    // Add the new terrain to the existing terrain neighbours
                    _terrains[foundTerrainID].SetNeighbors(_terrains[foundTerrainID].leftNeighbor, _terrains[foundTerrainID].topNeighbor, _terrains[foundTerrainID].rightNeighbor, _nextTerrain);
                }


                // Bottom first
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
                    // Stitch edges of neighbouring terrain to smooth out the terrain
                    //need to be aware of other terrains at corners neighbouringTerrains[]


                    terrainOneFound = false;
                    for (int i = 0; i < _terrains.Length; i++)
                    {
                        checkTerrainPosition = _terrains[i].GetPosition();
                        if (checkTerrainPosition == neighbouringTerrain[1] + nextTerrainChunk.transform.position)
                        {
                            terrainOneFound = true;
                        }
                    }
                    terrainTwoFound = false;
                    for (int i = 0; i < _terrains.Length; i++)
                    {
                        checkTerrainPosition = _terrains[i].GetPosition();
                        if (checkTerrainPosition == neighbouringTerrain[3] + nextTerrainChunk.transform.position)
                        {
                            terrainTwoFound = true;
                        }
                    }
                    StitchToBottom(_nextTerrain, _terrains[foundTerrainID], terrainOneFound, terrainTwoFound);

                    // Add neighbour to set neighbours
                    //        _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, _nextTerrain.topNeighbor, _nextTerrain.rightNeighbor, _terrains[foundTerrainID]);
                    // Add the new terrain to the existing terrain neighbours
                    //        _terrains[foundTerrainID].SetNeighbors(_terrains[foundTerrainID].leftNeighbor, _nextTerrain, _terrains[foundTerrainID].rightNeighbor, _terrains[foundTerrainID].bottomNeighbor);
                }


                //Now check right join 
                terrainFound = false;
                for (int i = 0; i < _terrains.Length; i++)
                {
                    checkTerrainPosition = _terrains[i].GetPosition();
                    if (checkTerrainPosition == neighbouringTerrain[3] + nextTerrainChunk.transform.position)
                    {
                        terrainFound = true;
                        foundTerrainID = i;
                    }
                }
                if (terrainFound == true)
                {
                    // Smooth new terrain to neighbour
                    // Stitch edges of neighbouring terrain to smooth out the terrain
                    //_nextTerrain is the terrain just created
                    //_terrains[foundTerrainID]; is the terrain next to the one we just created

                    terrainOneFound = false;
                    for (int i = 0; i < _terrains.Length; i++)
                    {
                        checkTerrainPosition = _terrains[i].GetPosition();
                        if (checkTerrainPosition == neighbouringTerrain[0] + nextTerrainChunk.transform.position)
                        {
                            terrainOneFound = true;
                        }
                    }
                    terrainTwoFound = false;
                    for (int i = 0; i < _terrains.Length; i++)
                    {
                        checkTerrainPosition = _terrains[i].GetPosition();
                        if (checkTerrainPosition == neighbouringTerrain[2] + nextTerrainChunk.transform.position)
                        {
                            terrainTwoFound = true;
                        }
                    }
                    StitchToRight(_nextTerrain, _terrains[foundTerrainID], terrainOneFound, terrainTwoFound);

                    if (terrainOneFound == true)
                    {
                        StitchBottomRight(_nextTerrain);
                    }

                    // Add neighbour to set neighbours
                    //           _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, _nextTerrain.topNeighbor, _terrains[foundTerrainID], _nextTerrain.bottomNeighbor);
                    // Add the new terrain to the existing terrain neighbours
                    //             _terrains[foundTerrainID].SetNeighbors(_nextTerrain, _terrains[foundTerrainID].topNeighbor, _terrains[foundTerrainID].rightNeighbor, _terrains[foundTerrainID].bottomNeighbor);
                }
                
                // Create tress across endless terrain
                _terrainAssetGenerator.CreateTrees(_nextTerrain);
                
                // Create water across endless terrain
                _terrainAssetGenerator.CreateWater(_nextTerrain);
                
                // Create grass across endless terrain
                _terrainAssetGenerator.CreateGrass(_terrain);
                
                terrainBuilt = true;
            }
        }
        // No terrains built in this loop, turn off request new needed terrain
        if (terrainBuilt == false)
        {
            _mainController.terrainsNeeded = false;
        }
        //treeGeneration.GetTrees(_nextTerrain);
    }

    public void StitchBottomRight(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        // Get current values of bottom 10% of heights to be smoothed
        int xSize = (int)(terrainData.heightmapWidth * 0.1f) * 2;               // 10% of Width
        int ySize = (int)(terrainData.heightmapHeight * 0.1f);              // 10% of height
        float[,] startValues = terrainData.GetHeights(terrainData.heightmapWidth - xSize, 0, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                        // Create array for new heights

        float distToX1;
        float distToX2;
        float distToY1;
        float distToY2;
        float influenceX1;
        float influenceX2;
        float influenceY1;
        float influenceY2;
        float influenceSum;
        float influenceMax;
        float influenceTotalHeight;
        float avX1;
        float avX2;
        float avY1;
        float avY2;

        for (int x = 0; x < xSize; x++)
        {


            for (int y = 0; y < ySize; y++)
            {
                if (y == 2)
                {
                    y = y + 0;
                    influenceTotalHeight = startValues[y, x];
                }
                distToX1 = y;
                distToX2 = ySize - 1 - y;
                influenceX1 = 1f - Mathf.InverseLerp(0, (ySize - 1) / 2 - 5, distToX1);
                influenceX2 = 1f - Mathf.InverseLerp(0, (ySize - 1) / 2 - 5, distToX2);


                distToY1 = x;
                distToY2 = xSize - 1 - x;
                influenceY1 = 1f - Mathf.InverseLerp(0, (xSize - 1) / 2 - 5, distToY1);
                influenceY2 = 1f - Mathf.InverseLerp(0, (xSize - 1) / 2 - 5, distToY2);


                //influenceMax = Mathf.Max(influenceX1, influenceX2, influenceY1, influenceY2);
                influenceSum = influenceX1 + influenceX2 + influenceY1 + influenceY2;
                // influenceTotalHeight = influenceX1 * startValues[y,0] + influenceX2 * startValues[y,xSize-1] + influenceY1 * startValues[0,x] +influenceY2 * startValues[ySize-1,x];

                avX1 = (influenceX1 * startValues[0, x]) + ((1 - influenceX1) * startValues[y, x]);
                avX2 = (influenceX2 * startValues[ySize - 1, x]) + ((1 - influenceX2) * startValues[y, x]);
                avY1 = (influenceY1 * startValues[y, 0]) + ((1 - influenceY1) * startValues[y, x]);
                avY2 = (influenceY2 * startValues[y, xSize - 1]) + ((1 - influenceY2) * startValues[y, x]);



                if (x == 0 || y == 0 || x == xSize - 1 || y == ySize - 1)
                {
                    endValues[y, x] = startValues[y, x];
                }
                else if (influenceSum <= 0)
                {
                    endValues[y, x] = startValues[y, x];
                }
                else
                {
                    endValues[y, x] = (avX1 * influenceX1 / influenceSum) + (avX2 * influenceX2 / influenceSum) + (avY1 * influenceY1 / influenceSum) + (avY2 * influenceY2 / influenceSum);
                    //  endValues[y, x] = 1.0f;
                }


            }
        }
        terrainData.SetHeights(terrainData.heightmapWidth - xSize, 0, endValues);

    }

    public void StitchToBottom(Terrain terrain, Terrain bottomTerrain, bool leftTerrainFound, bool rightTerrainFound)
    {
        /// Step 1:
        /// Take the last row of bottom terrain and apply heights to first row of new terrain
        /// 

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        // Take the top y-column of neighbors heightmap array
        // resolution pixels wide (all x values), 1 pixel tall (single y value)
        float[,] edgeValues = bottomTerrain.terrainData.GetHeights(0, resolution - 1, resolution, 1);

        // Stitch with other terrain by setting same heightmap values on the edge
        terrainData.SetHeights(0, 0, edgeValues);

        /// 
        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of bottom 10% of heights to be smoothed
        int xSize = terrainData.heightmapWidth;                             // Full width of terrain
        int ySize = (int)(terrainData.heightmapHeight * 0.1f);              // 10% of height
        float[,] startValues = terrainData.GetHeights(0, 0, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                        // Create array for new heights

        // Smooth out terrain based on a propotion of original terrain and the edge with the neighbour
        float propotionOfOriginal;                                          // Propotion of original height to be used in the new height
        float propotionOfTarget;                                            // Propotion of target height to be used in the new height

        // To avoid a sudden change at 10% mark we vary the percentage where the change starts
        // This is based on the difference in height between edge and 10% mark
        float variationValue;                                               // Value for the variation in the blend
        float blendLimit;                                                   // The actual limit that the blending is done up to 
        float heightDiff;                                                   // Difference in height where heights are always between 0 and 1

        // Loop through all the X's and Y's in the startValues array to workout the new values
        for (int x = 0; x < xSize; x++)
        {
            // Difference in height between start and end
            heightDiff = Mathf.Abs(startValues[0, x] - startValues[ySize - 1, x]);

            // Variation value is based on height difference, big height differences give big values, so blending is over a bigger distance
            variationValue = Mathf.InverseLerp(0, 0.6f, heightDiff);

            // Blend limit is 75% to 100% of blend region (ySize - 1, needs -1 as counting starts at 0)
            blendLimit = (0.75f * (ySize - 1)) + (0.25f * (ySize - 1) * variationValue);

            for (int y = 0; y < ySize; y++)
            {

                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, y);  // InverseLerp gives a decimal 0 to 1 of where Y is from 0 to blend limit

                if (propotionOfOriginal > 1)                                // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }

                propotionOfTarget = 1 - propotionOfOriginal;                // Propotion of target and original always add up to 1

                // Workout new height
                // startValues 0,x is the height at the edge of the terrain which is blended to
                if (x < ySize && leftTerrainFound == true)
                {
                    endValues[y, x] = startValues[y, x];
                }
                else if (x > xSize - ySize && rightTerrainFound == true)
                {
                    endValues[y, x] = startValues[y, x];
                }
                else
                {
                    endValues[y, x] = (propotionOfOriginal * startValues[y, x]) + (propotionOfTarget * startValues[0, x]);
                }


            }
        }

        // Apply new heights to terrain
        terrainData.SetHeights(0, 0, endValues);


        //New 3
        //float[,] startStitchValues1 = terrainData.GetHeights(0, 0, xSize, 2);   // Put current stitch values to into an array (terrain)
        //float[,] endStitchValues1 = new float[1, xSize];                        // Create array to store new heights for the end of the stitch (terrain)

        //// Start stitch values for bottom terrain, setting the yBase to be the max heightmap height of the bottom terrain -ySize (2% of the terrain height being stitched)
        //float[,] startStitchValues2 = bottomTerrain.terrainData.GetHeights(0, bottomTerrain.terrainData.heightmapHeight - 2, xSize, 2);

        //// Store new heights for the end of the stitch (bottomTerrain)
        //float[,] endStitchValues2 = new float[1, xSize];

        //for (int x = 0; x < xSize; x++)
        //{
        //    endStitchValues1 [0,x] = (startStitchValues1[1,x] + startStitchValues2[0,x] ) / 2;
        //    endStitchValues2[0, x] = endStitchValues1[0, x];
        //}
        //// Apply new heights to terrain
        //terrainData.SetHeights(0, 0, endStitchValues1);
        //bottomTerrain.terrainData.SetHeights(0, bottomTerrain.terrainData.heightmapHeight - 1, endStitchValues2);



        /// 
        /// Step 3:
        /// Blend heights 2% either side of join with a target that is an average of the heights from both terrains and the seam height
        /// This is to avoid sudden changes in direction
        /// 


        ///
        /// Step 4:
        /// Blend splatmaps from 2% either side of join
        /// 

        // Update part of splatmap to reflect the change in heights
        //_texturingTerrain.PartialSplatMap(terrainData, 0, 0, (int)(terrainData.alphamapWidth * 0.1f), terrainData.alphamapHeight);
        _textureGenerator.SplatMap(terrain);
        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);          // Put the current terrains splatmap width and height into an array
        float[,,] splatMap2 = bottomTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);    // Put the bottom terrains splatmap width and height into an array

        int numOfSplats = terrain.terrainData.terrainLayers.Length;                                     // Set number of splats available equal to the terrainlayers length (6)
        int ySplatMapPart = (int)(splatHeight * 0.02f);                                                 // Blend the splatmaps from both terrains at 0.02 from join

        // Loop through all the X's and Y's in the splatmap
        for (int y = 0; y < ySplatMapPart; y++)
        {
            propotionOfOriginal = (Mathf.InverseLerp(0, ySplatMapPart, y) / 2) + 0.5f;  // InverseLerp gives a decimal 0 to 1 of where Y is from ySplatMapPart(blend 2% splatmap height)
            propotionOfTarget = 1 - propotionOfOriginal;                                // Propotion of target and original always add up to 1
            for (int x = 0; x < splatWidth; x++)
            {
                for (int j = 0; j < numOfSplats; j++)
                {
                    // Update splatmap based on propotion of original side and other side of seam
                    splatMap1[y, x, j] = propotionOfOriginal * splatMap1[(int)(propotionOfOriginal * ySplatMapPart), x, j] +
                                         propotionOfTarget * splatMap2[splatHeight - 1 - (int)(propotionOfTarget * ySplatMapPart), x, j];

                    // Repeat for splatmap on other side of seam
                    splatMap2[splatHeight - 1 - y, x, j] = propotionOfTarget * splatMap1[(int)(propotionOfTarget * ySplatMapPart), x, j] +
                                                           propotionOfOriginal * splatMap2[splatHeight - 1 - (int)(propotionOfOriginal * ySplatMapPart), x, j];
                }
            }
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        // Apply bottom terrain splatmap
        bottomTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    public void StitchToTop(Terrain terrain, Terrain topTerrain)
    {
        // Smooth out terrain based on a propotion of original terrain and the edge with the neighbour
        float propotionOfOriginal;                                          // Propotion of original height to be used in the new height
        float propotionOfTarget;                                            // Propotion of target height to be used in the new height

        // To avoid a sudden change at 10% mark we vary the percentage where the change starts
        // This is based on the difference in height between edge and 10% mark
        float variationValue;                                               // Value for the variation in the blend
        float blendLimit;                                                   // The actual limit that the blending is done up to 
        float heightDiff;                                                   // Difference in height where heights are always between 0 and 1
        /// Step 1:
        /// Take the last row of bottom terrain and apply heights to first row of new terrain
        /// 

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        // Take the top y-column of neighbors heightmap array
        // resolution pixels wide (all x values), 1 pixel tall (single y value)
        float[,] edgeValues = topTerrain.terrainData.GetHeights(0, 0, topTerrain.terrainData.heightmapWidth, 1);

        // Stitch with other terrain by setting same heightmap values on the edge
        terrainData.SetHeights(0, terrainData.heightmapWidth - 1, edgeValues);

        /// 
        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of bottom 10% of heights to be smoothed
        int xSize = terrainData.heightmapWidth;                             // Full width of terrain
        int ySize = (int)(terrainData.heightmapHeight * 0.1f);              // 10% of height
        float[,] startValues = terrainData.GetHeights(0, terrainData.heightmapHeight - ySize, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                       // Create array for new heights



        // Loop through all the X's and Y's in the startValues array to workout the new values
        for (int x = 0; x < xSize; x++)
        {
            // Difference in height between start and end
            heightDiff = Mathf.Abs(startValues[0, x] - startValues[ySize - 1, x]);

            // Variation value is based on height difference, big height differences give big values, so blending is over a bigger distance
            variationValue = Mathf.InverseLerp(0, 0.6f, heightDiff);
            // Limit variation value to 1
            if (variationValue > 1)
            {
                variationValue = 1;
            }

            // Blend limit is 75% to 100% of blend region (ySize - 1, needs -1 as counting starts at 0)
            blendLimit = (0.75f * (ySize - 1)) + (0.25f * (ySize - 1) * variationValue);

            for (int y = 0; y < ySize; y++)
            {
                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, ySize - 1 - y); // InverseLerp gives a decimal 0 to 1 of where Y is from 0 to blend limit, ysize-y used as y=0 is 100% original

                if (propotionOfOriginal > 1)                                            // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }
                propotionOfTarget = 1 - propotionOfOriginal;                            // Propotion of target and original always add up to 1

                // Workout new height
                // startValues 0,x is the height at the edge of the terrain which is blended to
                endValues[y, x] = (propotionOfOriginal * startValues[y, x]) + (propotionOfTarget * startValues[ySize - 1, x]);
            }
        }

        // Apply new heights to terrain
        terrainData.SetHeights(0, terrainData.heightmapHeight - ySize, endValues);

        /// 
        /// Step 3:
        /// Blend heights 2% either side of join with a target that is an average of the heights from both terrains and the seam height
        /// This is to avoid sudden changes in direction
        /// 

        float[,] startStitchValues1 = terrainData.GetHeights(0, terrainData.heightmapHeight - 2, xSize, 2);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[1, xSize];                        // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for bottom terrain, setting the yBase to be the max heightmap height of the bottom terrain -ySize (2% of the terrain height being stitched)
        float[,] startStitchValues2 = topTerrain.terrainData.GetHeights(0, 0, xSize, 2);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[1, xSize];

        for (int x = 0; x < xSize; x++)
        {
            endStitchValues1[0, x] = (startStitchValues1[0, x] + startStitchValues2[1, x]) / 2;
            endStitchValues2[0, x] = endStitchValues1[0, x];
        }
        // Apply new heights to terrain
        terrainData.SetHeights(0, terrainData.heightmapHeight - 1, endStitchValues1);
        topTerrain.terrainData.SetHeights(0, 0, endStitchValues2);

        /*
        ySize = (int)(terrainData.heightmapHeight * 0.02f);                         // Update ySize to do 2% to fix stitching
        float[,] startStitchValues1 = terrainData.GetHeights(0, terrainData.heightmapHeight - 1 - ySize, xSize, ySize);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[ySize, xSize];                        // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for bottom terrain, setting the yBase to be the max heightmap height of the topTerrain -ySize (2% of the terrain height being stitched)
        float[,] startStitchValues2 = topTerrain.terrainData.GetHeights(0, 0, xSize, ySize);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[ySize, xSize];

        float targetStitchHeight;

        // Loop through all the X's and Y's in the start stitch values
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                blendLimit = ySize - 1;
                if (x < ySize && blendLimit > x)
                {
                    blendLimit = y;
                }
                if (x > xSize - ySize && blendLimit > xSize - 1 - x)
                {
                    blendLimit = y;
                }

                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, y);   // InverseLerp gives a decimal 0 to 1 of where Y is from ySize(2% of heightmap height) - 1
                if (propotionOfOriginal > 1)                                 // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }
                propotionOfTarget = 1 - propotionOfOriginal;                // Propotion of target and original always add up to 1

                // Take the average of heights from the join and 2% either side to give a target height to aim for
                targetStitchHeight = (startStitchValues1[ySize - 1, x] + startStitchValues1[0, x] + startStitchValues2[0, x]) / 3;
                // Workout new height
                // startStitchValues1 blends the stitch from the original propotion plus the propotion target(terrain) times by the targetStitchHeight
                endStitchValues1[y, x] = (propotionOfTarget * startStitchValues1[y, x]) + (propotionOfOriginal * targetStitchHeight);
                // startStitchValues1 blends the stitch from the original propotion plus the propotion target(bottom terrain) times by the targetStitchHeight
                endStitchValues2[y, x] = (propotionOfOriginal * startStitchValues2[y, x]) + (propotionOfTarget * targetStitchHeight);
            }
        }
        // Apply new heights to both terrains
        terrainData.SetHeights(0, terrainData.heightmapHeight - ySize, endStitchValues1);
        topTerrain.terrainData.SetHeights(0, 0, endStitchValues2);
        */

        /// 
        /// Step 4:
        /// Blend splatmaps from 2% either side of join
        /// 

        // Update part of splatmap to reflect the change in heights from above code
        // _texturingTerrain.PartialSplatMap(terrainData,  terrainData.alphamapHeight - 1 - (int)(terrainData.alphamapHeight * 0.1f), 0, (int)(terrainData.alphamapWidth * 0.1f), terrainData.alphamapHeight);
        _textureGenerator.SplatMap(terrain);
        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);          // Assign the current terrains splatmap width and height into an array
        float[,,] splatMap2 = topTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);       // Assign the topTerrains splatmap width and height into an array

        int numOfSplats = terrain.terrainData.terrainLayers.Length;                                     // Set number of splats available equal to the terrainlayers length (6)
        int ySplatMapPart = (int)(splatHeight * 0.02f);                                                 // Blend the splatmaps from both terrains at 0.02 from join

        // Loop through all the X's and Y's in the splatmap
        for (int y = 0; y < ySplatMapPart; y++)
        {
            propotionOfOriginal = (Mathf.InverseLerp(0, ySplatMapPart, y) / 2) + 0.5f;  // InverseLerp gives a decimal 0.5 to 1 of where Y is from ySplatMapPart(blend 2% splatmap height)
            propotionOfTarget = 1 - propotionOfOriginal;                                // Propotion of target and original always add up to 1
            for (int x = 0; x < splatWidth; x++)
            {
                for (int j = 0; j < numOfSplats; j++)
                {
                    // Update splatmap based on propotion of original side and other side of seam
                    splatMap1[splatHeight - 1 - y, x, j] = propotionOfOriginal * splatMap1[splatHeight - 1 - (int)(propotionOfOriginal * ySplatMapPart), x, j] +
                                                           propotionOfTarget * splatMap2[(int)(propotionOfTarget * ySplatMapPart), x, j];

                    // Repeat for splatmap on other side of seam
                    splatMap2[y, x, j] = propotionOfTarget * splatMap1[splatHeight - 1 - (int)(propotionOfTarget * ySplatMapPart), x, j] +
                                         propotionOfOriginal * splatMap2[(int)(propotionOfOriginal * ySplatMapPart), x, j];
                }
            }
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        // Apply bottom terrain splatmap
        topTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    public void StitchToRight(Terrain terrain, Terrain rightTerrain, bool bottomTerrainFound, bool topTerrainFound)
    {

        // Smooth out terrain based on a propotion of original terrain and the edge with the neighbour
        float propotionOfOriginal;                                          // Propotion of original height to be used in the new height
        float propotionOfTarget;                                            // Propotion of target height to be used in the new height

        // To avoid a sudden change at 10% mark we vary the percentage where the change starts
        // This is based on the difference in height between edge and 10% mark
        float variationValue;                                               // Value for the variation in the blend
        float blendLimit;                                                   // The actual limit that the blending is done up to 
        float heightDiff;                                                   // Difference in height where heights are always between 0 and 1

        /// Step 1:
        /// Take the last row of bottom terrain and apply heights to first row of new terrain
        /// 

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        // Take the top y-column of neighbors heightmap array
        // resolution pixels wide (all x values), 1 pixel tall (single y value)
        float[,] edgeValues = rightTerrain.terrainData.GetHeights(0, 0, 1, resolution);

        // Stitch with other terrain by setting same heightmap values on the edge
        terrainData.SetHeights(resolution - 1, 0, edgeValues);

        /// 
        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of right 10% of widths to be smoothed
        int xSize = (int)(terrainData.heightmapWidth * 0.1f);                                                    // 10% of width
        int ySize = terrainData.heightmapHeight;                                                                 // Full height
        float[,] startValues = terrainData.GetHeights(terrainData.heightmapWidth - xSize, 0, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                                                            // Create array for new heights



        // Loop through all the X's and Y's in the startValues array to workout the new values
        for (int y = 0; y < ySize; y++)
        {
            // Difference in height between start and end
            heightDiff = Mathf.Abs(startValues[y, 0] - startValues[y, xSize - 1]);

            // Variation value is based on height difference, big height differences give big values, so blending is over a bigger distance (max value 1)
            variationValue = Mathf.InverseLerp(0, 0.6f, heightDiff);

            // Blend limit is 75% to 100% of blend region (xSize - 1, needs -1 as counting starts at 0)
            blendLimit = (0.75f * (xSize - 1)) + (0.25f * (xSize - 1) * variationValue);

            for (int x = 0; x < xSize; x++)
            {
                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, xSize - 1 - x);  // InverseLerp gives a decimal 0 to 1 of where X is from 0 to blend limit

                if (propotionOfOriginal > 1)                                            // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }
                propotionOfTarget = 1 - propotionOfOriginal;                            // Propotion of target and original always add up to 1

                // Workout new height
                // startValues 0,x is the height at the edge of the terrain which is blended to
                // This does not get applied in the first 10% or last 10% if there is a terrain on that neighbouring edge
                if (y < xSize && bottomTerrainFound == true)
                {
                    endValues[y, x] = startValues[y, x];

                }
                else if (y > ySize - xSize && topTerrainFound == true)
                {
                    endValues[y, x] = startValues[y, x];

                }
                else
                {
                    endValues[y, x] = (propotionOfOriginal * startValues[y, x]) + (propotionOfTarget * startValues[y, xSize - 1]);
                }
            }
        }

        // Apply new heights to terrain
        terrainData.SetHeights(terrainData.heightmapWidth - xSize, 0, endValues);

        /// 
        /// Step 3:
        /// Blend heights 2% either side of join with a target that is an average of the heights from both terrains and the seam height
        /// This is to avoid sudden changes in direction
        /// 


        //float[,] startStitchValues1 = terrainData.GetHeights( terrainData.heightmapWidth - 2, 0, 2, ySize);   // Put current stitch values to into an array (terrain)
        //float[,] endStitchValues1 = new float[ySize, 1];                        // Create array to store new heights for the end of the stitch (terrain)

        //// Start stitch values for right terrain, xbase and ybase both 0 as array starts at bottom left of the right terrain
        //float[,] startStitchValues2 = rightTerrain.terrainData.GetHeights(0, 0, 2, ySize);

        //// Store new heights for the end of the stitch (bottomTerrain)
        //float[,] endStitchValues2 = new float[ySize, 1];

        //for (int y = 0; y < ySize; y++)
        //{
        //    endStitchValues1[y, 0] = (startStitchValues1[y, 0] + startStitchValues2[y, 1]) / 2;
        //    endStitchValues2[y, 0] = endStitchValues1[y, 0];
        //}
        //// Apply new heights to terrain
        //terrainData.SetHeights( terrainData.heightmapWidth - 1, 0, endStitchValues1);
        //rightTerrain.terrainData.SetHeights(0, 0, endStitchValues2);



        /// 
        /// Step 4:
        /// Blend splatmaps from 2% either side of join
        /// 

        // Update part of splatmap to reflect the change in heights
        //_texturingTerrain.PartialSplatMap(terrainData, 0, terrainData.alphamapWidth - 1 - (int)(terrainData.alphamapWidth * 0.1f),  terrainData.alphamapHeight, (int)(terrainData.alphamapWidth * 0.1f) );
        _textureGenerator.SplatMap(terrain);

        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);      // Assign the current terrains splatmap width and height into an array
        float[,,] splatMap2 = rightTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight); // Assign the topTerrains splatmap width and height into an array

        int numOfSplats = terrain.terrainData.terrainLayers.Length;                                 // Set number of splats available equal to the terrainlayers length (6)
        int xSplatMapPart = (int)(splatWidth * 0.02f);                                              // Blend the splatmaps from both terrains at 0.02 from join

        // Loop through all the X's and Y's in the splatmap
        for (int x = 0; x < xSplatMapPart; x++)
        {
            propotionOfOriginal = (Mathf.InverseLerp(0, xSplatMapPart, x) / 2) + 0.5f;  // InverseLerp gives a decimal 0.5 to 1 of where X is from xSplatMapPart(blend 2% splatmap height)
            propotionOfTarget = 1 - propotionOfOriginal;                                // Propotion of target and original always add up to 1

            for (int y = 0; y < splatHeight; y++)
            {
                for (int j = 0; j < numOfSplats; j++)
                {
                    // Update splatmap based on propotion of original side and other side of seam
                    splatMap1[y, splatWidth - 1 - x, j] = propotionOfOriginal * splatMap1[y, splatWidth - 1 - (int)(propotionOfOriginal * xSplatMapPart), j] +
                                                          propotionOfTarget * splatMap2[y, (int)(propotionOfTarget * xSplatMapPart), j];

                    // Repeat for other splatmap
                    splatMap2[y, x, j] = propotionOfTarget * splatMap1[y, splatWidth - 1 - (int)(propotionOfTarget * xSplatMapPart), j] +
                                         propotionOfOriginal * splatMap2[y, (int)(propotionOfOriginal * xSplatMapPart), j];
                }
            }
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        // Apply bottom terrain splatmap
        rightTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    public void StitchToLeft(Terrain terrain, Terrain leftTerrain)
    {
        /// Step 1:
        /// Take the last row of bottom terrain and apply heights to first row of new terrain
        /// 

        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;

        // Take the top y-column of neighbors heightmap array
        // resolution pixels wide (all x values), 1 pixel tall (single y value)
        float[,] edgeValues = leftTerrain.terrainData.GetHeights(resolution - 1, 0, 1, resolution);

        // Stitch with other terrain by setting same heightmap values on the edge
        terrainData.SetHeights(0, 0, edgeValues);

        /// 
        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of right 10% of widths to be smoothed
        int xSize = (int)(terrainData.heightmapWidth * 0.1f);               // 10% of width
        int ySize = terrainData.heightmapHeight;                            // Full height
        float[,] startValues = terrainData.GetHeights(0, 0, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                       // Create array for new heights

        // Smooth out terrain based on a propotion of original terrain and the edge with the neighbour
        float propotionOfOriginal;                                          // Propotion of original height to be used in the new height
        float propotionOfTarget;                                            // Propotion of target height to be used in the new height

        // To avoid a sudden change at 10% mark we vary the percentage where the change starts
        // This is based on the difference in height between edge and 10% mark
        float variationValue;                                               // Value for the variation in the blend
        float blendLimit;                                                   // The actual limit that the blending is done up to 
        float heightDiff;                                                   // Difference in height where heights are always between 0 and 1

        // Loop through all the X's and Y's in the startValues array to workout the new values
        for (int y = 0; y < ySize; y++)
        {
            // Difference in height between start and end
            heightDiff = Mathf.Abs(startValues[y, 0] - startValues[y, xSize - 1]);

            // Variation value is based on height difference, big height differences give big values, so blending is over a bigger distance
            variationValue = Mathf.InverseLerp(0, 0.6f, heightDiff);

            // Limit variation value to 1
            if (variationValue > 1)
            {
                variationValue = 1;
            }

            // Blend limit is 75% to 100% of blend region (xSize - 1, needs -1 as counting starts at 0)
            blendLimit = (0.75f * (xSize - 1)) + (0.25f * (xSize - 1) * variationValue);

            for (int x = 0; x < xSize; x++)
            {
                /*
                if (y < xSize && blendLimit > y)
                {
                    blendLimit = x;
                }
                if (y > ySize - xSize && blendLimit > ySize - 1 - y)
                {
                    blendLimit = x;
                }
                */
                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, x);  // InverseLerp gives a decimal 0 to 1 of where X is from 0 to blend limit

                if (propotionOfOriginal > 1)                                // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }
                propotionOfTarget = 1 - propotionOfOriginal;                // Propotion of target and original always add up to 1

                // Workout new height
                // startValues 0,x is the height at the edge of the terrain which is blended to
                endValues[y, x] = (propotionOfOriginal * startValues[y, x]) + (propotionOfTarget * startValues[y, 0]);
            }
        }

        // Apply new heights to terrain
        terrainData.SetHeights(0, 0, endValues);

        /// 
        /// Step 3:
        /// Blend heights 2% either side of join with a target that is an average of the heights from both terrains and the seam height
        /// This is to avoid sudden changes in direction
        /// 


        float[,] startStitchValues1 = terrainData.GetHeights(0, 0, 2, ySize);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[ySize, 1];                        // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for right terrain, xbase and ybase both 0 as array starts at bottom left of the right terrain
        float[,] startStitchValues2 = leftTerrain.terrainData.GetHeights(leftTerrain.terrainData.heightmapWidth - 2, 0, 2, ySize);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[ySize, 1];

        for (int y = 0; y < ySize; y++)
        {
            endStitchValues1[y, 0] = (startStitchValues1[y, 1] + startStitchValues2[y, 0]) / 2;
            endStitchValues2[y, 0] = endStitchValues1[y, 0];
        }
        // Apply new heights to terrain
        terrainData.SetHeights(0, 0, endStitchValues1);
        leftTerrain.terrainData.SetHeights(leftTerrain.terrainData.heightmapWidth - 1, 0, endStitchValues2);





        /*
        xSize = (int)(terrainData.heightmapWidth * 0.02f);                          // Update xSize to do 2% to fix stitching
        float[,] startStitchValues1 = terrainData.GetHeights(0, 0, xSize, ySize);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[ySize, xSize];                        // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for right terrain, xbase and ybase both 0 as array starts at bottom left of the right terrain
        float[,] startStitchValues2 = leftTerrain.terrainData.GetHeights(terrainData.heightmapWidth - 1 - xSize, 0, xSize, ySize);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[ySize, xSize];

        float targetStitchHeight;

        // Loop through all the X's and Y's in the start stitch values
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                blendLimit = xSize - 1;
                if (y < xSize && blendLimit > y)
                {
                    blendLimit = x;
                }
                if (y > ySize - xSize && blendLimit > ySize - 1 - y)
                {
                    blendLimit = x;
                }

                propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, y);  // InverseLerp gives a decimal 0 to 1 of where Y is from ySize(2% of heightmap height) - 1
                if (propotionOfOriginal > 1)                                // Propotion of the original cannot be more than 1
                {
                    propotionOfOriginal = 1;
                }
                propotionOfTarget = 1 - propotionOfOriginal;                // Propotion of target and original always add up to 1

                // Target height is average of the current join and 2% either side of that
                targetStitchHeight = (startStitchValues1[y, xSize - 1] + startStitchValues1[y, 0] + startStitchValues2[y, 0]) / 3;
                
                // Workout new height
                // startStitchValues1 blends the stitch from the original propotion plus the propotion target(terrain) times by the targetStitchHeight
                endStitchValues1[y, x] = (propotionOfOriginal * startStitchValues1[y, x]) + (propotionOfTarget * targetStitchHeight);
                
                // startStitchValues1 blends the stitch from the original propotion plus the propotion target(bottom terrain) times by the targetStitchHeight
                endStitchValues2[y, x] = (propotionOfTarget * startStitchValues2[y, x]) + (propotionOfOriginal * targetStitchHeight);
            }
        }
        // Apply new heights to both terrains
        terrainData.SetHeights(0, 0, endStitchValues1);
        leftTerrain.terrainData.SetHeights(terrainData.heightmapWidth - xSize, 0, endStitchValues2);
        */
        /// 
        /// Step 4:
        /// Blend splatmaps from 2% either side of join
        /// 

        // Update part of splatmap to reflect the change in heights
        //_texturingTerrain.PartialSplatMap(terrainData, 0, 0, terrainData.alphamapHeight, (int)(terrainData.alphamapWidth * 0.1f));
        _textureGenerator.SplatMap(terrain);
        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);      // Assign the current terrains splatmap width and height into an array
        float[,,] splatMap2 = leftTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);  // Assign the topTerrains splatmap width and height into an array

        int numOfSplats = terrain.terrainData.terrainLayers.Length;                                 // Set number of splats available equal to the terrainlayers length (6)
        int xSplatMapPart = (int)(splatWidth * 0.02f);                                              // Blend the splatmaps from both terrains at 0.02 from join

        // Loop through all the X's and Y's in the splatmap
        for (int x = 0; x < xSplatMapPart; x++)
        {
            propotionOfOriginal = (Mathf.InverseLerp(0, xSplatMapPart, x) / 2) + 0.5f;  // InverseLerp gives a decimal 0.5 to 1 of where X is from xSplatMapPart(blend 2% splatmap height)
            propotionOfTarget = 1 - propotionOfOriginal;                                // Propotion of target and original always add up to 1

            for (int y = 0; y < splatHeight; y++)
            {
                for (int j = 0; j < numOfSplats; j++)
                {
                    // Update splatmap based on propotion of original side and other side of seam
                    splatMap1[y, x, j] = propotionOfOriginal * splatMap1[y, (int)(propotionOfOriginal * xSplatMapPart), j] +
                                         propotionOfTarget * splatMap2[y, splatWidth - 1 - (int)(propotionOfTarget * xSplatMapPart), j];

                    // Repeat for other splatmap
                    splatMap2[y, splatWidth - 1 - x, j] = propotionOfTarget * splatMap1[y, (int)(propotionOfTarget * xSplatMapPart), j] +
                                                          propotionOfOriginal * splatMap2[y, splatWidth - 1 - (int)(propotionOfOriginal * xSplatMapPart), j];
                }
            }
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        // Apply bottom terrain splatmap
        leftTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
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
                }
                map[x, y] = perlinNoiseHeight;
            }
        }

        float pointHeight;
        float pointHeightMultiplier;
        float closenessX;  //How close to edge X as a number between 0 and 1 
        float closenessY;
        float closeness;

        // Loop through height and width to calculate the edges of the terrains, to flatten edges
        // This is done to reduce the gaps between the terrains so the stitching is so drastic
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pointHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]);

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
                    pointHeightMultiplier = 0.5f + (closeness * 5f);
                }
                else
                {
                    pointHeightMultiplier = 1;
                }

                //map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]) ;

                map[x, y] = (pointHeight * pointHeightMultiplier) + (1 - pointHeightMultiplier) / 2;
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