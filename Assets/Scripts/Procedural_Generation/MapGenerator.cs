using System.Collections;
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



    private TerrainData defaultTD;          // Default terrain data, used to store the values for the initial terrain
    private GameObject nextTerrainChunk;    // Create next terrain chunk
    private Terrain _nextTerrain;           // Create next terrain
    private Terrain[] _terrains;            // Terrains array

    private Vector3 checkTerrainPosition;   // Check all positions of the terrains in the terrains array
    private Vector3 currentTerrainPosition; // Get current terrain position
    private Vector3[] surroundingTerrain;   // Used to set the surronding terrain position in endless terrain
    private Vector3[] neighbouringTerrain;  // Used to set the neighbouring terrain position in endless terrain
    private Vector2 terrainCentre;          // Calculate center of terrain

    public int numOfTerrains;                                           // Set number of terrains, used to change how many possible heightmap and splatmaps are used

    private int terrainCount = 1;                                       //Count of terrains created
    private List<Terrain> terrainsToBeUpdated = new List<Terrain>();       //List of terrains created that need updating
    private List<Terrain> terrainsUpdated = new List<Terrain>();        //List of terrains created and updated
    private List<Terrain> terrainsBeingUpdated = new List<Terrain>();    //List of terrains being that need updating
    private bool TerrainUpdateInProgress = false;                       //Flag to show if terrains are being updated

    private bool terrainFound;  // Check if terrain is found when checking around the current terrain the player is on

    private Terrain terrainLeft;
    private Terrain terrainRight;
    private Terrain terrainTop;
    private Terrain terrainBottom;

    private bool terrainLeftFound;
    private bool terrainRightFound;
    private bool terrainTopFound;
    private bool terrainBottomFound;

    private int xSize;
    private int ySize;

    private float propotionOfOriginal;
    private float propotionOfTarget;
    private float distToX1;
    private float distToX2;
    private float distToY1;
    private float distToY2;
    private float influenceX1;
    private float influenceX2;
    private float influenceY1;
    private float influenceY2;
    private float influenceX;
    private float influenceY;
    private float influenceHeightX1;
    private float influenceHeightX2;
    private float influenceHeightY1;
    private float influenceHeightY2;
    private float influenceXHeight;
    private float influenceYHeight;
    private float target;

    void Start()
    {
        // Creates default terrain data to equal current terrain data
        defaultTD = _terrain.terrainData;
        //Set initial values
        InitialVariables(defaultTD);

        // Update the appearance of the terrain
        StartCoroutine(UpdateTerrainCoroutine(_terrain));
        //Add initial terrain to updated list
        terrainsUpdated.Add(_terrain);

        // CreateEndlessTerrain is called to create new terrains around this one
        // CreateEndlessTerrain will loop through ensuring the player is always in a 3x3 terrain grid
        CreateEndlessTerrain(_terrain);

        // Loop through terrains to be updated
        foreach (Terrain terrain in terrainsToBeUpdated)
        {
            // Add terrain to list of terrains being updated (this prevents being added to the list while the list is being worked through)
            terrainsBeingUpdated.Add(terrain);
        }

        // Update terrains just created
        StartCoroutine(UpdateTerrains());
    }

    private void Update()
    {
        //If there are terrains to be updated and this is not started then trigger the update
        if (TerrainUpdateInProgress == false && terrainsToBeUpdated.Count > 0)
        {
            terrainsBeingUpdated.Clear();
            //clear out updated terrains from list of terrains to update
            foreach (Terrain _nextTerrain in terrainsUpdated)
            {
                terrainsToBeUpdated.Remove(_nextTerrain);
            }
            foreach (Terrain terrain in terrainsToBeUpdated)
            {
                terrainsBeingUpdated.Add(terrain);
            }
            //set in progress flag to true and start the update process
            TerrainUpdateInProgress = true;
            StartCoroutine(UpdateTerrains());
        }
    }

    public void CreateEndlessTerrain(Terrain centreTerrain)
    {
        //// Loops through each of the surrounding terrain positions, 
        //// checks if it exists and if not build one and add to list to update

        // Put all the terrains into an array to check against
        _terrains = Terrain.activeTerrains;

        currentTerrainPosition = centreTerrain.GetPosition();
        for (int x = 0; x < 8; x++)
        {
            //// This checks all existing terrains to see if the position matches the surrounding position being checked
            terrainFound = false;
            for (int i = 0; i < _terrains.Length; i++)
            {
                checkTerrainPosition = _terrains[i].GetPosition();
                if (checkTerrainPosition == surroundingTerrain[x] + currentTerrainPosition)
                {
                    terrainFound = true;
                }
            }

            //// This is where the new terrain is created
            // Skip to next iteration of For Loop if there is already a terrain in place
            if (terrainFound == true)
            {
                continue;
            }
            // Create new terrain object
            nextTerrainChunk = Terrain.CreateTerrainGameObject(new TerrainData());
            // Set position for new terrain
            nextTerrainChunk.transform.position = surroundingTerrain[x] + currentTerrainPosition;

            // Next terrain equals new terrain
            _nextTerrain = nextTerrainChunk.GetComponent<Terrain>();

            //// Set next terrain values equal to default terrain data
            _nextTerrain.terrainData.terrainLayers = defaultTD.terrainLayers;
            _nextTerrain.terrainData.heightmapResolution = defaultTD.heightmapResolution;
            _nextTerrain.terrainData.baseMapResolution = defaultTD.baseMapResolution;
            _nextTerrain.terrainData.size = defaultTD.size;
            _nextTerrain.terrainData.treePrototypes = defaultTD.treePrototypes;
            _nextTerrain.terrainData.detailPrototypes = defaultTD.detailPrototypes;
            _nextTerrain.terrainData.SetDetailResolution(defaultTD.detailResolution, defaultTD.detailResolutionPerPatch);

            //Add new terrain to the ToUpdate list for heights, splats, trees, grass etc.
            terrainsToBeUpdated.Add(_nextTerrain);
        }
    }

    public IEnumerator UpdateTerrains()
    {
        //// Work through list of terrains to update, updating 1 terrain at a time
        foreach (Terrain _nextTerrain in terrainsBeingUpdated)
        {
            if (terrainCount >= 9)
            {
                yield return StartCoroutine(UpdateTerrainCoroutine(_nextTerrain));
            }
            else
            {
                StartCoroutine(UpdateTerrainCoroutine(_nextTerrain));
            }
            //TerrainCount is used to run process without delay for the initial 3x3 terrains 
            //and then with delays using yield in Coroutine for terrains after that to maintain player performance
            terrainCount++;
            terrainsUpdated.Add(_nextTerrain);
        }
        //set in progress to false once all these terrains are updated
        TerrainUpdateInProgress = false;
    }

    public IEnumerator UpdateTerrainCoroutine(Terrain _nextTerrain)
    {
        //// Update all the details of the terrains, like height map, splat map and blending

        //Set initial height map
        if (terrainCount >= 9)
        {
            yield return StartCoroutine(SetPerlinNoise(_nextTerrain.terrainData, _nextTerrain.terrainData.heightmapWidth, _nextTerrain.terrainData.heightmapHeight, _noiseValues, terrainCentre, (int)(Random.value * 100)));
        }
        else
        {
            StartCoroutine(SetPerlinNoise(_nextTerrain.terrainData, _nextTerrain.terrainData.heightmapWidth, _nextTerrain.terrainData.heightmapHeight, _noiseValues, terrainCentre, (int)(Random.value * 100)));
        }

        // Check for neighbours
        terrainLeftFound = false;
        terrainRightFound = false;
        terrainTopFound = false;
        terrainBottomFound = false;

        //// Now check new terrain for neighbours
        foreach (Terrain _terrains in terrainsUpdated)
        {
            checkTerrainPosition = _terrains.GetPosition();
            if (checkTerrainPosition == neighbouringTerrain[0] + _nextTerrain.GetPosition())
            {
                terrainBottomFound = true;
                terrainBottom = _terrains;
            }
            else if (checkTerrainPosition == neighbouringTerrain[1] + _nextTerrain.GetPosition())
            {
                terrainLeftFound = true;
                terrainLeft = _terrains;
            }
            else if (checkTerrainPosition == neighbouringTerrain[2] + _nextTerrain.GetPosition())
            {
                terrainTopFound = true;
                terrainTop = _terrains;
            }
            else if (checkTerrainPosition == neighbouringTerrain[3] + _nextTerrain.GetPosition())
            {
                terrainRightFound = true;
                terrainRight = _terrains;
            }
        }

        //// Stitch terrains together
        if (terrainBottomFound == true)
        {
            StitchToBottom(_nextTerrain, terrainBottom, terrainLeftFound, terrainRightFound);
            // Add neighbour to set neighbours
            _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, _nextTerrain.topNeighbor, _nextTerrain.rightNeighbor, terrainBottom);
            // Add the new terrain to the existing terrain neighbours
            terrainBottom.SetNeighbors(terrainBottom.leftNeighbor, _nextTerrain, terrainBottom.rightNeighbor, terrainBottom.bottomNeighbor);
        }
        if (terrainLeftFound == true)
        {
            StitchToLeft(_nextTerrain, terrainLeft);
            // Add neighbour to set neighbours
            _nextTerrain.SetNeighbors(terrainLeft, _nextTerrain.topNeighbor, _nextTerrain.rightNeighbor, _nextTerrain.bottomNeighbor);
            // Add the new terrain to the existing terrain neighbours
            terrainLeft.SetNeighbors(terrainLeft.leftNeighbor, terrainLeft.topNeighbor, _nextTerrain, terrainLeft.bottomNeighbor);
        }
        if (terrainTopFound == true)
        {
            StitchToTop(_nextTerrain, terrainTop);
            // Add neighbour to set neighbours
            _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, terrainTop, _nextTerrain.rightNeighbor, _nextTerrain.bottomNeighbor);
            // Add the new terrain to the existing terrain neighbours
            terrainTop.SetNeighbors(terrainTop.leftNeighbor, terrainTop.topNeighbor, terrainTop.rightNeighbor, _nextTerrain);
        }
        if (terrainRightFound == true)
        {
            StitchToRight(_nextTerrain, terrainRight, terrainBottomFound, terrainTopFound);
            // Add neighbour to set neighbours
            _nextTerrain.SetNeighbors(_nextTerrain.leftNeighbor, _nextTerrain.topNeighbor, terrainRight, _nextTerrain.bottomNeighbor);
            // Add the new terrain to the existing terrain neighbours
            terrainRight.SetNeighbors(_nextTerrain, terrainRight.topNeighbor, terrainRight.rightNeighbor, terrainRight.bottomNeighbor);
        }

        if (terrainCount >= 9)
        {
            yield return null;
        }

        //// Stitch bottom right terrain
        if (terrainBottomFound == true && terrainRightFound == true)
        {
            xSize = (int)(_nextTerrain.terrainData.heightmapWidth * 0.1f) + 1;               // 10% of Width +1
            ySize = (int)(_nextTerrain.terrainData.heightmapHeight * 0.1f) + 1;              // 10% of height +1
            SmoothCorner(_nextTerrain, xSize, ySize, _nextTerrain.terrainData.heightmapWidth - xSize, 0);
            StitchBottomSeam(_nextTerrain, terrainBottom);
            StitchRightSeam(_nextTerrain, terrainRight);
            StitchHorizontalSeam(_nextTerrain, _nextTerrain.terrainData.heightmapWidth - xSize, ySize, xSize);
            StitchVericalSeam(_nextTerrain, _nextTerrain.terrainData.heightmapWidth - xSize, 0, ySize);
        }
        //// Stitch bottom left terrain
        if (terrainBottomFound == true && terrainLeftFound == true)
        {
            xSize = (int)(_nextTerrain.terrainData.heightmapWidth * 0.1f) + 1;               // 10% of Width +1
            ySize = (int)(_nextTerrain.terrainData.heightmapHeight * 0.1f) + 1;              // 10% of height +1
            SmoothCorner(_nextTerrain, xSize, ySize, 0, 0);
            StitchBottomSeam(_nextTerrain, terrainBottom);
            StitchLeftSeam(_nextTerrain, terrainLeft);
            StitchHorizontalSeam(_nextTerrain, 0, ySize, xSize);
            StitchVericalSeam(_nextTerrain, xSize, 0, ySize);
        }
        //// Stitch top right terrain
        if (terrainTopFound == true && terrainRightFound == true)
        {
            xSize = (int)(_nextTerrain.terrainData.heightmapWidth * 0.1f) + 1;               // 10% of Width +1
            ySize = (int)(_nextTerrain.terrainData.heightmapHeight * 0.1f) + 1;              // 10% of height +1
            SmoothCorner(_nextTerrain, xSize, ySize, _nextTerrain.terrainData.heightmapWidth - xSize, _nextTerrain.terrainData.heightmapHeight - ySize);
            StitchTopSeam(_nextTerrain, terrainTop);
            StitchRightSeam(_nextTerrain, terrainRight);
            StitchHorizontalSeam(_nextTerrain, _nextTerrain.terrainData.heightmapWidth - xSize, _nextTerrain.terrainData.heightmapHeight - ySize, xSize);
            StitchVericalSeam(_nextTerrain, _nextTerrain.terrainData.heightmapWidth - xSize, _nextTerrain.terrainData.heightmapHeight - ySize, ySize);
        }
        //// Stitch top left terrain
        if (terrainTopFound == true && terrainLeftFound == true)
        {
            xSize = (int)(_nextTerrain.terrainData.heightmapWidth * 0.1f) + 1;               // 10% of Width +1
            ySize = (int)(_nextTerrain.terrainData.heightmapHeight * 0.1f) + 1;              // 10% of height +1
            SmoothCorner(_nextTerrain, xSize, ySize, 0, _nextTerrain.terrainData.heightmapHeight - ySize);
            StitchTopSeam(_nextTerrain, terrainTop);
            StitchLeftSeam(_nextTerrain, terrainLeft);
            StitchHorizontalSeam(_nextTerrain, 0, _nextTerrain.terrainData.heightmapHeight - ySize, xSize);
            StitchVericalSeam(_nextTerrain, xSize, _nextTerrain.terrainData.heightmapHeight - ySize, ySize);
        }

        //Yield here to put in pause after smoothing and stitching before next piece of processing
        if (terrainCount >= 9)
        {
            yield return null;
        }

        //When doing normal run need to use yield to maintain order of updates and performance
        //when doing initial terrain generations need to do without yield so everything gets done before start of game
        // Update entire splatmap to reflect the change in heights
        if (terrainCount >= 9)
        {
            yield return StartCoroutine(_textureGenerator.SplatMap(_nextTerrain, terrainCount));
        }
        else
        {
            StartCoroutine(_textureGenerator.SplatMap(_nextTerrain, terrainCount));
        }

        //// Blend splatmap edges with neighbors if the terrains are found
        if (terrainBottomFound == true)
        {
            if (terrainCount >= 9)
            {
                yield return StartCoroutine(SplatBlendBottom(_nextTerrain, terrainBottom));
            }
            else
            {
                StartCoroutine(SplatBlendBottom(_nextTerrain, terrainBottom));
            }
        }
        if (terrainRightFound == true)
        {
            if (terrainCount >= 9)
            {
                yield return StartCoroutine(SplatBlendRight(_nextTerrain, terrainRight));
            }
            else
            {
                StartCoroutine(SplatBlendRight(_nextTerrain, terrainRight));
            }
        }
        if (terrainTopFound == true)
        {
            if (terrainCount >= 9)
            {
                yield return StartCoroutine(SplatBlendTop(_nextTerrain, terrainTop));
            }
            else
            {
                StartCoroutine(SplatBlendTop(_nextTerrain, terrainTop));
            }
        }
        if (terrainLeftFound == true)
        {
            if (terrainCount >= 9)
            {
                yield return StartCoroutine(SplatBlendLeft(_nextTerrain, terrainLeft));
            }
            else
            {
                StartCoroutine(SplatBlendLeft(_nextTerrain, terrainLeft));
            }
        }

        // Create tress across endless terrain
        if (terrainCount >= 9)
        {
            yield return StartCoroutine(_terrainAssetGenerator.CreateTrees(_nextTerrain, terrainCount));
        }
        else
        {
            StartCoroutine(_terrainAssetGenerator.CreateTrees(_nextTerrain, terrainCount));
        }

        // Create water across endless terrain
        _terrainAssetGenerator.CreateWater(_nextTerrain);

        // Create grass across endless terrain
        if (terrainCount >= 9)
        {
            yield return StartCoroutine(_terrainAssetGenerator.CreateGrass(_nextTerrain, terrainCount));
        }
        else
        {
            StartCoroutine(_terrainAssetGenerator.CreateGrass(_nextTerrain, terrainCount));
        }
    }

    /// <summary>
    /// Start stitch code section
    /// </summary>
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

        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of bottom 10% of heights to be smoothed
        int xSize = terrainData.heightmapWidth;                             // Full width of terrain
        int ySize = (int)(terrainData.heightmapHeight * 0.1f);              // 10% of height
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
                // propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, y);  // InverseLerp gives a decimal 0 to 1 of where Y is from 0 to blend limit
                propotionOfOriginal = Mathf.SmoothStep(0f, 1f, y / blendLimit); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
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
    }

    public void StitchToTop(Terrain terrain, Terrain topTerrain)
    {
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

        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of bottom 10% of heights to be smoothed
        int xSize = terrainData.heightmapWidth;                             // Full width of terrain
        int ySize = (int)(terrainData.heightmapHeight * 0.1f);              // 10% of height
        float[,] startValues = terrainData.GetHeights(0, terrainData.heightmapHeight - ySize, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
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
                // propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, ySize - 1 - y); // InverseLerp gives a decimal 0 to 1 of where Y is from 0 to blend limit, ysize-y used as y=0 is 100% original
                propotionOfOriginal = Mathf.SmoothStep(0f, 1f, (ySize - 1 - y) / blendLimit);   // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
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
    }

    public void StitchToRight(Terrain terrain, Terrain rightTerrain, bool bottomTerrainFound, bool topTerrainFound)
    {
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

        /// Step 2: 
        /// Blend 10% of heights in new terrain to the height of the first row of the new terrain
        /// First row of the new terrain is the same as bottom terrain
        /// 

        // Get current values of right 10% of widths to be smoothed
        int xSize = (int)(terrainData.heightmapWidth * 0.1f);                                                   // 10% of width
        int ySize = terrainData.heightmapHeight;                                                                // Full height
        float[,] startValues = terrainData.GetHeights(terrainData.heightmapWidth - xSize, 0, xSize, ySize);     // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                                                           // Create array for new heights

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

            // Variation value is based on height difference, big height differences give big values, so blending is over a bigger distance (max value 1)
            variationValue = Mathf.InverseLerp(0, 0.6f, heightDiff);

            // Blend limit is 75% to 100% of blend region (xSize - 1, needs -1 as counting starts at 0)
            blendLimit = (0.75f * (xSize - 1)) + (0.25f * (xSize - 1) * variationValue);

            for (int x = 0; x < xSize; x++)
            {
                // propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, xSize - 1 - x);  // InverseLerp gives a decimal 0 to 1 of where X is from 0 to blend limit
                propotionOfOriginal = Mathf.SmoothStep(0f, 1f, (xSize - 1 - x) / blendLimit);   // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line

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
                // propotionOfOriginal = Mathf.InverseLerp(0, blendLimit, x);   // InverseLerp gives a decimal 0 to 1 of where X is from 0 to blend limit
                propotionOfOriginal = Mathf.SmoothStep(0f, 1f, x / blendLimit); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line

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
    }

    /// <summary>
    /// Start of blending section of code
    /// </summary>
    public IEnumerator SplatBlendBottom(Terrain terrain, Terrain bottomTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);          // Put the current terrains splatmap width and height into an array
        float[,,] splatMap1End = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);       // Put the current terrains splatmap width and height into an array

        float[,,] splatMap2 = bottomTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);    // Put the bottom terrains splatmap width and height into an array
        float[,,] splatMap2End = bottomTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight); // Put the bottom terrains splatmap width and height into an array

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
                    splatMap1End[y, x, j] = propotionOfOriginal * splatMap1[(int)(propotionOfOriginal * ySplatMapPart), x, j] +
                                         propotionOfTarget * splatMap2[splatHeight - 1 - (int)(propotionOfTarget * ySplatMapPart), x, j];

                    // Repeat for splatmap on other side of seam
                    splatMap2End[splatHeight - 1 - y, x, j] = propotionOfTarget * splatMap1[(int)(propotionOfTarget * ySplatMapPart), x, j] +
                                                           propotionOfOriginal * splatMap2[splatHeight - 1 - (int)(propotionOfOriginal * ySplatMapPart), x, j];
                }
            }
            // Uses y/50 to break up the processing and maintain performance for player
            // will return to normal frame update every 50 y's
            if ((float)y / 50 == y / 50 && terrainCount >= 9)
            {
                yield return null;
            }
        }

        // Yield used before application of newly calculated splatmap
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1End);
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply bottom terrain splatmap
        bottomTerrain.terrainData.SetAlphamaps(0, 0, splatMap2End);
    }

    public IEnumerator SplatBlendRight(Terrain terrain, Terrain rightTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);      // Assign the current terrains splatmap width and height into an array
        float[,,] splatMap2 = rightTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight); // Assign the right terrains splatmap width and height into an array

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
            // Uses x/50 to break up the processing and maintain performance for player
            // will return to normal frame update every 50 x's
            if ((float)x / 50 == x / 50 && terrainCount >= 9)
            {
                yield return null;
            }
        }

        // Yield used before application of newly calculated splatmap
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply bottom terrain splatmap
        rightTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    public IEnumerator SplatBlendTop(Terrain terrain, Terrain topTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

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
            // Uses y/50 to break up the processing and maintain performance for player
            // will return to normal frame update every 50 y's
            if ((float)y / 50 == y / 50 && terrainCount >= 9)
            {
                yield return null;
            }
        }

        // Yield used before application of newly calculated splatmap
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply bottom terrain splatmap
        topTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    public IEnumerator SplatBlendLeft(Terrain terrain, Terrain leftTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Set splat width and height equal to alphamap values
        int splatWidth = terrainData.alphamapWidth;
        int splatHeight = terrainData.alphamapHeight;

        float[,,] splatMap1 = terrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);      // Assign the current terrains splatmap width and height into an array
        float[,,] splatMap2 = leftTerrain.terrainData.GetAlphamaps(0, 0, splatWidth, splatHeight);  // Assign the left terrains splatmap width and height into an array

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
                    // Update splatmap based on propotion of original side and other side of seam
                    splatMap1[y, x, j] = propotionOfOriginal * splatMap1[y, x, j] +
                                         propotionOfTarget * splatMap2[y, splatWidth - 1, j];

                    // Repeat for other splatmap
                    splatMap2[y, splatWidth - 1 - x, j] = propotionOfTarget * splatMap1[y, 0, j] +
                                                          propotionOfOriginal * splatMap2[y, splatWidth - 1 - x, j];
                }
            }
            // Uses x/50 to break up the processing and maintain performance for player
            // will return to normal frame update every 50 x's
            if ((float)x / 50 == x / 50 && terrainCount >= 9)
            {
                yield return null;
            }
        }

        // Yield used before application of newly calculated splatmap
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply terrain splatmap
        terrain.terrainData.SetAlphamaps(0, 0, splatMap1);
        if (terrainCount >= 9)
        {
            yield return null;
        }
        // Apply bottom terrain splatmap
        leftTerrain.terrainData.SetAlphamaps(0, 0, splatMap2);
    }

    /// <summary>
    /// Start of stitching section of code
    /// Smooths on the join itself
    /// </summary>
    public void StitchBottomSeam(Terrain terrain, Terrain terrainBottom)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(1, 0, terrainData.heightmapWidth - 2, 2);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[1, terrainData.heightmapWidth - 2];                        // Create array to store new heights for the end of the stitch (terrain)

        //// Start stitch values for bottom terrain, setting the yBase to be the max heightmap height of the bottom terrain -ySize (2% of the terrain height being stitched)
        float[,] startStitchValues2 = terrainBottom.terrainData.GetHeights(1, terrainBottom.terrainData.heightmapHeight - 2, terrainData.heightmapWidth - 2, 2);

        //// Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[1, terrainData.heightmapWidth - 2];

        for (int x = 0; x < terrainData.heightmapWidth - 2; x++)
        {
            endStitchValues1[0, x] = (startStitchValues1[1, x] + startStitchValues2[0, x]) / 2;
            endStitchValues2[0, x] = endStitchValues1[0, x];
        }
        //// Apply new heights to terrain
        terrainData.SetHeights(1, 0, endStitchValues1);
        terrainBottom.terrainData.SetHeights(1, terrainBottom.terrainData.heightmapHeight - 1, endStitchValues2);
    }

    public void StitchRightSeam(Terrain terrain, Terrain rightTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(terrainData.heightmapWidth - 2, 1, 2, terrainData.heightmapHeight - 2);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[terrainData.heightmapHeight - 2, 1];                        // Create array to store new heights for the end of the stitch (terrain)

        //// Start stitch values for right terrain, xbase and ybase both 0 as array starts at bottom left of the right terrain
        float[,] startStitchValues2 = rightTerrain.terrainData.GetHeights(0, 1, 2, terrainData.heightmapHeight - 2);

        //// Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[terrainData.heightmapHeight - 2, 1];

        for (int y = 0; y < terrainData.heightmapHeight - 2; y++)
        {
            endStitchValues1[y, 0] = (startStitchValues1[y, 0] + startStitchValues2[y, 1]) / 2;
            endStitchValues2[y, 0] = endStitchValues1[y, 0];
        }
        //// Apply new heights to terrain
        terrainData.SetHeights(terrainData.heightmapWidth - 1, 1, endStitchValues1);
        rightTerrain.terrainData.SetHeights(0, 1, endStitchValues2);
    }

    public void StitchLeftSeam(Terrain terrain, Terrain leftTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(0, 1, 2, terrainData.heightmapHeight - 2);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[terrainData.heightmapHeight - 2, 1];                        // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for left terrain, xbase and ybase both 0 as array starts at bottom right of the left terrain
        float[,] startStitchValues2 = leftTerrain.terrainData.GetHeights(leftTerrain.terrainData.heightmapWidth - 2, 1, 2, terrainData.heightmapHeight - 2);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[terrainData.heightmapHeight - 2, 1];

        for (int y = 0; y < terrainData.heightmapHeight - 2; y++)
        {
            endStitchValues1[y, 0] = (startStitchValues1[y, 1] + startStitchValues2[y, 0]) / 2;
            endStitchValues2[y, 0] = endStitchValues1[y, 0];
        }
        // Apply new heights to terrain
        terrainData.SetHeights(0, 1, endStitchValues1);
        leftTerrain.terrainData.SetHeights(leftTerrain.terrainData.heightmapWidth - 1, 1, endStitchValues2);
    }

    public void StitchTopSeam(Terrain terrain, Terrain topTerrain)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(1, terrainData.heightmapHeight - 2, terrainData.heightmapWidth - 2, 2); // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[1, terrainData.heightmapWidth - 2];                                                    // Create array to store new heights for the end of the stitch (terrain)

        // Start stitch values for top terrain, setting the yBase to be the max heightmap height of the top terrain -ySize (2% of the terrain height being stitched)
        float[,] startStitchValues2 = topTerrain.terrainData.GetHeights(1, 0, terrainData.heightmapWidth - 2, 2);

        // Store new heights for the end of the stitch (bottomTerrain)
        float[,] endStitchValues2 = new float[1, terrainData.heightmapWidth - 2];

        for (int x = 0; x < terrainData.heightmapWidth - 2; x++)
        {
            endStitchValues1[0, x] = (startStitchValues1[0, x] + startStitchValues2[1, x]) / 2;
            endStitchValues2[0, x] = endStitchValues1[0, x];
        }
        // Apply new heights to terrain
        terrainData.SetHeights(1, terrainData.heightmapHeight - 1, endStitchValues1);
        topTerrain.terrainData.SetHeights(1, 0, endStitchValues2);
    }

    /// <summary>
    /// Stitching of corners to rest of the terrain along the internal horizontal join
    /// </summary>
    public void StitchHorizontalSeam(Terrain terrain, int stitchStart, int stitchHeight, int stitchDistance)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(stitchStart, stitchHeight - 1, stitchDistance, 3);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[1, stitchDistance];                        // Create array to store new heights for the end of the stitch (terrain)

        /// Loop through stitch distance, taking the average heights from either side of the join and setting a new join height
        for (int x = 0; x < stitchDistance; x++)
        {
            endStitchValues1[0, x] = (startStitchValues1[0, x] + startStitchValues1[2, x]) / 2;
        }
        //// Apply new heights to terrain
        terrainData.SetHeights(stitchStart, stitchHeight, endStitchValues1);
    }

    /// <summary>
    /// Stitching of corners to rest of the terrain along the internal vertical join
    /// </summary>
    public void StitchVericalSeam(Terrain terrain, int stitchWidth, int stitchStart, int stitchDistance)
    {
        TerrainData terrainData = terrain.terrainData;

        float[,] startStitchValues1 = terrainData.GetHeights(stitchWidth - 1, stitchStart, 3, stitchDistance);   // Put current stitch values to into an array (terrain)
        float[,] endStitchValues1 = new float[stitchDistance, 1];                        // Create array to store new heights for the end of the stitch (terrain)

        /// Loop through stitch distance, taking the average heights from either side of the join and setting a new join height
        for (int y = 0; y < stitchDistance; y++)
        {
            endStitchValues1[y, 0] = (startStitchValues1[y, 0] + startStitchValues1[y, 2]) / 2;
        }
        //// Apply new heights to terrain
        terrainData.SetHeights(stitchWidth, stitchStart, endStitchValues1);
    }

    /// <summary>
    /// Smooths corner sections of terrains where terrain is joined to 2 other terrains at the same time
    /// Normal smoothing joins one against another and cannot cope with the additional terrain
    /// </summary>
    public void SmoothCorner(Terrain terrain, int xSize, int ySize, int left, int bottom)
    {
        TerrainData terrainData = terrain.terrainData;

        // Get current values of bottom 10% of heights to be smoothed
        float[,] startValues = terrainData.GetHeights(left, bottom, xSize, ySize);  // Put current heights into array (x and y goes in reverse)
        float[,] endValues = new float[ySize, xSize];                               // Create array for new heights

        // Loop through the x and y's in the corner being smoothed
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //Distances to edges
                distToX1 = y;               //Bottom of corner square
                distToX2 = ySize - 1 - y;   //Top of corner square
                distToY1 = x;               //Left of corner square
                distToY2 = xSize - 1 - x;   //Right of corner square

                ////Going left to right workout smoothing based on how close to edge
                //Work out influence from left side
                target = startValues[y, 0];
                influenceY1 = 1 - Mathf.SmoothStep(0f, 1f, distToY1 / (xSize)); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
                influenceHeightY1 = target * influenceY1;
                
                //Work out influence from right side
                target = startValues[y, xSize - 1];
                influenceY2 = 1 - Mathf.SmoothStep(0f, 1f, distToY2 / (xSize)); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
                influenceHeightY2 = target * influenceY2;

                //Work out new height from left to right
                //If influence is more than 100% limit to that, if less then add some of original height to calculation
                if (influenceY1 + influenceY2 >= 1)
                {
                    influenceYHeight = (influenceHeightY1 + influenceHeightY2) / (influenceY1 + influenceY2);
                }
                else
                {
                    influenceYHeight = (influenceHeightY1 + influenceHeightY2) + startValues[y, x] / (1 - influenceY1 - influenceY2);
                }

                ////Going bottom to top workout smoothing based on how close to edge
                //Work out influence from bottom side
                target = startValues[0, x];
                influenceX1 = 1 - Mathf.SmoothStep(0f, 1f, distToX1 / (ySize)); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
                influenceHeightX1 = target * influenceX1;
                
                //Work out influence from top side
                target = startValues[ySize - 1, x];
                influenceX2 = 1 - Mathf.SmoothStep(0f, 1f, distToX2 / (ySize)); // SmoothStep interpolates between 0 and 1 smoothing at the limits giving a smooth curve line
                influenceHeightX2 = target * influenceX2;

                //Work out new height from bottom to top
                //If influence is more than 100% limit to that, if less then add some of original height to calculation
                if (influenceX1 + influenceX2 >= 1)
                {
                    influenceXHeight = (influenceHeightX1 + influenceHeightX2) / (influenceX1 + influenceX2);
                }
                else
                {
                    influenceXHeight = (influenceHeightX1 + influenceHeightX2) + startValues[y, x] / (1 - influenceX1 - influenceX2);
                }

                //Work out influence from top/bottom and left/right 
                influenceY = Mathf.Max(influenceY1, influenceY2);
                influenceX = Mathf.Max(influenceX1, influenceX2);
                //Apply proportion of each height, but use to the power of 10 to make it much stronger near closest edge
                endValues[y, x] = ((influenceXHeight * Mathf.Pow(influenceX, 10)) + (influenceYHeight * Mathf.Pow(influenceY, 10))) / (Mathf.Pow(influenceX, 10) + Mathf.Pow(influenceY, 10));
            }
        }
        //Update heights back to heightmap
        terrainData.SetHeights(left, bottom, endValues);
    }

    public IEnumerator SetPerlinNoise(TerrainData _terrainData, int width, int height, NoiseValues _noiseValues, Vector2 sampleCentre, int seed)
    {
        float[,] map = new float[width, height];

        // Randomise the seed
        System.Random randomSeed = new System.Random(seed);

        // Set octaves
        Vector2[] setOctaves = new Vector2[_noiseValues.octaves];

        // Set values for max possible height, amplitude and frequency
        float maxPossibleHeight = 0;
        float amplitude = 1;

        // Loop through octaves
        for (int i = 0; i < _noiseValues.octaves; i++)
        {
            // Randomise seed across the x and y axis between the values -100000, 100000
            float sampleX = randomSeed.Next(-100000, 100000) + sampleCentre.x;
            float sampleY = randomSeed.Next(-100000, 100000) + sampleCentre.y;
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
                float frequency = 1;

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
            if ((float)y / 20 == y / 20 && terrainCount >= 9)
            {
                yield return null;
            }
        }

        float pointHeight;
        float pointHeightMultiplier;
        float closenessX;  //How close to edge X as a number between 0 and 1 
        float closenessY;
        float closeness;

        // Loop through height and width to calculate the edges of the terrains, to flatten edges
        // This is done to reduce the gaps between the terrains so the stitching is not so drastic
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
        if (terrainCount >= 9)
        {
            yield return null;
        }
        _terrainData.SetHeights(0, 0, map);
    }

    /// <summary>
    /// Inititial variables used to calculate the centre of the terrain,
    /// set the surrounding terrains using the default terrain data and
    /// set the positions for the neighbouring terrains using the default terrain data
    /// </summary>
    private void InitialVariables(TerrainData defaultTD)
    {
        // Calculate center of terrain
        terrainCentre = new Vector2(_terrain.terrainData.size.x / 2, _terrain.terrainData.size.z / 2);

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
        neighbouringTerrain[3] = new Vector3(defaultTD.size.x, 0, 0);  // Right terrain
    }
}

[System.Serializable]
public class NoiseValues
{
    public float scale = 250;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
 
    // Function to declare validated values 
    public void ValidateValues()
    {
        // Max function used to choose whichever is greater, so if the scale is less than 0.01 then the value will be set to 0.01
        // The max function will be apply the same functions to octaves and lacunarity
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);   // Clamp persistance between 0 and 1
    }
}